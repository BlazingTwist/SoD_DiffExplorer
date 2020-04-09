using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace SoD_DiffExplorer.csutils
{
	class AssetToolUtils
	{
		public ClassDatabasePackage classPackage;

		public List<AssetTypeValueField> GetMonobehaviourData(Stream stream, List<string> targetPath) {
			List<KeyValuePair<string, string>> convertedPath = targetPath.Select(value => {
				string[] split = value.Split(':');
				return new KeyValuePair<string, string>(split[0], split[1]);
			}).ToList();
			List<AssetTypeValueField> result = new List<AssetTypeValueField>();

			classPackage = new ClassDatabasePackage();
			Console.WriteLine("Reading classdata...");
			using(AssetsFileReader reader = new AssetsFileReader(new FileStream("classdata.tpk", FileMode.Open, FileAccess.Read, FileShare.Read))) {
				classPackage.Read(reader);
			}

			AssetBundleFile file = new AssetBundleFile();
			Console.WriteLine("Reading bundleFileStream...");
			file.Read(new AssetsFileReader(stream), true);
			file.reader.Position = 0;
			Stream memoryStream = new MemoryStream();
			Console.WriteLine("Unpacking bundleFile...");
			file.Unpack(file.reader, new AssetsFileWriter(memoryStream));
			memoryStream.Position = 0;
			file.Close();
			file = new AssetBundleFile();
			file.Read(new AssetsFileReader(memoryStream), false);

			byte[] assetData = BundleHelper.LoadAssetDataFromBundle(file, 0);
			MemoryStream mainStream = new MemoryStream(assetData);
			string mainName = file.bundleInf6.dirInf[0].name;
			AssetsFileInstance fileInstance = new AssetsFileInstance(mainStream, mainName, "");
			file.Close();

			fileInstance.table.GenerateQuickLookupTree();
			ClassDatabaseFile classDBFile = LoadClassDatabaseFromPackage(fileInstance.file.typeTree.unityVersion);

			if(classDBFile == null) {
				Console.WriteLine("classDatabaseFile was null? Okay, that's probably bad. Continuing anyway...");
			}

			Console.WriteLine("Searching for targetPath in bundlefile...");
			foreach(AssetFileInfoEx info in fileInstance.table.assetFileInfo) {
				ClassDatabaseType type = AssetHelper.FindAssetClassByID(classDBFile, info.curFileType);
				if(type == null) {
					continue;
				}
				string typeName = type.name.GetString(classDBFile);
				if(typeName != "MonoBehaviour") {
					//not what we're looking for.
					continue;
				}
				AssetTypeValueField baseField = GetATI(fileInstance.file, info, classDBFile).GetBaseField();
				AssetTypeValueField targetField = GetFieldAtPath(baseField, convertedPath);
				if(targetField != null) {
					result.Add(targetField);
					Console.WriteLine("found target field.");
				}
			}

			mainStream.Close();
			stream.Close();

			Console.WriteLine("found all targetFiles in bundleFile. (" + result.Count + ")");
			return result;
		}

		private AssetTypeValueField GetFieldAtPath(AssetTypeValueField baseField, List<KeyValuePair<string, string>> targetPath) {
			if(baseField == null) {
				return null;
			}
			int pathProgression = 0;
			while(baseField.GetChildrenCount() > 0) {
				KeyValuePair<string, string> pathSection = targetPath[pathProgression];
				List<AssetTypeValueField> found = baseField.GetChildrenList().Where(field => {
					return field.GetName() == pathSection.Key && field.GetFieldType() == pathSection.Value;
				}).ToList();
				if(found.Count > 1) {
					Console.WriteLine("Error: targetPath did not yield a uniquely identifyable result!");
					Console.WriteLine("\tReached pathstep " + pathProgression + " = " + pathSection.Key + " : " + pathSection.Value);
					Console.WriteLine("found:");
					found.ForEach(item => PrintFieldRecursively(item));
					return null;
				} else if(found.Count == 1) {
					pathProgression++;
					if(pathProgression == targetPath.Count) {
						return found[0];
					}
					baseField = found[0];
				} else {
					//unable to find path
					return null;
				}
			}
			return null;
		}

		private void PrintFieldRecursively(AssetTypeValueField field, int depth = 0) {
			if(field == null) {
				Console.WriteLine("\tnull");
				return;
			}
			Console.WriteLine(new string('\t', depth) + FieldToString(field));

			if(field.GetChildrenCount() > 0) {
				foreach(AssetTypeValueField child in field.GetChildrenList()) {
					PrintFieldRecursively(child, depth + 1);
				}
			}
		}

		private string FieldToString(AssetTypeValueField field) {
			string result = field.GetName() + " : " + field.GetFieldType();
			if(field.GetValue() != null) {
				result += " (" + field.GetValue().AsString() + ")";
			}
			return result;
		}

		private ClassDatabaseFile LoadClassDatabaseFromPackage(string version) {
			if(version.StartsWith("U")) {
				version = version.Substring(1);
			}
			for(int i = 0; i < classPackage.files.Length; i++) {
				ClassDatabaseFile file = classPackage.files[i];
				for(int j = 0; j < file.header.unityVersions.Length; j++) {
					string unityVersion = file.header.unityVersions[j];
					if(WildcardMatches(version, unityVersion)) {
						return file;
					}
				}
			}
			return null;
		}

		private bool WildcardMatches(string test, string pattern) {
			return Regex.IsMatch(test, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
		}

		private AssetTypeInstance GetATI(AssetsFile file, AssetFileInfoEx info, ClassDatabaseFile classFile) {
			ushort scriptIndex = file.typeTree.unity5Types[info.curFileTypeOrIndex].scriptIndex;
			uint fixedId = info.curFileType;
			if(fixedId == 0xf1) {   //AudioMixerController
				fixedId = 0xf0;     //AudioMixer
			} else if(fixedId == 0xf3) {    //AudioMixerGroupController
				fixedId = 0x111;        //AudioMixerGroup
			} else if(fixedId == 0xf5) {    //AudioMixerSnapshotController
				fixedId = 0x110;        //AudioMixerSnapshot
			}

			bool hasTypeTree = file.typeTree.hasTypeTree;
			AssetTypeTemplateField baseField = new AssetTypeTemplateField();
			if(hasTypeTree) {
				baseField.From0D(file.typeTree.unity5Types.First(t => (t.classId == fixedId || t.classId == info.curFileType) && t.scriptIndex == scriptIndex), 0);
			} else {
				baseField.FromClassDatabase(classFile, AssetHelper.FindAssetClassByID(classFile, fixedId), 0);
			}
			return new AssetTypeInstance(baseField, file.reader, info.absoluteFilePos);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace SoD_DiffExplorer.csutils
{
	public class AssetFile
	{
		public AssetsFileInstance fileInstance;
		public ClassDatabaseFile classDBFile;

		public AssetFile(AssetsFileInstance file, ClassDatabaseFile classDBFile) {
			this.fileInstance = file;
			this.classDBFile = classDBFile;
		}
	}

	class AssetToolUtils
	{
		public ClassDatabasePackage classPackage;
		public List<MemoryStream> activeStreams = new List<MemoryStream>();
		public AssetBundleFile activeBunldeFile = null;

		public List<AssetFile> BuildAssetsFileInstance(Stream stream) {
			List<AssetFile> result = new List<AssetFile>();

			classPackage = new ClassDatabasePackage();
			Console.WriteLine("Reading classdata...");
			using(AssetsFileReader reader = new AssetsFileReader(new FileStream("classdata.tpk", FileMode.Open, FileAccess.Read, FileShare.Read))) {
				classPackage.Read(reader);
			}

			AssetBundleFile file = new AssetBundleFile();
			activeBunldeFile = file;
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

			for(int i = 0; i < file.bundleInf6.dirInf.Length; i++) {
				try {
					if(file.IsAssetsFile(file.reader, file.bundleInf6.dirInf[i])) {
						byte[] assetData = BundleHelper.LoadAssetDataFromBundle(file, i);
						MemoryStream mainStream = new MemoryStream(assetData);
						activeStreams.Add(mainStream);

						string mainName = file.bundleInf6.dirInf[i].name;
						AssetsFileInstance fileInstance = new AssetsFileInstance(mainStream, mainName, "");
						ClassDatabaseFile classDBFile = LoadClassDatabaseFromPackage(fileInstance.file.typeTree.unityVersion);
						if(classDBFile == null) {
							Console.WriteLine("classDatabaseFile was null? Okay, that's probably bad. Continuing anyway...");
						}
						result.Add(new AssetFile(fileInstance, classDBFile));
					}
				}catch(Exception) {
					//guess it's not an assetsFile then?
				}
			}

			return result;
		}

		public void CloseActiveStreams() {
			for(int i = 0; i < activeStreams.Count; i++) {
				activeStreams[i].Dispose();
				activeStreams[i] = null;
			}
			activeStreams = new List<MemoryStream>();

			if(activeBunldeFile != null) {
				activeBunldeFile.Close();
				activeBunldeFile = null;
			}
		}

		public List<AssetTypeValueField> GetFieldAtPath(AssetFile file, AssetTypeValueField baseField, string[] customPaths) {
			List<AssetTypeValueField> currentScope = new List<AssetTypeValueField>{baseField};
			foreach(string customPath in customPaths) {
				if(customPath.Contains('/')) {
					string[] referencePaths = customPath.Split('/');
					for(int i = 0; i < referencePaths.Length; i++) {
						List<AssetTypeValueField> found = new List<AssetTypeValueField>();
						if(i == 0) {
							foreach(AssetTypeValueField field in currentScope.Where(field => field.GetChildrenCount() > 0)) {
								found.AddRange(field.GetChildrenList().Where(child => child.GetName() == referencePaths[i]));
							}
						} else {
							foreach(AssetTypeValueField field in currentScope.Where(field => field.GetValue() != null)) {
								AssetFileInfoEx referenceInfo = file.fileInstance.table.GetAssetInfo(field.GetValue().AsInt64());
								if(referenceInfo == null) {
									Console.WriteLine("could not find referenceInfo for pathID = (" + field.GetValue().AsInt64() + "), that's probably bad, skipping");
									continue;
								}
								/*Console.WriteLine("loading fileReference at pathID = " + field.GetValue().AsInt64());
								List<AssetTypeValueField> fields = GetATI(file, referenceInfo).baseFields.ToList();
								foreach(AssetTypeValueField foundField in fields) {
									Console.WriteLine("======== found field ========");
									PrintFieldRecursively(foundField);
									Console.ReadKey(true);

									Console.WriteLine("fieldName = " + foundField.GetName() + " | looking for: " + referencePaths[i]);

									if(foundField.GetName() == referencePaths[i]) {
										found.Add(foundField);
									}
								}*/
								found.AddRange(GetATI(file, referenceInfo).baseFields.Where(field => field.GetName() == referencePaths[i]));
							}
						}
						currentScope = found;
					}
				} else {
					List<AssetTypeValueField> found = new List<AssetTypeValueField>();
					foreach(AssetTypeValueField field in currentScope.Where(field => field.GetChildrenCount() > 0)) {
						found.AddRange(field.GetChildrenList().Where(child => child.GetName() == customPath));
					}
					currentScope = found;
				}
			}
			return currentScope;
		}

		public bool IsMatchingPathConstraints(AssetFile file, AssetTypeValueField baseField, List<string> pathConstraints) {
			return pathConstraints.All(path => GetFieldAtPath(file, baseField, path.Split(':')).Count > 0);
		}

		public void PrintFieldRecursively(AssetTypeValueField field, int depth = 0) {
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

		public void PrintFieldRecursively(StreamWriter writer, AssetTypeValueField field, int depth = 0) {
			if(field == null) {
				writer.WriteLine("\tnull");
				return;
			}
			writer.WriteLine(new string('\t', depth) + FieldToString(field));

			if(field.GetChildrenCount() > 0) {
				foreach(AssetTypeValueField child in field.GetChildrenList()) {
					PrintFieldRecursively(writer, child, depth + 1);
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

		public AssetTypeInstance GetATI(AssetFile file, AssetFileInfoEx info) {
			ushort scriptIndex = file.fileInstance.file.typeTree.unity5Types[info.curFileTypeOrIndex].scriptIndex;
			uint fixedId = info.curFileType;
			if(fixedId == 0xf1) {   //AudioMixerController
				fixedId = 0xf0;     //AudioMixer
			} else if(fixedId == 0xf3) {    //AudioMixerGroupController
				fixedId = 0x111;        //AudioMixerGroup
			} else if(fixedId == 0xf5) {    //AudioMixerSnapshotController
				fixedId = 0x110;        //AudioMixerSnapshot
			}

			bool hasTypeTree = file.fileInstance.file.typeTree.hasTypeTree;
			AssetTypeTemplateField baseField = new AssetTypeTemplateField();
			if(hasTypeTree) {
				baseField.From0D(file.fileInstance.file.typeTree.unity5Types.First(t => (t.classId == fixedId || t.classId == info.curFileType) && t.scriptIndex == scriptIndex), 0);
			} else {
				baseField.FromClassDatabase(file.classDBFile, AssetHelper.FindAssetClassByID(file.classDBFile, fixedId), 0);
			}
			return new AssetTypeInstance(baseField, file.fileInstance.file.reader, info.absoluteFilePos);
		}
	}
}

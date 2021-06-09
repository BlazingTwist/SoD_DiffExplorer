using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace SoD_DiffExplorer.utils {
	public class AssetFile {
		public readonly AssetsFileInstance fileInstance;
		public readonly ClassDatabaseFile classDBFile;

		public AssetFile(AssetsFileInstance file, ClassDatabaseFile classDBFile) {
			fileInstance = file;
			this.classDBFile = classDBFile;
		}
	}

	public class AssetToolUtils {
		private ClassDatabasePackage classPackage;
		private List<MemoryStream> activeStreams = new List<MemoryStream>();
		private AssetBundleFile activeBundleFile;

		public IEnumerable<AssetFile> BuildAssetsFileInstance(Stream stream) {
			List<AssetFile> result = new List<AssetFile>();

			classPackage = new ClassDatabasePackage();
			Console.WriteLine("Reading class-data...");
			using (var reader = new AssetsFileReader(new FileStream("classdata.tpk", FileMode.Open, FileAccess.Read, FileShare.Read))) {
				classPackage.Read(reader);
			}

			var file = new AssetBundleFile();
			activeBundleFile = file;
			Console.WriteLine("Reading bundleFileStream...");
			file.Read(new AssetsFileReader(stream), true);
			file.reader.Position = 0;
			Stream memoryStream = new MemoryStream();
			Console.WriteLine("Unpacking bundleFile...");
			file.Unpack(file.reader, new AssetsFileWriter(memoryStream));
			memoryStream.Position = 0;
			file.Close();
			file = new AssetBundleFile();
			file.Read(new AssetsFileReader(memoryStream));

			Console.WriteLine("file.bundleInf6.dirInf.Length: " + file.bundleInf6.dirInf.Length);

			for (int i = 0; i < file.bundleInf6.dirInf.Length; i++) {
				try {
					if (file.IsAssetsFile(file.reader, file.bundleInf6.dirInf[i])) {
						byte[] assetData = BundleHelper.LoadAssetDataFromBundle(file, i);
						var mainStream = new MemoryStream(assetData);
						activeStreams.Add(mainStream);

						string mainName = file.bundleInf6.dirInf[i].name;
						var fileInstance = new AssetsFileInstance(mainStream, mainName, "");
						ClassDatabaseFile classDBFile = LoadClassDatabaseFromPackage(fileInstance.file.typeTree.unityVersion);
						if (classDBFile == null) {
							Console.WriteLine("classDatabaseFile was null? Okay, that's probably bad. Continuing anyway...");
						}

						result.Add(new AssetFile(fileInstance, classDBFile));
					}
				} catch (Exception e) {
					Console.WriteLine("caught exception while reading AssetsFile: " + e);
					//guess it's not an assetsFile then?
				}
			}

			Console.WriteLine("found " + result.Count + " AssetFiles");

			return result;
		}

		public void CloseActiveStreams() {
			for (int i = 0; i < activeStreams.Count; i++) {
				activeStreams[i].Dispose();
				activeStreams[i] = null;
			}

			activeStreams = new List<MemoryStream>();

			if (activeBundleFile != null) {
				activeBundleFile.Close();
				activeBundleFile = null;
			}
		}

		public static List<AssetTypeValueField> GetFieldAtPath(AssetFile file, AssetTypeValueField baseField, IEnumerable<string> customPaths) {
			List<AssetTypeValueField> currentScope = new List<AssetTypeValueField> { baseField };
			foreach (string customPath in customPaths) {
				if (customPath.Contains('/')) {
					string[] referencePaths = customPath.Split('/');
					for (int i = 0; i < referencePaths.Length; i++) {
						List<AssetTypeValueField> found = new List<AssetTypeValueField>();
						if (i == 0) {
							foreach (AssetTypeValueField field in currentScope.Where(field => field.GetChildrenCount() > 0)) {
								found.AddRange(field.GetChildrenList().Where(child => child.GetName() == referencePaths[i]));
							}
						} else {
							foreach (AssetTypeValueField field in currentScope.Where(field => field.GetValue() != null)) {
								AssetFileInfoEx referenceInfo = file.fileInstance.table.GetAssetInfo(field.GetValue().AsInt64());
								if (referenceInfo == null) {
									Console.WriteLine("could not find referenceInfo for pathID = (" + field.GetValue().AsInt64() +
											"), that's probably bad, skipping");
									continue;
								}

								found.AddRange(GetATI(file, referenceInfo).baseFields.Where(referenceField => referenceField.GetName() == referencePaths[i]));
							}
						}

						currentScope = found;
					}
				} else {
					List<AssetTypeValueField> found = new List<AssetTypeValueField>();
					foreach (AssetTypeValueField field in currentScope.Where(field => field.GetChildrenCount() > 0)) {
						found.AddRange(field.GetChildrenList().Where(child => child.GetName() == customPath));
					}

					currentScope = found;
				}
			}

			return currentScope;
		}

		public static bool IsMatchingPathConstraints(AssetFile file, AssetTypeValueField baseField, IEnumerable<string> pathConstraints) {
			return pathConstraints.All(path => GetFieldAtPath(file, baseField, path.Split(':')).Count > 0);
		}

		private ClassDatabaseFile LoadClassDatabaseFromPackage(string version) {
			if (version.StartsWith("U")) {
				version = version.Substring(1);
			}

			return (from file in classPackage.files
			        from unityVersion in file.header.unityVersions
			        where WildcardMatches(version, unityVersion)
			        select file)
					.FirstOrDefault();
		}

		private static bool WildcardMatches(string test, string pattern) {
			return Regex.IsMatch(test, "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
		}

		public static AssetTypeInstance GetATI(AssetFile file, AssetFileInfoEx info) {
			ushort scriptIndex = file.fileInstance.file.typeTree.unity5Types[info.curFileTypeOrIndex].scriptIndex;
			uint fixedId = info.curFileType;
			if (fixedId == 0xf1) { //AudioMixerController
				fixedId = 0xf0; //AudioMixer
			} else if (fixedId == 0xf3) { //AudioMixerGroupController
				fixedId = 0x111; //AudioMixerGroup
			} else if (fixedId == 0xf5) { //AudioMixerSnapshotController
				fixedId = 0x110; //AudioMixerSnapshot
			}

			bool hasTypeTree = file.fileInstance.file.typeTree.hasTypeTree;
			var baseField = new AssetTypeTemplateField();
			if (hasTypeTree) {
				baseField.From0D(
						file.fileInstance.file.typeTree.unity5Types.First(t =>
								(t.classId == fixedId || t.classId == info.curFileType) && t.scriptIndex == scriptIndex), 0);
			} else {
				baseField.FromClassDatabase(file.classDBFile, AssetHelper.FindAssetClassByID(file.classDBFile, fixedId), 0);
			}

			return new AssetTypeInstance(baseField, file.fileInstance.file.reader, info.absoluteFilePos);
		}
	}
}
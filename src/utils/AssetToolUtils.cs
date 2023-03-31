using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace SoD_DiffExplorer.utils {

	public class AssetToolUtils {

		public readonly AssetsManager assetsManager;

		public AssetToolUtils() {
			assetsManager = new AssetsManager();
			assetsManager.LoadClassPackage("classdata.tpk");
		}

		public List<AssetsFileInstance> BuildAssetsFileInstance(Stream stream) {
			BundleFileInstance bundleFile = assetsManager.LoadBundleFile(stream, "null");
			Console.WriteLine($"reading bundle. engineVersion '${bundleFile.file.Header.EngineVersion}' signature '${bundleFile.file.Header.Signature}'");
			Console.WriteLine($"bundleName '${bundleFile.name}' num loaded files ${bundleFile.loadedAssetsFiles.Count}");
			return bundleFile.loadedAssetsFiles.Count <= 0
					? new List<AssetsFileInstance> { assetsManager.LoadAssetsFileFromBundle(bundleFile, 0) }
					: bundleFile.loadedAssetsFiles;
		}

		public void CloseActiveStreams() {
			assetsManager.UnloadAll();
		}

		public List<AssetTypeValueField> GetFieldAtPath(AssetsFileInstance file, AssetTypeValueField baseField, IEnumerable<string> customPaths) {
			List<AssetTypeValueField> currentScope = new List<AssetTypeValueField> { baseField };
			foreach (string customPath in customPaths) {
				if (customPath.Contains('/')) {
					string[] referencePaths = customPath.Split('/');
					for (int i = 0; i < referencePaths.Length; i++) {
						List<AssetTypeValueField> found = new List<AssetTypeValueField>();
						if (i == 0) {
							foreach (AssetTypeValueField field in currentScope.Where(field => field.Children.Count > 0)) {
								found.AddRange(field.Children.Where(child => child.FieldName == referencePaths[i].Split('=')[0]));
							}
						} else {
							foreach (AssetTypeValueField field in currentScope.Where(field => field.Value?.AsObject != null)) {
								long pathID = field.Value.AsLong;
								AssetTypeValueField referenceField = assetsManager.GetBaseField(file, pathID);
								if (referenceField == null) {
									Console.WriteLine("could not find referenceInfo for pathID = (" + pathID + "), that's probably bad, skipping");
									continue;
								}

								string searchFieldName = referencePaths[i].Split('=')[0];
								if (referenceField.FieldName != searchFieldName) {
									Console.WriteLine(
											$"searching for name: (${searchFieldName}) but found (${referenceField.FieldName}), that's probably bad, skipping");
									continue;
								}

								found.Add(referenceField);
							}
						}

						if (customPath.Contains('=')) {
							string targetValue = customPath.Split('=')[1];
							found.RemoveAll(field => field.Value?.AsString != targetValue);
						}
						currentScope = found;
					}
				} else {
					List<AssetTypeValueField> found = new List<AssetTypeValueField>();
					foreach (AssetTypeValueField field in currentScope.Where(field => field.Children.Count > 0)) {
						found.AddRange(field.Children.Where(child => child.FieldName == customPath.Split('=')[0]));
					}

					if (customPath.Contains('=')) {
						string targetValue = customPath.Split('=')[1];
						found.RemoveAll(field => field.Value?.AsString != targetValue);
					}
					currentScope = found;
				}
			}

			return currentScope;
		}

		public bool IsMatchingPathConstraints(AssetsFileInstance file, AssetTypeValueField baseField, string pathConstraint) {
			return IsMatchingPathConstraints(file, baseField, new[] { pathConstraint });
		}

		public bool IsMatchingPathConstraints(AssetsFileInstance file, AssetTypeValueField baseField, IEnumerable<string> pathConstraints) {
			return pathConstraints.All(path => GetFieldAtPath(file, baseField, path.Split(':')).Count > 0);
		}

	}

}

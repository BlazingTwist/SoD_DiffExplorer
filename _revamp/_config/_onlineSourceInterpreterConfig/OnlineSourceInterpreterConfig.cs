using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using AssetsTools.NET.Extra;
using AssetsTools.NET;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class OnlineSourceInterpreterConfig : YamlObject, IMenuObject
	{
		public string configPath = null;
		public List<string> pathConstraints = null;
		public IMappingValue mapConfigBy = null;
		public BetterDict<string, RegexTranslator> regexTranslator = null;
		public BetterDict<string, BetterDict<string, string>> typeTranslationDict = null;
		public BetterDict<string, string> outputNameToTypeDict = null;
		public List<OnlineInterpreterFilter> configFilter = null;

		public Dictionary<string, Dictionary<string, List<string>>> BuildXMLContent(Queue<string> fileUrls) {
			Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();

			foreach(string fileUrl in fileUrls) {
				Console.WriteLine("loading xml data from url: " + fileUrl);
				XDocument document = XMLUtils.LoadDocumentFromURL(fileUrl);

				if(!XMLUtils.IsMatchingPathConstraints(document.Root, pathConstraints)) {
					Console.WriteLine("skipping file, because of nonMatching pathconstraints: " + fileUrl);
					continue;
				}
				List<XElement> primaryConfigElements = XMLUtils.FindNodesAtPath(document.Root, configPath.Split(':'));
				Console.WriteLine("found " + primaryConfigElements.Count + " elements at path: " + configPath);
				foreach(XElement primaryElement in primaryConfigElements) {
					List<string> primaryMapValues = mapConfigBy.GetMapValues(fileUrl, document, primaryElement);
					if(primaryMapValues.Count != 1) {
						Console.WriteLine("Could not uniquely identify node at mapConfigBy.path");
						throw new InvalidOperationException("make sure your mapConfigBy.path can uniquely identify a node when loading from xml files!");
					}

					Dictionary<string, List<string>> secondaryValues = new Dictionary<string, List<string>>();
					foreach(OnlineInterpreterFilter filter in configFilter) {
						List<string> filterValues = new List<string>();
						if(filter.pathType == EOnlineInterpreterPathType.relative) {
							filterValues.AddRange(XMLUtils.FindNodeValuesAtPath(primaryElement, filter.path.Split(':')).Select(value => ResolveTranslationValue(value.Trim(), filter.outputName)));
						} else if(filter.pathType == EOnlineInterpreterPathType.absolute) {
							filterValues.AddRange(XMLUtils.FindNodeValuesAtPath(document.Root, filter.path.Split(':')).Select(value => ResolveTranslationValue(value.Trim(), filter.outputName)));
						} else {
							throw new InvalidOperationException("pathType " + filter.pathType.ToString() + " is not supported by OnlineSourceInterpreterConfig.cs");
						}
						secondaryValues[filter.outputName] = filterValues;
					}

					result[primaryMapValues[0]] = secondaryValues;
				}
			}

			return result;
		}

		public Dictionary<string, Dictionary<string, List<string>>> BuildBundleContent(Queue<string> fileUrls) {
			Dictionary<string, Dictionary<string, List<string>>> result = new Dictionary<string, Dictionary<string, List<string>>>();
			foreach(string fileUrl in fileUrls) {
				Console.WriteLine("loading bundle data from url: " + fileUrl);
				using(WebClient client = new WebClient()) {
					using(MemoryStream stream = new MemoryStream(client.DownloadData(fileUrl))) {
						AssetToolUtils assetToolUtils = new AssetToolUtils();
						Console.WriteLine("Download done, building AssetsFileInstance...");
						foreach(AssetFile file in assetToolUtils.BuildAssetsFileInstance(stream)) {
							try {
								BuildBundleContent(ref result, fileUrl, file, assetToolUtils);
							} catch(Exception e) {
								Console.WriteLine("failed to parse bundle! blame AssetTools...");
								Console.WriteLine("Exception: " + e.Message);
								Console.WriteLine(e.StackTrace);
							}
						}
						assetToolUtils.CloseActiveStreams();
					}
				}
			}

			return result;
		}

		private void BuildBundleContent(ref Dictionary<string, Dictionary<string, List<string>>> result, string fileUrl, AssetFile file, AssetToolUtils assetToolUtils) {
			foreach(AssetFileInfoEx info in file.fileInstance.table.assetFileInfo) {
				ClassDatabaseType type = AssetHelper.FindAssetClassByID(file.classDBFile, info.curFileType);
				if(type == null) {
					continue;
				}
				string typeName = type.name.GetString(file.classDBFile);
				if(typeName != "MonoBehaviour") {
					continue;
				}

				AssetTypeValueField baseField = assetToolUtils.GetATI(file, info).GetBaseField();

				/*using(StreamWriter writer = new StreamWriter(@"D:\monobehaviourcontentlogger.txt", true)) {
					writer.WriteLine("====================== found monobehaviour, printing... ======================");
					assetToolUtils.PrintFieldRecursively(writer, baseField);
					writer.WriteLine();
				}*/

				List<AssetTypeValueField> targetFields = assetToolUtils.GetFieldAtPath(file, baseField, configPath.Split(':'));
				if(targetFields.Count == 0) {
					continue;
				}
				if(!assetToolUtils.IsMatchingPathConstraints(file, baseField, pathConstraints)) {
					continue;
				}
				Console.WriteLine("found " + targetFields.Count + " matches in monobehaviour at path " + configPath);

				foreach(AssetTypeValueField targetField in targetFields) {
					List<string> mapByField = mapConfigBy.GetMapValues(fileUrl, file, baseField, targetField, assetToolUtils);
					Console.WriteLine("found " + mapByField.Count + " mapFields in targetField at mapConfigBy.path");

					foreach(string mapValue in mapByField) {
						Dictionary<string, List<string>> secondaryValues = new Dictionary<string, List<string>>();

						foreach(OnlineInterpreterFilter filter in configFilter) {
							List<string> filterValues = new List<string>();
							List<AssetTypeValueField> filterFields;

							if(filter.pathType == EOnlineInterpreterPathType.relative) {
								filterFields = assetToolUtils.GetFieldAtPath(file, targetField, filter.path.Split(':'));
							} else if(filter.pathType == EOnlineInterpreterPathType.absolute) {
								filterFields = assetToolUtils.GetFieldAtPath(file, baseField, filter.path.Split(':'));
							} else {
								throw new InvalidOperationException("pathType " + filter.pathType.ToString() + " is not supported by OnlineSourceInterpreterConfig.cs");
							}
							Console.WriteLine("found " + filterFields.Count + " fields at path: " + filter.path);
							filterValues.AddRange(filterFields
								.Where(field => field.GetValue() != null)
								.Select(field => ResolveTranslationValue(field.GetValue().AsString().Trim(), filter.outputName)));

							secondaryValues[filter.outputName] = filterValues;
						}

						if(result.ContainsKey(mapValue)) {
							foreach(string key in secondaryValues.Keys) {
								if(result[mapValue].ContainsKey(key)) {
									result[mapValue][key].AddRange(secondaryValues[key]);
								} else {
									result[mapValue][key] = secondaryValues[key];
								}
							}
						} else {
							result[mapValue] = secondaryValues;
						}

						LevelMappingResult(ref result, mapValue);
					}
				}
			}
		}

		private void LevelMappingResult(ref Dictionary<string, Dictionary<string, List<string>>> result, string mapValue) {
			Dictionary<string, List<string>> mappingResult = result[mapValue];

			int maxHeight = mappingResult.Max(kvp => kvp.Value.Count);
			foreach(KeyValuePair<string, List<string>> kvp in mappingResult) {
				if(kvp.Value.Count < maxHeight) {
					for(int i = (maxHeight - kvp.Value.Count); i > 0; i--) {
						kvp.Value.Add("");
					}
				}
			}

			result[mapValue] = mappingResult;
		}

		public string ResolveTranslationValue(string value, string outputName) {
			if(outputNameToTypeDict.ContainsKey(outputName)) {
				string typeName = outputNameToTypeDict[outputName];
				if(typeTranslationDict.ContainsKey(typeName)) {
					BetterDict<string, string> typeDict = typeTranslationDict[typeName];
					if(typeDict.ContainsKey(value)) {
						return typeDict[value];
					}
				}else if(regexTranslator.ContainsKey(typeName)) {
					return regexTranslator[typeName].Apply(value);
				}
			}
			return value;
		}

		private BetterDict<string, List<YamlObject>> GetObjectListChangeDict() {
			return new BetterDict<string, List<YamlObject>> {
				{nameof(configFilter), configFilter.ToList<YamlObject>()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeYamlObjectLists(ref lines, startLine, ref endLine, currentTabDepth, GetObjectListChangeDict());
		}

		private string GetConfigFilterInfoString() {
			return configFilter.Count(filter => filter.doDisplay) + " active filter";
		}

		private void OnConfigFilterClicked(MenuUtils menuUtils, string header, int spacing) {
			string displayHeader = "Currently Editing: " + header + "." + nameof(configFilter);
			string displayBackText = "Go back to " + header;

			int selection = 0;
			while(true) {
				string[] options = configFilter.Select(filter => MenuUtils.GetToggleString(filter.outputName) + " (" + filter.doDisplay + ")").ToArray();

				selection = menuUtils.OpenSelectionMenu(options, displayBackText, displayHeader, selection, spacing);

				if(selection >= options.Length) {
					break;
				}

				configFilter[selection].ToggleDoDisplay();
			}
		}

		string IMenuObject.GetInfoString() {
			return GetConfigFilterInfoString();
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new MenuOption[] {
				new MenuOption(nameof(configFilter), MenuUtils.GetConfigureString, GetConfigFilterInfoString, OnConfigFilterClicked)
			};
		}
	}
}

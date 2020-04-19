using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menuutils;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.flightstatcompare
{
	class FSCConfig
	{
		[YamlIgnore]
		public ConfigHolder configHolder;

		public SourceConfig sourceFrom = null;
		public SourceConfig sourceTo = null;
		public FSCOnlineSourceConfig onlineSourcesConfig = null;
		public LocalSourcesConfig localSourcesConfig = null;
		public ResultConfig resultConfig = null;
		public BetterDict<int, string> flightTypesDict = null;
		public List<ResultFilter> displayFilter;

		public void SaveConfig() {
			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string>{
				{"flightStatsCompareConfig.sourceFrom.sourceType", sourceFrom.sourceType.ToString()},
				{"flightStatsCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"flightStatsCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"flightStatsCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"flightStatsCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"flightStatsCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"flightStatsCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"flightStatsCompareConfig.sourceFrom.local.date", sourceFrom.local.date},

				{"flightStatsCompareConfig.sourceTo.sourceType", sourceTo.sourceType.ToString()},
				{"flightStatsCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"flightStatsCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"flightStatsCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"flightStatsCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"flightStatsCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"flightStatsCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"flightStatsCompareConfig.sourceTo.local.date", sourceTo.local.date},

				{"flightStatsCompareConfig.localSourcesConfig.lastcreated", localSourcesConfig.lastcreated},
				{"flightStatsCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"flightStatsCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"flightStatsCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},

				{"flightStatsCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"flightStatsCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict) && YamlUtils.ChangeSimpleObjectListContent(ref lines, "flightStatsCompareConfig.displayFilter", displayFilter.ToList<YamlObject>())) {
				Console.WriteLine("config saving was successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("config saving failed");
			}

			Console.WriteLine("Press any key to return to the menu");
			Console.ReadKey(true);
		}

		public void SaveLastCreatedSource(string value) {
			localSourcesConfig.lastcreated = value;
			string yamlPath = "flightStatsCompareConfig.localSourcesConfig.lastcreated";

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValue(ref lines, yamlPath, value)) {
				Console.WriteLine("lastcreated saving was successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("lastcreated saving failed!");
			}
		}

		public void ManageMakeFile(Dictionary<string, List<Dictionary<string, string>>> stats, SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online && sourceConfig.online.makeFile) {
				Console.WriteLine("parsing flightStatsSource to file from onlineSource, because makeFile is true");
				string targetMakeFile = GetLocalSourceFile(sourceConfig);
				string targetMakeFileDirectory = Path.GetDirectoryName(targetMakeFile);

				if(!Directory.Exists(targetMakeFileDirectory)) {
					Console.WriteLine("had to create directory: " + targetMakeFileDirectory);
					Directory.CreateDirectory(targetMakeFileDirectory);
				}

				using(StreamWriter writer = new StreamWriter(targetMakeFile, false)) {
					Console.WriteLine("Writing content to file: " + targetMakeFile);
					ISerializer serializer = new SerializerBuilder().Build();
					string yamlText = serializer.Serialize(stats);
					writer.WriteLine(yamlText);
				}

				if(sourceConfig.online.makeLastCreated) {
					Console.WriteLine("setting lastCreated to: " + targetMakeFile);
					SaveLastCreatedSource(targetMakeFile);
				}
			}
		}

		public Dictionary<string, List<Dictionary<string, string>>> GetFlightStatsFromSource(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online) {
				return GetFlightStatsFromOnlineAddresses(GatherOnlineSourceAddresses(sourceConfig), sourceConfig);
			} else if(sourceConfig.sourceType == SourceType.local) {
				return GetFlightStatsFromFile(GetLocalSourceFile(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				return GetFlightStatsFromFile(localSourcesConfig.lastcreated);
			}
			return null;
		}

		private Queue<string> GatherOnlineSourceAddresses(SourceConfig sourceConfig) {
			Console.WriteLine("gathering all dragonfiles from online source...");
			HtmlDocument document = new HtmlDocument();
			using(WebClient client = new WebClient()) {
				document.Load(client.OpenRead(GetAssetInfoFileURL(sourceConfig)));
			}
			Queue<string> dragonAddresses = new Queue<string>();

			HtmlNodeCollection nodeCollection = document.DocumentNode.SelectNodes("/*/*");
			foreach(HtmlNode node in nodeCollection) {
				string fileName = node.GetAttributeValue("N", null);
				if(fileName == null) {
					Console.WriteLine("fileName was null, not sure why. skipping...");
					continue;
				}

				if(!DoRegexCheck(fileName)) {
					continue;
				}

				dragonAddresses.Enqueue(fileName);
			}

			Console.WriteLine("=======================================\n\n");
			Console.WriteLine("found " + dragonAddresses.Count + " matching files!");

			return dragonAddresses;
		}

		private bool DoRegexCheck(string fileName) {
			foreach(string regex in onlineSourcesConfig.dataContainerRegexFilters) {
				if(!IsMatchingCustomRegex(fileName, regex, Regex.IsMatch)) {
					string baseString = "skipping file: " + fileName;
					Console.WriteLine(MenuUtils.AddTabsUntilTargetWidth(baseString, 8) + "reason: failed regexCheck: " + regex);
					return false;
				}
			}
			return true;
		}

		private bool IsMatchingCustomRegex(string value, string customRegex, Func<string, string, bool> isMatch) {
			if(customRegex.StartsWith("!")) {
				return !isMatch(value, customRegex.Remove(0, 1));
			} else {
				if(customRegex.StartsWith(@"\!")) {
					return isMatch(value, customRegex.Remove(0, 1));
				} else {
					return isMatch(value, customRegex);
				}
			}
		}

		private string GetAssetInfoFileURL(SourceConfig sourceConfig) {
			return string.Join('/', onlineSourcesConfig.baseURL, sourceConfig.online.platform, sourceConfig.online.version, onlineSourcesConfig.baseSuffix, onlineSourcesConfig.assetInfo);
		}

		private string GetOnlineSourceBaseURL(SourceConfig sourceConfig) {
			return string.Join('/', onlineSourcesConfig.baseURL, sourceConfig.online.platform, sourceConfig.online.version, onlineSourcesConfig.baseSuffix) + "/";
		}

		private Dictionary<string, List<Dictionary<string, string>>> GetFlightStatsFromOnlineAddresses(Queue<string> fileNames, SourceConfig sourceConfig) {
			Dictionary<string, List<Dictionary<string, string>>> result = new Dictionary<string, List<Dictionary<string, string>>>();

			List<KeyValuePair<string, string>> flightStatsArrayPath = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("_FlightInformation", "FlightInformation"),
				new KeyValuePair<string, string>("Array", "Array")
			};

			using(WebClient client = new WebClient()){
				foreach(string fileName in fileNames) {
					Console.WriteLine("building stats for file: " + fileName);
					string fileAddress = GetFileAddress(fileName, sourceConfig);
					using(MemoryStream memoryStream = new MemoryStream(client.DownloadData(fileAddress))) {
						AssetToolUtils assetToolUtils = new AssetToolUtils();
						AssetsFileInstance fileInstance = assetToolUtils.BuildAssetsFileInstance(memoryStream);

						foreach(AssetFileInfoEx info in fileInstance.table.assetFileInfo) {
							ClassDatabaseType type = AssetHelper.FindAssetClassByID(assetToolUtils.classDBFile, info.curFileType);
							if(type == null) {
								continue;
							}
							string typeName = type.name.GetString(assetToolUtils.classDBFile);
							if(typeName != "MonoBehaviour") {
								continue;
							}
							AssetTypeValueField baseField = assetToolUtils.GetATI(fileInstance.file, info).GetBaseField();
							if(baseField == null || baseField.GetChildrenCount() == 0) {
								continue;
							}
							AssetTypeValueField flightStatsArray = assetToolUtils.GetFieldAtPath(baseField, flightStatsArrayPath);
							if(flightStatsArray == null) {
								continue;
							}
							AssetTypeValueField gameObject = assetToolUtils.GetFieldAtPathNaive(baseField, "m_GameObject/m_PathID");
							if(gameObject == null) {
								continue;
							}
							Console.WriteLine("found: " + gameObject.value.AsString());
							AssetFileInfoEx gameObjInfo = fileInstance.table.GetAssetInfo(gameObject.GetValue().AsInt64());
							string dragonName = assetToolUtils.GetFieldAtPathNaive(assetToolUtils.GetATI(fileInstance.file, gameObjInfo).baseFields, "Base/m_Name").GetValue().AsString();
							
							Console.WriteLine("Loading stats for Dragon: " + dragonName);
							BuildStatMapFromAssetField(result, dragonName, flightStatsArray.GetChildrenList(), assetToolUtils);
						}

						assetToolUtils.CloseMainStream();
					}
				}
			}

			return result;
		}

		private void BuildStatMapFromAssetField(Dictionary<string, List<Dictionary<string, string>>> statMap, string dragonName, AssetTypeValueField[] flightInformation, AssetToolUtils assetToolUtils) {
			if(!statMap.ContainsKey(dragonName)) {
				statMap[dragonName] = new List<Dictionary<string, string>>();
			}
			for(int targetType = 0; targetType < flightInformation.Length; targetType++) {
				AssetTypeValueField statRoot = assetToolUtils.GetAssetMatching("data/_FlightType", targetType.ToString(), flightInformation);
				if(statRoot == null) {
					throw new NullReferenceException("could not find statRoot, trying to match " + targetType.ToString());
				}

				Dictionary<string, string> flightStats = new Dictionary<string, string>();

				AssetTypeValueField[] flightData = assetToolUtils.GetFieldNamed("_FlightData", statRoot.GetChildrenList()).GetChildrenList();
				foreach(AssetTypeValueField data in flightData) {
					List<Tuple<string, string>> statEntrys = ResolveStatListField(data);
					if(statEntrys != null && statEntrys.Count > 0) {
						statEntrys.ForEach(statEntry => {
							flightStats[statEntry.Item1] = statEntry.Item2;
						});
					}
				}

				statMap[dragonName].Add(flightStats);
			}
		}

		private List<Tuple<string, string>> ResolveStatListField(AssetTypeValueField field) {
			if(displayFilter.Any(filter => filter.path == field.GetName())){
				return new List<Tuple<string, string>> {new Tuple<string, string>(field.GetName(), field.GetValue().AsString())};
			} else {
				List<string[]> specialStats = displayFilter
					.Where(filter => filter.path.Contains(':'))
					.Select(filter => filter.path.Split(':'))
					.ToList();
				return ResolveSpecialStats(specialStats, new List<AssetTypeValueField>{field});
			}
		}

		private List<Tuple<string, string>> ResolveSpecialStats(List<string[]> applyableStats, List<AssetTypeValueField> fields, int matchDepth = 0) {
			List<Tuple<string, string>> result = new List<Tuple<string, string>>();
			if(fields == null || fields.Count == 0) {
				return null;
			}

			foreach(AssetTypeValueField field in fields) {
				List<string[]> applicableStats = applyableStats.Where(stats => stats.Length > matchDepth && stats[matchDepth] == field.GetName()).ToList();
				if(applicableStats.Count > 0) {
					foreach(string[] stats in applicableStats) {

						if(matchDepth == (stats.Length - 1)) {
							//found!
							result.Add(new Tuple<string, string>(string.Join(":", stats), field.value.AsString()));
						}
					}

					if(field.GetChildrenCount() > 0){
						List<Tuple<string, string>> found = ResolveSpecialStats(applicableStats, field.GetChildrenList().ToList(), matchDepth + 1);
						if(found != null) {
							result.AddRange(found);
						}
					}
				}
			}
			return result;
		}

		private string GetFileAddress(string fileName, SourceConfig sourceConfig) {
			foreach(KeyValuePair<string, string> pair in configHolder.onlineAddressDict) {
				if(fileName.StartsWith(pair.Key)) {
					fileName = pair.Value + fileName.Remove(0, pair.Key.Length);
					break;
				}
			}
			return GetOnlineSourceBaseURL(sourceConfig) + fileName;
		}

		private Dictionary<string, List<Dictionary<string, string>>> GetFlightStatsFromFile(string filePath) {
			using(StreamReader reader = new StreamReader(filePath)) {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				Console.WriteLine("parsing stats from local source: " + filePath);
				return deserializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(reader);
			}
		}

		private string GetLocalSourceFile(SourceConfig sourceConfig) {
			string fileName = localSourcesConfig.targetFileName;
			if(sourceConfig.sourceType == SourceType.online) {
				if(localSourcesConfig.appendPlatform) {
					fileName += ("_" + sourceConfig.online.platform);
				}
				if(localSourcesConfig.appendVersion) {
					fileName += ("_" + sourceConfig.online.version);
				}
				if(localSourcesConfig.appendDate) {
					fileName += ("_" + DateTime.Now.ToString("yyyy.MM.dd"));
				}
			} else if(sourceConfig.sourceType == SourceType.local) {
				if(localSourcesConfig.appendPlatform) {
					fileName += ("_" + sourceConfig.local.platform);
				}
				if(localSourcesConfig.appendVersion) {
					fileName += ("_" + sourceConfig.local.version);
				}
				if(localSourcesConfig.appendDate) {
					fileName += ("_" + sourceConfig.local.date);
				}
			} else {
				//undefined behaviour
				return null;
			}
			fileName += ("." + localSourcesConfig.targetFileExtension);

			return Path.Combine(localSourcesConfig.baseDirectory, fileName);
		}

		public string GetResultFile() {
			string fileName = localSourcesConfig.targetFileName;
			if(resultConfig.appendDate) {
				fileName += ("_" + DateTime.Now.ToString("yyyy.MM.dd"));
			}
			fileName += ("." + localSourcesConfig.targetFileExtension);
			return Path.Combine(resultConfig.baseDirectory, fileName);
		}
	}
}

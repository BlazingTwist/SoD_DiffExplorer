using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.squadtacticscompare
{
	class STCConfig
	{
		public SourceConfig sourceFrom = null;
		public SourceConfig sourceTo = null;
		public STCOnlineSourceConfig onlineSourcesConfig = null;
		public LocalSourcesConfig localSourcesConfig = null;
		public ResultConfig resultConfig = null;
		public List<string> targetStatPath = null;
		public string mapStatsBy = null;
		public BetterDict<string, bool> statList = null;

		public void SaveConfig() {
			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"squadTacticsCompareConfig.sourceFrom.sourceType", sourceFrom.sourceType.ToString()},
				{"squadTacticsCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"squadTacticsCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"squadTacticsCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"squadTacticsCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"squadTacticsCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"squadTacticsCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"squadTacticsCompareConfig.sourceFrom.local.date", sourceFrom.local.date},

				{"squadTacticsCompareConfig.sourceTo.sourceType", sourceTo.sourceType.ToString()},
				{"squadTacticsCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"squadTacticsCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"squadTacticsCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"squadTacticsCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"squadTacticsCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"squadTacticsCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"squadTacticsCompareConfig.sourceTo.local.date", sourceTo.local.date},

				{"squadTacticsCompareConfig.localSourcesConfig.lastcreated", localSourcesConfig.lastcreated},
				{"squadTacticsCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"squadTacticsCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"squadTacticsCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},

				{"squadTacticsCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"squadTacticsCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

			BetterDict<string, string> statFilterChangeDict = new BetterDict<string, string>(statList.ToDictionary(
				kvp => {
					return "squadTacticsCompareConfig.statList." + kvp.Key;
				}, kvp => {
					return kvp.Value.ToString();
				}));

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict) && YamlUtils.ChangeSimpleValues(ref lines, statFilterChangeDict)) {
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
			string yamlPath = "squadTacticsCompareConfig.localSourcesConfig.lastcreated";

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValue(ref lines, yamlPath, value)) {
				Console.WriteLine("lastCreated saving was successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("lastCreated saving failed!");
			}
		}

		public void ManageMakeFile(Dictionary<string, Dictionary<string, string>> stats, SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online && sourceConfig.online.makeFile) {
				Console.WriteLine("parsing squadTacticsStatSource to file from onlineSource, because makeFile is true");
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

		public Dictionary<string, Dictionary<string, string>> GetSquadTacticsStatsFromSource(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online) {
				using(WebClient client = new WebClient()) {
					using(MemoryStream memoryStream = new MemoryStream(client.DownloadData(GetSquadTacticsFileURL(sourceConfig)))) {
						return GetSquadTacticsStatsFromStream(memoryStream);
					}
				}
			} else if(sourceConfig.sourceType == SourceType.local) {
				return GetSquadTacticsStatsFromFile(GetLocalSourceFile(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				return GetSquadTacticsStatsFromFile(localSourcesConfig.lastcreated);
			}
			return null;
		}

		private Dictionary<string, Dictionary<string, string>> GetSquadTacticsStatsFromStream(Stream stream) {
			AssetToolUtils assetToolUtils = new AssetToolUtils();
			List<AssetTypeValueField> monoData = assetToolUtils.GetMonobehaviourData(stream, targetStatPath);

			if(monoData.Count != 1) {
				Console.WriteLine("Unexpected amount for files found! Aborting...");
				return null;
			}

			Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
			foreach(AssetTypeValueField arrayEntry in monoData[0].GetChildrenList()) {
				string mapValue = null;
				Dictionary<string, string> tempStats = new Dictionary<string, string>();

				foreach(AssetTypeValueField characterParam in arrayEntry.GetChildrenList()) {
					List<Tuple<string, string>> statEntrys = ResolveStatListField(characterParam);
					if(statEntrys != null && statEntrys.Count > 0) {
						statEntrys.ForEach(statEntry => {
							if(statEntry.Item1 == mapStatsBy) {
								mapValue = statEntry.Item2;
							} else {
								tempStats[statEntry.Item1] = statEntry.Item2;
							}
						});
					}
				}

				if(mapValue == null) {
					Console.WriteLine("Could not find mapValue! Discarding statmap...");
					Console.WriteLine("\t" + string.Join("\n\t", tempStats.Select(x => x.Key + "=" + x.Value).ToArray()));
				} else {
					result[mapValue] = tempStats;
				}
			}

			return result;
		}

		private Dictionary<string, Dictionary<string, string>> GetSquadTacticsStatsFromFile(string filePath) {
			using(StreamReader reader = new StreamReader(filePath)) {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				Console.WriteLine("parsing stats from local source: " + filePath);
				return deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader);
			}
		}

		private List<Tuple<string, string>> ResolveStatListField(AssetTypeValueField field) {
			if(statList.ContainsKey(field.GetName())) {
				return new List<Tuple<string, string>> { new Tuple<string, string>(field.GetName(), field.GetValue().AsString()) };
			} else {
				List<string[]> specialStats = statList
					.Where(pair => pair.Key.Contains(':'))
					.Select(pair => pair.Key.Split(':'))
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

		private string GetSquadTacticsFileURL(SourceConfig sourceConfig) {
			return string.Join('/', onlineSourcesConfig.baseURL, sourceConfig.online.platform, sourceConfig.online.version, onlineSourcesConfig.baseSuffix, onlineSourcesConfig.dataContainer);
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

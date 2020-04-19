using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.fireballcompare
{
	class FCConfig
	{
		public SourceConfig sourceFrom = null;
		public SourceConfig sourceTo = null;
		public SimpleOnlineSourcesConfig onlineSourcesConfig = null;
		public LocalSourcesConfig localSourcesConfig = null;
		public ResultConfig resultConfig = null;
		public List<string> targetStatPath = null;
		public MappingValue mapStatsBy = null;
		public List<ResultFilter> displayFilter = null;

		public void SaveConfig() {
			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"fireballCompareConfig.sourceFrom.sourceType", sourceFrom.sourceType.ToString()},
				{"fireballCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"fireballCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"fireballCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"fireballCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"fireballCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"fireballCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"fireballCompareConfig.sourceFrom.local.date", sourceFrom.local.date},

				{"fireballCompareConfig.sourceTo.sourceType", sourceTo.sourceType.ToString()},
				{"fireballCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"fireballCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"fireballCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"fireballCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"fireballCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"fireballCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"fireballCompareConfig.sourceTo.local.date", sourceTo.local.date},

				{"fireballCompareConfig.localSourcesConfig.lastcreated", localSourcesConfig.lastcreated},
				{"fireballCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"fireballCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"fireballCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},

				{"fireballCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"fireballCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict) && YamlUtils.ChangeSimpleObjectListContent(ref lines, "fireballCompareConfig.displayFilter", displayFilter.ToList<YamlObject>())) {
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
			string yamlPath = "fireballCompareConfig.localSourcesConfig.lastcreated";

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
				Console.WriteLine("parsing fireballStatSource to file from onlineSource, because makeFile is true");
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

		public Dictionary<string, Dictionary<string, string>> GetFireballStatsFromSource(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online) {
				using(WebClient client = new WebClient()) {
					using(MemoryStream memoryStream = new MemoryStream(client.DownloadData(GetFireballFileURL(sourceConfig)))) {
						return GetFireballStatsFromStream(memoryStream);
					}
				}
			} else if(sourceConfig.sourceType == SourceType.local) {
				string sourceFile = GetLocalSourceFile(sourceConfig);
				using(StreamReader reader = new StreamReader(sourceFile)) {
					IDeserializer deserializer = new DeserializerBuilder().Build();
					Console.WriteLine("parsing stats from local source: " + sourceFile);
					return deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader);
				}
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				using(StreamReader reader = new StreamReader(localSourcesConfig.lastcreated)) {
					IDeserializer deserializer = new DeserializerBuilder().Build();
					Console.WriteLine("parsing stats from lastcreated source: " + localSourcesConfig.lastcreated);
					return deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(reader);
				}
			}
			return null;
		}

		private Dictionary<string, Dictionary<string, string>> GetFireballStatsFromStream(Stream stream) {
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

				foreach(AssetTypeValueField weaponParam in arrayEntry.GetChildrenList()) {
					List<Tuple<string, string>> statEntrys = ResolveStatListField(weaponParam);
					if(statEntrys != null && statEntrys.Count > 0) {
						statEntrys.ForEach(statEntry => {
							if(statEntry.Item1 == mapStatsBy.path) {
								mapValue = statEntry.Item2;
							} else {
								tempStats[statEntry.Item1] = statEntry.Item2;
							}
						});
					}
				}

				if(mapValue == null) {
					Console.WriteLine("Could not find mapValue (" + mapStatsBy.path + ")! Discarding statmap...");
					Console.WriteLine("\t" + string.Join("\n\t", tempStats.Select(x => x.Key + "=" + x.Value).ToArray()));
				} else {
					result[mapValue] = tempStats;
				}
			}

			return result;
		}

		private List<Tuple<string, string>> ResolveStatListField(AssetTypeValueField field) {
			if(displayFilter.Any(filter => filter.path == field.GetName()) || mapStatsBy.path == field.GetName()) {
				return new List<Tuple<string, string>> { new Tuple<string, string>(field.GetName(), field.GetValue().AsString()) };
			} else {
				List<string[]> specialStats = displayFilter
					.Where(filter => filter.path.Contains(':'))
					.Select(filter => filter.path.Split(':'))
					.ToList();
				if(mapStatsBy.path.Contains(':')) {
					specialStats.Add(mapStatsBy.path.Split(':'));
				}
				return ResolveSpecialStats(specialStats, new List<AssetTypeValueField> { field });
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

					if(field.GetChildrenCount() > 0) {
						List<Tuple<string, string>> found = ResolveSpecialStats(applicableStats, field.GetChildrenList().ToList(), matchDepth + 1);
						if(found != null) {
							result.AddRange(found);
						}
					}
				}
			}
			return result;
		}

		private string GetFireballFileURL(SourceConfig sourceConfig) {
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

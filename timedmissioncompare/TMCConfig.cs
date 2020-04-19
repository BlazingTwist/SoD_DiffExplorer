using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using YamlDotNet.Serialization;
using System.Text;

namespace SoD_DiffExplorer.timedmissioncompare
{
	class TMCConfig
	{
		public SourceConfig sourceFrom = null;
		public SourceConfig sourceTo = null;
		public SimpleOnlineSourcesConfig onlineSourcesConfig = null;
		public LocalSourcesConfig localSourcesConfig = null;
		public ResultConfig resultConfig = null;
		public List<ResultFilter> displayFilter;

		public void SaveConfig() {
			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"timedMissionCompareConfig.sourceFrom.sourceType", sourceFrom.sourceType.ToString()},
				{"timedMissionCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"timedMissionCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"timedMissionCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"timedMissionCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"timedMissionCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"timedMissionCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"timedMissionCompareConfig.sourceFrom.local.date", sourceFrom.local.date},

				{"timedMissionCompareConfig.sourceTo.sourceType", sourceTo.sourceType.ToString()},
				{"timedMissionCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"timedMissionCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"timedMissionCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"timedMissionCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"timedMissionCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"timedMissionCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"timedMissionCompareConfig.sourceTo.local.date", sourceTo.local.date},

				{"timedMissionCompareConfig.localSourcesConfig.lastcreated", localSourcesConfig.lastcreated},
				{"timedMissionCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"timedMissionCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"timedMissionCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},

				{"timedMissionCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"timedMissionCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict) && YamlUtils.ChangeSimpleObjectListContent(ref lines, "timedMissionCompareConfig.displayFilter", displayFilter.ToList<YamlObject>())) {
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
			string yamlPath = "timedMissionCompareConfig.localSourcesConfig.lastcreated";

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

		public void ManageMakeFile(Dictionary<int, Dictionary<string, List<string>>> missionData, SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online && sourceConfig.online.makeFile) {
				Console.WriteLine("parsing timedMission to file from onlineSource, because makeFile is true");
				string targetMakeFile = GetLocalSourceFile(sourceConfig);
				string targetMakeFileDirectory = Path.GetDirectoryName(targetMakeFile);

				if(!Directory.Exists(targetMakeFileDirectory)) {
					Console.WriteLine("had to create directory: " + targetMakeFileDirectory);
					Directory.CreateDirectory(targetMakeFileDirectory);
				}

				using(StreamWriter writer = new StreamWriter(targetMakeFile, false)) {
					Console.WriteLine("Writing content to file: " + targetMakeFile);
					ISerializer serializer = new SerializerBuilder().Build();
					string yamlText = serializer.Serialize(missionData);
					writer.WriteLine(yamlText);
				}

				if(sourceConfig.online.makeLastCreated) {
					Console.WriteLine("setting lastCreated to: " + targetMakeFile);
					SaveLastCreatedSource(targetMakeFile);
				}
			}
		}

		public Dictionary<int, Dictionary<string, List<string>>> GetMissionDataFromSource(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online) {
				return GetMissionDataFromURL(GetMissionDataFileURL(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.local) {
				return GetMissionDataFromFile(GetLocalSourceFile(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				return GetMissionDataFromFile(localSourcesConfig.lastcreated);
			}
			return null;
		}

		private Dictionary<int, Dictionary<string, List<string>>> GetMissionDataFromURL(string url) {
			XDocument document;
			using(WebClient client = new WebClient()) {
				using(Stream stream = client.OpenRead(url)) {
					using(StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
						using(XmlReader xReader = XmlReader.Create(reader)) {
							document = XDocument.Load(xReader);
						}
					}
				}
			}
			IEnumerable<XElement> missions = document.Root.Descendants("Missions");
			Dictionary<int, Dictionary<string, List<string>>> result = new Dictionary<int, Dictionary<string, List<string>>>();
			foreach(XElement mission in missions) {
				int missionID = int.Parse(mission.Descendants("MissionID").ToArray()[0].Value.Trim());
				Console.WriteLine("found mission ID = " + missionID);

				Dictionary<string, List<string>> missionData = new Dictionary<string, List<string>>();
				foreach(string filter in displayFilter.Select(filter => filter.path)) {
					List<string> filterValues = new List<string>();

					IEnumerable<XElement> values = FindNodesAtPath(mission, filter.Split(':'));
					if(values != null) {
						foreach(XElement value in values) {
							filterValues.Add(value.Value);
						}
					}

					missionData[filter] = filterValues;
				}

				result[missionID] = missionData;
			}

			return result;
		}

		private IEnumerable<XElement> FindNodesAtPath(XElement node, string[] paths) {
			IEnumerable<XElement> currentScope = new List<XElement>{node};
			foreach(string path in paths) {
				List<XElement> found = new List<XElement>();
				foreach(IEnumerable<XElement> list in currentScope.Select(node => node.Elements())) {
					foreach(XElement child in list.Where(listChild => listChild.Name.LocalName == path)) {
						found.Add(child);
					}
				}
				currentScope = found;
			}
			return currentScope;
		}

		private Dictionary<int, Dictionary<string, List<string>>> GetMissionDataFromFile(string filePath) {
			using(StreamReader reader = new StreamReader(filePath)) {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				Console.WriteLine("parsing missionData from local source: " + filePath);
				return deserializer.Deserialize<Dictionary<int, Dictionary<string, List<string>>>>(reader);
			}
		}

		private string GetMissionDataFileURL(SourceConfig sourceConfig) {
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

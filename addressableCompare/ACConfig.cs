using System.Collections.Generic;
using System.IO;
using System;
using HtmlAgilityPack;
using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using System.Net;

namespace SoD_DiffExplorer.addressablecompare
{
	class ACConfig
	{
		public SourceConfig sourceFrom = null;
		public SourceConfig sourceTo = null;
		public SimpleOnlineSourcesConfig onlineSourcesConfig = null;
		public LocalSourcesConfig localSourcesConfig = null;
		public ResultConfig resultConfig = null;

		public void SaveConfig() {
			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"addressableCompareConfig.sourceFrom.sourceType", sourceFrom.sourceType.ToString()},
				{"addressableCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"addressableCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"addressableCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"addressableCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"addressableCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"addressableCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"addressableCompareConfig.sourceFrom.local.date", sourceFrom.local.date},

				{"addressableCompareConfig.sourceTo.sourceType", sourceTo.sourceType.ToString()},
				{"addressableCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"addressableCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"addressableCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"addressableCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"addressableCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"addressableCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"addressableCompareConfig.sourceTo.local.date", sourceTo.local.date},

				{"addressableCompareConfig.localSourcesConfig.lastcreated", localSourcesConfig.lastcreated},
				{"addressableCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"addressableCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"addressableCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},

				{"addressableCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"addressableCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict)) {
				Console.WriteLine("config saving was successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("config saving failed!");
			}

			Console.WriteLine("Press any key to return to the menu");
			Console.ReadKey(true);
		}

		public void SaveLastCreatedSource(string value) {
			localSourcesConfig.lastcreated = value;
			string yamlPath = "addressableCompareConfig.localSourcesConfig.lastcreated";

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

		public void ManageMakeFile(List<string> lines, SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online && sourceConfig.online.makeFile) {
				Console.WriteLine("parsing assetInfo to file from onlineSource, because makeFile is true");
				string targetMakeFile = GetLocalSourceFile(sourceConfig);
				string targetMakeFileDirectory = Path.GetDirectoryName(targetMakeFile);

				if(!Directory.Exists(targetMakeFileDirectory)) {
					Console.WriteLine("had to create directory: " + targetMakeFileDirectory);
					Directory.CreateDirectory(targetMakeFileDirectory);
				}

				using(StreamWriter writer = new StreamWriter(targetMakeFile, false)) {
					Console.WriteLine("Writing content to file: " + targetMakeFile);
					lines.ForEach(line => writer.WriteLine(line));
				}

				if(sourceConfig.online.makeLastCreated) {
					Console.WriteLine("setting lastCreated to: " + targetMakeFile);
					SaveLastCreatedSource(targetMakeFile);
				}
			}
		}

		public List<string> GetSourceEntryList(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType == SourceType.online) {
				return GetSourceEntryListFromOnline(GetAssetInfoURL(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.local) {
				return GetSourceEntryListFromFile(GetLocalSourceFile(sourceConfig));
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				return GetSourceEntryListFromFile(localSourcesConfig.lastcreated);
			}
			return null;
		}

		private List<string> GetSourceEntryListFromOnline(string assetInfoUrl) {
			HtmlDocument document = new HtmlDocument();
			using(WebClient client = new WebClient()) {
				document.Load(client.OpenRead(assetInfoUrl));
			}
			HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//a");

			List<string> result = new List<string>();
			foreach(HtmlNode node in collection) {
				string fileName = node.GetAttributeValue("N", null);
				if(fileName == null) {
					continue;
				}
				result.Add(fileName);
			}
			return result;
		}

		private List<string> GetSourceEntryListFromFile(string filePath) {
			List<string> result = new List<string>();
			using(StreamReader reader = new StreamReader(filePath)) {
				string line;
				while((line = reader.ReadLine()) != null) {
					result.Add(line);
				}
			}
			return result;
		}

		private string GetAssetInfoURL(SourceConfig sourceConfig) {
			return onlineSourcesConfig.baseURL + "/" + sourceConfig.online.platform + "/" + sourceConfig.online.version + "/" + onlineSourcesConfig.baseSuffix + "/" + onlineSourcesConfig.dataContainer;
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
				if(localSourcesConfig.appendPlatform) {
					fileName += ("_" + sourceConfig.local.platform);
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

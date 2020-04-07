using System.Collections.Generic;
using System.IO;
using System;
using HtmlAgilityPack;
using SoD_DiffExplorer.csutils;
using System.Net;

namespace SoD_DiffExplorer.addressableCompare
{
	class ACConfig
	{
		public ACSourceType sourceTypeFrom;
		public ACSourceType sourceTypeTo;
		public ACSourceConfigImpl sourceFrom = null;
		public ACSourceConfigImpl sourceTo = null;
		public ACOnlineSourceConfig onlineSourcesConfig = null;
		public ACLocalSourceConfig localSourcesConfig = null;
		public ACResultConfig resultConfig = null;

		public void SaveConfig() {
			List<string> lines = new List<string>();
			using(StreamReader reader = new StreamReader("config.yaml")) {
				string line;
				while((line = reader.ReadLine()) != null) {
					lines.Add(line);
				}
			}

			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"addressableCompareConfig.sourceTypeFrom", sourceTypeFrom.ToString()},
				{"addressableCompareConfig.sourceTypeTo", sourceTypeTo.ToString()},
				{"addressableCompareConfig.sourceFrom.online.platform", sourceFrom.online.platform},
				{"addressableCompareConfig.sourceFrom.online.version", sourceFrom.online.version},
				{"addressableCompareConfig.sourceFrom.online.makeFile", sourceFrom.online.makeFile.ToString()},
				{"addressableCompareConfig.sourceFrom.online.makeLastCreated", sourceFrom.online.makeLastCreated.ToString()},
				{"addressableCompareConfig.sourceTo.online.platform", sourceTo.online.platform},
				{"addressableCompareConfig.sourceTo.online.version", sourceTo.online.version},
				{"addressableCompareConfig.sourceTo.online.makeFile", sourceTo.online.makeFile.ToString()},
				{"addressableCompareConfig.sourceTo.online.makeLastCreated", sourceTo.online.makeLastCreated.ToString()},
				{"addressableCompareConfig.sourceFrom.local.platform", sourceFrom.local.platform},
				{"addressableCompareConfig.sourceFrom.local.version", sourceFrom.local.version},
				{"addressableCompareConfig.sourceFrom.local.date", sourceFrom.local.date},
				{"addressableCompareConfig.sourceTo.local.platform", sourceTo.local.platform},
				{"addressableCompareConfig.sourceTo.local.version", sourceTo.local.version},
				{"addressableCompareConfig.sourceTo.local.date", sourceTo.local.date},
				{"addressableCompareConfig.localSourcesConfig.appendPlatform", localSourcesConfig.appendPlatform.ToString()},
				{"addressableCompareConfig.localSourcesConfig.appendVersion", localSourcesConfig.appendVersion.ToString()},
				{"addressableCompareConfig.localSourcesConfig.appendDate", localSourcesConfig.appendDate.ToString()},
				{"addressableCompareConfig.resultConfig.makeFile", resultConfig.makeFile.ToString()},
				{"addressableCompareConfig.resultConfig.appendDate", resultConfig.appendDate.ToString()}
			};

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
			List<string> lines = new List<string>();
			using(StreamReader reader = new StreamReader("config.yaml")) {
				string line;
				while((line = reader.ReadLine()) != null) {
					lines.Add(line);
				}
			}

			BetterDict<string, string> simpleChangeDict = new BetterDict<string, string> {
				{"addressableCompareConfig.sourceFrom.lastcreated", value},
				{"addressableCompareConfig.sourceTo.lastcreated", value}
			};

			if(YamlUtils.ChangeSimpleValues(ref lines, simpleChangeDict)) {
				Console.WriteLine("config saving was successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("config saving failed!");
			}
		}

		public List<string> GetSourceEntryList(ACSourceType sourceType, ACSourceConfigImpl sourceConfig) {
			if(sourceType == ACSourceType.online) {
				return GetSourceEntryListFromOnline(GetAssetInfoURL(sourceConfig));
			}else if(sourceType == ACSourceType.local) {
				return GetSourceEntryListFromFile(GetAssetInfoPath(sourceConfig));
			}else if(sourceType == ACSourceType.lastcreated) {
				return GetSourceEntryListFromFile(sourceConfig.lastcreated);
			}
			return null;
		}

		public string GetLocalSourceDirectoryForMakeFile(ACSourceConfigImpl sourceConfig) {
			List<string> splittedPath = new List<string>();
			splittedPath.Add(localSourcesConfig.baseDirectory);

			if(localSourcesConfig.appendPlatform) {
				splittedPath.Add(sourceConfig.online.platform);
			}

			if(localSourcesConfig.appendVersion) {
				splittedPath.Add(sourceConfig.online.version);
			}

			return Path.Combine(splittedPath.ToArray());
		}

		public string GetResultDirectory() {
			return resultConfig.baseDirectory;
		}

		public string GetResultFile() {
			string fileName = localSourcesConfig.assetInfoFileName;
			if(resultConfig.appendDate) {
				fileName += ("_" + DateTime.Now.ToString("yyyy.MM.dd"));
			}
			fileName += ("." + localSourcesConfig.assetInfoFileExtension);
			return Path.Combine(GetResultDirectory(), fileName);
		}

		public void ManageMakeFile(List<string> lines, ACSourceType sourceType, ACSourceConfigImpl sourceConfig) {
			if(sourceType == ACSourceType.online && sourceConfig.online.makeFile) {
				Console.WriteLine("parsing assetInfo to file from addressablesFromList, because makeFile is true");
				string targetLocation = GetLocalSourceDirectoryForMakeFile(sourceConfig);
				if(!Directory.Exists(targetLocation)) {
					Console.WriteLine("had to create directory: " + targetLocation);
					Directory.CreateDirectory(targetLocation);
				}

				string targetFile = Path.Combine(targetLocation, GetLocalSourceFileName(DateTime.Now.ToString("yyyy.MM.dd")));
				using(StreamWriter writer = new StreamWriter(targetFile, false)) {
					Console.WriteLine("Writing content to file: " + targetFile);
					lines.ForEach(line => writer.WriteLine(line));
				}

				if(sourceConfig.online.makeLastCreated) {
					Console.WriteLine("setting lastCreated to: " + targetFile);
					sourceFrom.lastcreated = targetFile;
					sourceTo.lastcreated = targetFile;
					SaveLastCreatedSource(targetFile);
				}
			}
		}

		private string GetLocalSourceFileName(string date) {
			string result = localSourcesConfig.assetInfoFileName;
			if(localSourcesConfig.appendDate) {
				result += ("_" + date);
			}
			result += ("." + localSourcesConfig.assetInfoFileExtension);
			return result;
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

		private string GetAssetInfoURL(ACSourceConfigImpl sourceConfig) {
			return onlineSourcesConfig.baseURL + "/" + sourceConfig.online.platform + "/" + sourceConfig.online.version + "/" + onlineSourcesConfig.baseSuffix + "/" + onlineSourcesConfig.assetInfo;
		}

		private string GetAssetInfoPath(ACSourceConfigImpl sourceConfig) {
			List<string> pathList = new List<string>();
			pathList.Add(localSourcesConfig.baseDirectory);
			if(localSourcesConfig.appendPlatform) {
				pathList.Add(sourceConfig.local.platform);
			}
			if(localSourcesConfig.appendVersion) {
				pathList.Add(sourceConfig.local.version);
			}
			pathList.Add(GetLocalSourceFileName(sourceConfig.local.date));
			return Path.Combine(pathList.ToArray());
		}
	}
}

using System.Collections.Generic;
using System.Text.RegularExpressions;
using SoD_DiffExplorer.csutils;
using System;
using System.IO;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.filedownloader
{
	class FDConfig
	{
		[YamlIgnore]
		public ConfigHolder configHolder;

		public bool pauseDownloadOnError = false;
		public bool doDownload = false;
		public FDDownloadURL downloadURL = null;
		public FDOutputDirectory outputDirectory = null;
		public List<string> regexFilters = null;
		public List<string> localeFilters = null;

		public bool DoRegexCheck(string fileName) {
			foreach(string regex in regexFilters) {
				if(!IsMatchingCustomRegex(fileName, regex, Regex.IsMatch)) {
					Console.WriteLine("skipping file: " + fileName + " reason: failed regexCheck: " + regex);
					return false;
				}
			}
			return true;
		}

		public bool DoLocaleCheck(string locale) {
			return localeFilters.Count == 0 || localeFilters.Contains(locale);
		}

		public bool IsMatchingCustomRegex(string value, string customRegex, Func<string, string, bool> isMatch) {
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

		public string GetFileAddress(string fileName) {
			foreach(KeyValuePair<string, string> pair in configHolder.onlineAddressDict) {
				if(fileName.StartsWith(pair.Key)) {
					fileName = pair.Value + fileName.Remove(0, pair.Key.Length);
					break;
				}
			}
			return downloadURL.GetFullBaseUrl() + fileName;
		}

		public void SaveConfig() {
			BetterDict<string, string> changeDict = new BetterDict<string, string> {
				{"fileDownloaderConfig.pauseDownloadOnError", pauseDownloadOnError.ToString()},
				{"fileDownloaderConfig.doDownload", doDownload.ToString()},
				{"fileDownloaderConfig.downloadURL.platform", downloadURL.platform},
				{"fileDownloaderConfig.downloadURL.version", downloadURL.version},
				{"fileDownloaderConfig.outputDirectory.appendPlatform", outputDirectory.appendPlatform.ToString()},
				{"fileDownloaderConfig.outputDirectory.appendVersion", outputDirectory.appendVersion.ToString()},
				{"fileDownloaderConfig.outputDirectory.appendDate", outputDirectory.appendDate.ToString()}
			};

			BetterDict<string, List<string>> listChangeDict = new BetterDict<string, List<string>> {
				{"fileDownloaderConfig.regexFilters", regexFilters},
				{"fileDownloaderConfig.localeFilters", localeFilters}
			};

			List<string> lines = YamlUtils.GetAllConfigLines();
			if(YamlUtils.ChangeSimpleValues(ref lines, changeDict) && YamlUtils.ChangeSimpleListContents(ref lines, listChangeDict)) {
				Console.WriteLine("config saving successful");
				using(StreamWriter writer = new StreamWriter("config.yaml", false)) {
					lines.ForEach(line => writer.WriteLine(line));
				}
			} else {
				Console.WriteLine("config saving failed!");
			}

			Console.WriteLine("Press any key to return to the menu");
			Console.ReadKey(true);
		}
	}
}

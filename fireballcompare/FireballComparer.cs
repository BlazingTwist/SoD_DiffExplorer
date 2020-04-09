using SoD_DiffExplorer.menuutils;
using SoD_DiffExplorer.csutils;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SoD_DiffExplorer.fireballcompare
{
	class FireballComparer
	{
		private FCConfig config;

		public FireballComparer(FCConfig config) {
			this.config = config;
		}

		private void RunFireballComparison() {
			Dictionary<string, Dictionary<string, string>> fireballFrom = config.GetFireballStatsFromSource(config.sourceFrom);
			Dictionary<string, Dictionary<string, string>> fireballTo = config.GetFireballStatsFromSource(config.sourceTo);

			config.ManageMakeFile(fireballFrom, config.sourceFrom);
			config.ManageMakeFile(fireballTo, config.sourceTo);

			//do comparison
			CompareResultImpl compareResult = new CompareResultImpl(fireballFrom, fireballTo, config.statList);
			StringBuilder resultText = new StringBuilder();
			resultText.Append("\t").Append(config.mapStatsBy).Append("\t").Append(string.Join("\t", compareResult.statOrder));
			resultText.Append("\nnew").Append(compareResult.formatComparison(compareResult.addedValues));
			resultText.Append("\nchanged").Append(compareResult.formatComparisonChange(compareResult.changedValuesFrom, compareResult.changedValuesTo));
			resultText.Append("\nremoved").Append(compareResult.formatComparison(compareResult.removedValues));
			resultText.Append("\nunchanged").Append(compareResult.formatComparison(compareResult.sameValues));
			if(config.resultConfig.makeFile) {
				string resultFile = config.GetResultFile();
				string resultDirectory = Path.GetDirectoryName(resultFile);
				if(!Directory.Exists(resultDirectory)) {
					Console.WriteLine("had to create directory: " + resultDirectory);
					Directory.CreateDirectory(resultDirectory);
				}
				using(StreamWriter writer = new StreamWriter(resultFile, false)) {
					writer.WriteLine(resultText.ToString());
				}
			}
			Console.WriteLine(resultText.ToString());
			Console.ReadKey(true);
		}

		public void OpenFireballComparerMenu() {
			string header = "Fireball Comparer:";
			string[] options = new string[] {
				"Adjust Configuration",
				"Run Comparison\n"
			};
			string backtext = "Back to Main Menu";
			int spacing = 1;

			int selection = 0;
			while(true) {
				selection = MenuUtils.OpenSelectionMenu(options, backtext, header, selection, spacing);

				switch(selection) {
					case 0:
						OpenConfigMenu();
						break;
					case 1:
						Console.Clear();
						RunFireballComparison();
						break;
					case 2:
						return;
				}
			}
		}

		private void OpenConfigMenu() {
			string header = "Fireball Comparer Config";
			string backText = "Back to Fireball Comparer";
			int spacing = 2;

			int selection = 0;
			while(true) {
				string[] options = GetConfigOptions();
				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						OpenSourceConfigMenu(config.sourceFrom, "sourceFrom");
						break;
					case 1:
						OpenSourceConfigMenu(config.sourceTo, "sourceTo");
						break;
					case 2:
						OpenLocalSourcesConfigMenu();
						break;
					case 3:
						OpenResultConfigMenu();
						break;
					case 4:
						OpenStatListConfigMenu();
						break;
					case 5:
						Console.Clear();
						config.SaveConfig();
						break;
					case 6:
						return;
				}
			}
		}

		private string[] GetConfigOptions() {
			return new string[]{
				"adjust sourceFrom (" + GetSourceInfoString(config.sourceFrom) + ")",
				"adjust sourceTo (" + GetSourceInfoString(config.sourceTo) + ")",
				"adjust localSourcesConfig (" + GetLocalSourcesConfigInfoString() + ")",
				"adjust resultConfig (" + GetResultConfigInfoString() + ")",
				"adjust statFilters (" + GetStatListInfoString() + ")\n",
				"save config\n"
			};
		}

		private string GetSourceInfoString(FCSourceConfig sourceConfig) {
			List<string> values = new List<string>();
			if(sourceConfig.sourceType == FCSourceType.online) {
				FCOnlineSource fcosConfig = sourceConfig.online;
				values.Add("platform = " + fcosConfig.platform);
				values.Add("version = " + fcosConfig.version);
				values.Add("makeFile = " + fcosConfig.makeFile);
				if(fcosConfig.makeFile) {
					values.Add("makeLastCreated = " + fcosConfig.makeLastCreated);
				}
			} else if(sourceConfig.sourceType == FCSourceType.local) {
				FCLocalSource fclsConfig = sourceConfig.local;
				if(config.localSourcesConfig.appendPlatform) {
					values.Add("platform = " + fclsConfig.platform);
				}
				if(config.localSourcesConfig.appendVersion) {
					values.Add("version = " + fclsConfig.version);
				}
				if(config.localSourcesConfig.appendDate) {
					values.Add("date = " + fclsConfig.date);
				}
			} else if(sourceConfig.sourceType == FCSourceType.lastcreated) {
				values.Add(config.localSourcesConfig.lastcreated);
			}
			return string.Join(" | ", values);
		}

		private string GetLocalSourcesConfigInfoString() {
			FCLocalSourceConfig fclsConfig = config.localSourcesConfig;
			return "appendPlatform = " + fclsConfig.appendPlatform + " | appendVersion = " + fclsConfig.appendVersion + " | appendDate = " + fclsConfig.appendDate;
		}

		private string GetResultConfigInfoString() {
			FCResultConfig fcrConfig = config.resultConfig;
			return "makeFile = " + fcrConfig.makeFile + " | appendDate = " + fcrConfig.appendDate;
		}

		private string GetStatListInfoString() {
			return string.Join(", ", config.statList.Where(kvp => kvp.Value).Select(kvp => kvp.Key));
		}

		private void OpenSourceConfigMenu(FCSourceConfig sourceConfig, string sourceName) {
			string header = "Currently editing " + sourceName;
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = GetSourceConfigOptions(sourceConfig);
				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						sourceConfig.sourceType = Enum.Parse<FCSourceType>(MenuUtils.OpenEnumConfigEditor("sourceType", sourceConfig.sourceType.ToString(), Enum.GetNames(typeof(FCSourceType)), spacing));
						break;
					case 1:
						sourceConfig.online.platform = MenuUtils.OpenSimpleConfigEditor("online.platform", sourceConfig.online.platform);
						break;
					case 2:
						sourceConfig.online.version = MenuUtils.OpenSimpleConfigEditor("online.version", sourceConfig.online.version);
						break;
					case 3:
						sourceConfig.online.makeFile = !sourceConfig.online.makeFile;
						break;
					case 4:
						sourceConfig.online.makeLastCreated = !sourceConfig.online.makeLastCreated;
						break;
					case 5:
						sourceConfig.local.platform = MenuUtils.OpenSimpleConfigEditor("local.platform", sourceConfig.local.platform);
						break;
					case 6:
						sourceConfig.local.version = MenuUtils.OpenSimpleConfigEditor("local.version", sourceConfig.local.version);
						break;
					case 7:
						sourceConfig.local.date = MenuUtils.OpenSimpleConfigEditor("local.date", sourceConfig.local.date);
						break;
					case 8:
						return;
				}
			}
		}

		private string[] GetSourceConfigOptions(FCSourceConfig sourceConfig) {
			return new string[] {
				"change sourceType (" + sourceConfig.sourceType + ")\n",
				"change online.platform (" + sourceConfig.online.platform + ")",
				"change online.version (" + sourceConfig.online.version + ")",
				"toggle online.makeFile (" + sourceConfig.online.makeFile + ")",
				"toggle online.makeLastCreated (" + sourceConfig.online.makeLastCreated + ")\n",
				"change local.platform (" + sourceConfig.local.platform + ")",
				"change local.version (" + sourceConfig.local.version + ")",
				"change local.date (" + sourceConfig.local.date + ")\n",
			};
		}

		private void OpenLocalSourcesConfigMenu() {
			string header = "Currently editing localSourcesConfig";
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"select lastCreated from local files (" + config.localSourcesConfig.lastcreated + ")",
					"toggle appendPlatform (" + config.localSourcesConfig.appendPlatform + ")",
					"toggle appendVersion (" + config.localSourcesConfig.appendVersion + ")",
					"toggle appendDate (" + config.localSourcesConfig.appendDate + ")\n"
				};

				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						config.localSourcesConfig.lastcreated = MenuUtils.OpenFileSelectionMenu(config.localSourcesConfig.baseDirectory, config.localSourcesConfig.lastcreated, 3);
						break;
					case 1:
						config.localSourcesConfig.appendPlatform = !config.localSourcesConfig.appendPlatform;
						break;
					case 2:
						config.localSourcesConfig.appendVersion = !config.localSourcesConfig.appendVersion;
						break;
					case 3:
						config.localSourcesConfig.appendDate = !config.localSourcesConfig.appendDate;
						break;
					case 4:
						return;
				}
			}
		}

		private void OpenResultConfigMenu() {
			string header = "Currently editing resultConfig";
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"toggle makeFile (" + config.resultConfig.makeFile + ")",
					"toggle appendDate (" + config.resultConfig.appendDate + ")\n"
				};

				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						config.resultConfig.makeFile = !config.resultConfig.makeFile;
						break;
					case 1:
						config.resultConfig.appendDate = !config.resultConfig.appendDate;
						break;
					case 2:
						return;
				}
			}
		}

		private void OpenStatListConfigMenu() {
			List<KeyValuePair<string, bool>> orderedStatList = config.statList.Select(kvp => kvp).ToList();
			string header = "Currently editing statList (filtered stats)";
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = orderedStatList.Select(kvp => "toggle " + kvp.Key + " (" + kvp.Value + ")").ToArray();
				options[options.Length - 1] += "\n";

				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				if(selection >= options.Length) {
					break;
				}

				orderedStatList[selection] = new KeyValuePair<string, bool>(orderedStatList[selection].Key, !orderedStatList[selection].Value);
			}

			config.statList = new BetterDict<string, bool>(orderedStatList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
		}
	}
}

using SoD_DiffExplorer.commonconfig;
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
		private MenuUtils menuUtils;

		public FireballComparer(FCConfig config, MenuUtils menuUtils) {
			this.config = config;
			this.menuUtils = menuUtils;
		}

		private void RunFireballComparison() {
			Dictionary<string, Dictionary<string, string>> fireballFrom = config.GetFireballStatsFromSource(config.sourceFrom);
			Dictionary<string, Dictionary<string, string>> fireballTo = config.GetFireballStatsFromSource(config.sourceTo);

			config.ManageMakeFile(fireballFrom, config.sourceFrom);
			config.ManageMakeFile(fireballTo, config.sourceTo);

			//do comparison
			CompareResultImpl compareResult = new CompareResultImpl(fireballFrom, fireballTo, config.displayFilter);
			StringBuilder resultText = new StringBuilder();
			resultText.Append("\t").Append(config.mapStatsBy.outputName).Append("\t").Append(string.Join("\t", compareResult.resultFilter.Select(filter => filter.outputName)));
			resultText.Append("\nnew").Append(compareResult.FormatComparison(compareResult.addedValues));
			resultText.Append("\nchanged").Append(compareResult.FormatComparisonChange(compareResult.changedValuesFrom, compareResult.changedValuesTo));
			resultText.Append("\nremoved").Append(compareResult.FormatComparison(compareResult.removedValues));
			resultText.Append("\nunchanged").Append(compareResult.FormatComparison(compareResult.sameValues));

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
				selection = menuUtils.OpenSelectionMenu(options, backtext, header, selection, spacing);

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
				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

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
						OpenDisplayFilterConfigMenu();
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
				"adjust sourceFrom \t\t(" + GetSourceInfoString(config.sourceFrom) + ")",
				"adjust sourceTo \t\t(" + GetSourceInfoString(config.sourceTo) + ")",
				"adjust localSourcesConfig \t(" + GetLocalSourcesConfigInfoString() + ")",
				"adjust resultConfig \t\t(" + GetResultConfigInfoString() + ")",
				"adjust displayFilters \t(" + GetDisplayFilterInfoString() + ")\n",
				"save config\n"
			};
		}

		private string GetSourceInfoString(SourceConfig sourceConfig) {
			List<string> values = new List<string>();
			if(sourceConfig.sourceType == SourceType.online) {
				OnlineSource fcosConfig = sourceConfig.online;
				values.Add("platform = " + fcosConfig.platform);
				values.Add("version = " + fcosConfig.version);
				values.Add("makeFile = " + fcosConfig.makeFile);
				if(fcosConfig.makeFile) {
					values.Add("makeLastCreated = " + fcosConfig.makeLastCreated);
				}
			} else if(sourceConfig.sourceType == SourceType.local) {
				LocalSource fclsConfig = sourceConfig.local;
				if(config.localSourcesConfig.appendPlatform) {
					values.Add("platform = " + fclsConfig.platform);
				}
				if(config.localSourcesConfig.appendVersion) {
					values.Add("version = " + fclsConfig.version);
				}
				if(config.localSourcesConfig.appendDate) {
					values.Add("date = " + fclsConfig.date);
				}
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				values.Add(config.localSourcesConfig.lastcreated);
			}
			return string.Join(" | ", values);
		}

		private string GetLocalSourcesConfigInfoString() {
			LocalSourcesConfig fclsConfig = config.localSourcesConfig;
			return "appendPlatform = " + fclsConfig.appendPlatform + " | appendVersion = " + fclsConfig.appendVersion + " | appendDate = " + fclsConfig.appendDate;
		}

		private string GetResultConfigInfoString() {
			ResultConfig fcrConfig = config.resultConfig;
			return "makeFile = " + fcrConfig.makeFile + " | appendDate = " + fcrConfig.appendDate;
		}

		private string GetDisplayFilterInfoString() {
			return config.displayFilter.Where(filter => filter.isAllowed).ToArray().Length + " selected";
		}

		private void OpenSourceConfigMenu(SourceConfig sourceConfig, string sourceName) {
			string header = "Currently editing " + sourceName;
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = GetSourceConfigOptions(sourceConfig);
				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						sourceConfig.sourceType = Enum.Parse<SourceType>(menuUtils.OpenEnumConfigEditor("sourceType", sourceConfig.sourceType.ToString(), Enum.GetNames(typeof(SourceType)), spacing));
						break;
					case 1:
						sourceConfig.online.platform = menuUtils.OpenSimpleConfigEditor("online.platform", sourceConfig.online.platform);
						break;
					case 2:
						sourceConfig.online.version = menuUtils.OpenSimpleConfigEditor("online.version", sourceConfig.online.version);
						break;
					case 3:
						sourceConfig.online.makeFile = !sourceConfig.online.makeFile;
						break;
					case 4:
						sourceConfig.online.makeLastCreated = !sourceConfig.online.makeLastCreated;
						break;
					case 5:
						sourceConfig.local.platform = menuUtils.OpenSimpleConfigEditor("local.platform", sourceConfig.local.platform);
						break;
					case 6:
						sourceConfig.local.version = menuUtils.OpenSimpleConfigEditor("local.version", sourceConfig.local.version);
						break;
					case 7:
						sourceConfig.local.date = menuUtils.OpenSimpleConfigEditor("local.date", sourceConfig.local.date);
						break;
					case 8:
						return;
				}
			}
		}

		private string[] GetSourceConfigOptions(SourceConfig sourceConfig) {
			return new string[] {
				"change sourceType \t\t\t(" + sourceConfig.sourceType + ")\n",
				"change online.platform \t\t(" + sourceConfig.online.platform + ")",
				"change online.version \t\t(" + sourceConfig.online.version + ")",
				"toggle online.makeFile \t\t(" + sourceConfig.online.makeFile + ")",
				"toggle online.makeLastCreated \t(" + sourceConfig.online.makeLastCreated + ")\n",
				"change local.platform \t\t(" + sourceConfig.local.platform + ")",
				"change local.version \t\t(" + sourceConfig.local.version + ")",
				"change local.date \t\t\t(" + sourceConfig.local.date + ")\n",
			};
		}

		private void OpenLocalSourcesConfigMenu() {
			string header = "Currently editing localSourcesConfig";
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"select lastCreated from local files \t(" + config.localSourcesConfig.lastcreated + ")",
					"toggle appendPlatform \t\t(" + config.localSourcesConfig.appendPlatform + ")",
					"toggle appendVersion \t\t(" + config.localSourcesConfig.appendVersion + ")",
					"toggle appendDate \t\t\t(" + config.localSourcesConfig.appendDate + ")\n"
				};

				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						config.localSourcesConfig.lastcreated = menuUtils.OpenFileSelectionMenu(config.localSourcesConfig.baseDirectory, config.localSourcesConfig.lastcreated, spacing);
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
					"toggle makeFile \t(" + config.resultConfig.makeFile + ")",
					"toggle appendDate \t(" + config.resultConfig.appendDate + ")\n"
				};

				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

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

		private void OpenDisplayFilterConfigMenu() {
			string header = "Currently editing statList (filtered stats)";
			string backText = "Back to Fireball Config Menu";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = config.displayFilter.Select(filter => "toggle " + filter.outputName + " (" + filter.isAllowed + ")").ToArray();
				options[options.Length - 1] += "\n";

				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				if(selection >= options.Length) {
					break;
				}

				config.displayFilter[selection].ToggleIsAllowed();
			}
		}
	}
}

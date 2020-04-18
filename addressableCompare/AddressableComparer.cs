using System;
using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.menuutils;
using System.Collections.Generic;
using System.IO;

namespace SoD_DiffExplorer.addressablecompare
{
	class AddressableComparer
	{
		private ACConfig config;
		private MenuUtils menuUtils;

		public AddressableComparer(ACConfig config, MenuUtils menuUtils) {
			this.config = config;
			this.menuUtils = menuUtils;
		}

		private void RunAddressableComparison() {
			List<string> addressablesFrom = config.GetSourceEntryList(config.sourceFrom);
			List<string> addressablesTo = config.GetSourceEntryList(config.sourceTo);

			config.ManageMakeFile(addressablesFrom, config.sourceFrom);
			config.ManageMakeFile(addressablesTo, config.sourceTo);

			CompareResultImpl result = new CompareResultImpl(addressablesFrom, addressablesTo);
			if(config.resultConfig.makeFile) {
				string resultFile = config.GetResultFile();
				string resultDirectory = Path.GetDirectoryName(resultFile);
				if(!Directory.Exists(resultDirectory)) {
					Directory.CreateDirectory(resultDirectory);
				}
				using(StreamWriter writer = new StreamWriter(resultFile, false)) {
					Console.WriteLine("===== printing added values =====");
					writer.WriteLine("===== printing added values =====");
					result.addedValues.ForEach(value => {
						Console.WriteLine(value);
						writer.WriteLine(value);
					});
					Console.WriteLine("===== printing removed values =====");
					writer.WriteLine("===== printing removed values =====");
					result.removedValues.ForEach(value => {
						Console.WriteLine(value);
						writer.WriteLine(value);
					});
				}
			} else {
				Console.WriteLine("===== printing added values =====");
				result.addedValues.ForEach(value => Console.WriteLine(value));
				Console.WriteLine("===== printing removed values =====");
				result.removedValues.ForEach(value => Console.WriteLine(value));
			}

			Console.WriteLine("Waiting for keypress...");
			Console.ReadKey(true);
		}

		public void OpenAddressableComparerMenu() {
			string header = "Addressable Comparer:";
			string[] options = new string[]{
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
						RunAddressableComparison();
						break;
					case 2:
						return;
				}
			}
		}

		private void OpenConfigMenu() {
			string header = "Fireball Comparer Config";
			string backText = "Back to Addressable Comparer";
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
						Console.Clear();
						config.SaveConfig();
						break;
					case 5:
						return;
				}
			}
		}

		private string[] GetConfigOptions() {
			return new string[] {
				"adjust sourceFrom (" + GetSourceInfoString(config.sourceFrom) + ")",
				"adjust sourceTo (" + GetSourceInfoString(config.sourceTo) + ")",
				"adjust localSourcesConfig (" + GetLocalSourcesConfigInfoString() + ")",
				"adjust resultConfig (" + GetResultConfigInfoString() + ")\n",
				"save config\n"
			};
		}

		private string GetSourceInfoString(SourceConfig sourceConfig) {
			List<string> values = new List<string>();
			if(sourceConfig.sourceType == SourceType.online) {
				OnlineSource acosConfig = sourceConfig.online;
				values.Add("platform = " + acosConfig.platform);
				values.Add("version = " + acosConfig.version);
				values.Add("makeFile = " + acosConfig.makeFile);
				if(acosConfig.makeFile) {
					values.Add("makeLastCreated = " + acosConfig.makeLastCreated);
				}
			} else if(sourceConfig.sourceType == SourceType.local) {
				LocalSource aclsConfig = sourceConfig.local;
				if(config.localSourcesConfig.appendPlatform) {
					values.Add("platform = " + aclsConfig.platform);
				}
				if(config.localSourcesConfig.appendVersion) {
					values.Add("version = " + aclsConfig.version);
				}
				if(config.localSourcesConfig.appendDate) {
					values.Add("date = " + aclsConfig.date);
				}
			} else if(sourceConfig.sourceType == SourceType.lastcreated) {
				values.Add(config.localSourcesConfig.lastcreated);
			}
			return string.Join(" | ", values);
		}

		private string GetLocalSourcesConfigInfoString() {
			LocalSourcesConfig aclsConfig = config.localSourcesConfig;
			return "appendPlatform = " + aclsConfig.appendPlatform + " | appendVersion = " + aclsConfig.appendVersion + " | appendDate = " + aclsConfig.appendDate;
		}

		private string GetResultConfigInfoString() {
			ResultConfig acrConfig = config.resultConfig;
			return "makeFile = " + acrConfig.makeFile + " | appendDate = " + acrConfig.appendDate;
		}

		private void OpenSourceConfigMenu(SourceConfig sourceConfigImpl, string sourceName) {
			string backText = "Back to Addressable Config Menu";
			string header = "Currently editing " + sourceName;
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = GetSourceConfigImplOptions(sourceConfigImpl);
				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						sourceConfigImpl.sourceType = Enum.Parse<SourceType>(menuUtils.OpenEnumConfigEditor("sourceType", sourceConfigImpl.sourceType.ToString(), Enum.GetNames(typeof(SourceType)), spacing));
						break;
					case 1:
						sourceConfigImpl.online.platform = menuUtils.OpenSimpleConfigEditor("online.platform", sourceConfigImpl.online.platform);
						break;
					case 2:
						sourceConfigImpl.online.version = menuUtils.OpenSimpleConfigEditor("online.version", sourceConfigImpl.online.version);
						break;
					case 3:
						sourceConfigImpl.online.makeFile = !sourceConfigImpl.online.makeFile;
						break;
					case 4:
						sourceConfigImpl.online.makeLastCreated = !sourceConfigImpl.online.makeLastCreated;
						break;
					case 5:
						sourceConfigImpl.local.platform = menuUtils.OpenSimpleConfigEditor("local.platform", sourceConfigImpl.local.platform);
						break;
					case 6:
						sourceConfigImpl.local.version = menuUtils.OpenSimpleConfigEditor("local.version", sourceConfigImpl.local.version);
						break;
					case 7:
						sourceConfigImpl.local.date = menuUtils.OpenSimpleConfigEditor("local.date", sourceConfigImpl.local.date);
						break;
					case 8:
						return;
				}
			}
		}

		private string[] GetSourceConfigImplOptions(SourceConfig sourceConfigImpl) {
			return new string[] {
				"change sourceType (" + sourceConfigImpl.sourceType + ")\n",
				"change online.platform (" + sourceConfigImpl.online.platform + ")",
				"change online.version (" + sourceConfigImpl.online.version + ")",
				"toggle online.makeFile (" + sourceConfigImpl.online.makeFile + ")",
				"toggle online.makeLastCreated (" + sourceConfigImpl.online.makeLastCreated + ")\n",
				"change local.platform (" + sourceConfigImpl.local.platform + ")",
				"change local.version (" + sourceConfigImpl.local.version + ")",
				"change local.date (" + sourceConfigImpl.local.date + ")\n"
			};
		}

		private void OpenLocalSourcesConfigMenu() {
			string backText = "Back to Addressable Config Menu";
			string header = "Currently editing localSourcesConfig";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"select lastCreated from local files (" + config.localSourcesConfig.lastcreated + ")",
					"toggle appendPlatform (" + config.localSourcesConfig.appendPlatform + ")",
					"toggle appendVersion (" + config.localSourcesConfig.appendVersion + ")",
					"toggle appendDate (" + config.localSourcesConfig.appendDate + ")\n"
				};

				selection = menuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						config.localSourcesConfig.lastcreated = menuUtils.OpenFileSelectionMenu(config.localSourcesConfig.baseDirectory, config.localSourcesConfig.lastcreated, 3);
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
			string backText = "Back to Addressable Config Menu";
			string header = "Currently editing resultConfig";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[]{
					"toggle makeFile (" + config.resultConfig.makeFile + ")",
					"toggle appendDate (" + config.resultConfig.appendDate + ")\n"
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
	}
}

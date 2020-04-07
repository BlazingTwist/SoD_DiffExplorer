using System;
using System.Linq;
using SoD_DiffExplorer.menuutils;
using System.Collections.Generic;
using System.IO;

namespace SoD_DiffExplorer.addressableCompare
{
	class AddressableComparer
	{
		private ACConfig config;

		public AddressableComparer(ACConfig config) {
			this.config = config;
		}

		private void RunAddressableComparison() {
			List<string> addressablesFrom = config.GetSourceEntryList(config.sourceTypeFrom, config.sourceFrom);
			List<string> addressablesTo = config.GetSourceEntryList(config.sourceTypeTo, config.sourceTo);

			config.ManageMakeFile(addressablesFrom, config.sourceTypeFrom, config.sourceFrom);
			config.ManageMakeFile(addressablesTo, config.sourceTypeTo, config.sourceTo);

			CompareResultImpl result = new CompareResultImpl(addressablesFrom, addressablesTo);
			if(config.resultConfig.makeFile) {
				string resultDirectory = config.GetResultDirectory();
				string resultFile = config.GetResultFile();
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
			string[] options = new string[]{
				"Adjust Configuration",
				"Run Comparison"
			};
			string backtext = "Back to Main Menu";
			int spacing = 1;

			int selection = 0;
			while(true) {
				selection = MenuUtils.OpenSelectionMenu(options, backtext, selection, spacing);

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
			string backText = "Back to Addressable Comparer";
			int spacing = 2;

			int selection = 0;
			while(true) {
				string[] options = GetConfigOptions();
				selection = MenuUtils.OpenSelectionMenu(options, backText, selection, spacing);

				switch(selection) {
					case 0:
						config.sourceTypeFrom = Enum.Parse<ACSourceType>(MenuUtils.OpenEnumConfigEditor("sourceTypeFrom", config.sourceTypeFrom.ToString(), Enum.GetNames(typeof(ACSourceType)), 3));
						break;
					case 1:
						config.sourceTypeTo = Enum.Parse<ACSourceType>(MenuUtils.OpenEnumConfigEditor("sourceTypeTo", config.sourceTypeTo.ToString(), Enum.GetNames(typeof(ACSourceType)), 3));
						break;
					case 2:
						OpenSourceConfigMenu(config.sourceFrom, "sourceFrom", config.sourceTypeFrom.ToString());
						break;
					case 3:
						OpenSourceConfigMenu(config.sourceTo, "sourceTo", config.sourceTypeTo.ToString());
						break;
					case 4:
						OpenLocalSourcesConfigMenu();
						break;
					case 5:
						OpenResultConfigMenu();
						break;
					case 6:
						Console.Clear();
						config.SaveConfig();
						break;
					case 7:
						return;
				}
			}
		}

		private void OpenSourceConfigMenu(ACSourceConfigImpl sourceConfigImpl, string sourceName, string sourceType) {
			string backText = "Back to Addressable Config Menu";
			string header = "Currently editing " + sourceName + " sourceType is: " + sourceType;
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = GetSourceConfigImplOptions(sourceConfigImpl);
				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						sourceConfigImpl.online.platform = MenuUtils.OpenSimpleConfigEditor("online.platform", sourceConfigImpl.online.platform);
						break;
					case 1:
						sourceConfigImpl.online.version = MenuUtils.OpenSimpleConfigEditor("online.version", sourceConfigImpl.online.version);
						break;
					case 2:
						sourceConfigImpl.online.makeFile = !sourceConfigImpl.online.makeFile;
						break;
					case 3:
						sourceConfigImpl.online.makeLastCreated = !sourceConfigImpl.online.makeLastCreated;
						break;
					case 4:
						sourceConfigImpl.local.platform = MenuUtils.OpenSimpleConfigEditor("local.platform", sourceConfigImpl.local.platform);
						break;
					case 5:
						sourceConfigImpl.local.version = MenuUtils.OpenSimpleConfigEditor("local.version", sourceConfigImpl.local.version);
						break;
					case 6:
						sourceConfigImpl.local.date = MenuUtils.OpenSimpleConfigEditor("local.date", sourceConfigImpl.local.date);
						break;
					case 7:
						return;
				}
			}
		}

		private void OpenLocalSourcesConfigMenu() {
			string backText = "Back to Addressable Config Menu";
			string header = "Currently editing localSourcesConfig";
			int spacing = 3;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"toggle appendPlatform (" + config.localSourcesConfig.appendPlatform + ")",
					"toggle appendVersion (" + config.localSourcesConfig.appendVersion + ")",
					"toggle appendDate (" + config.localSourcesConfig.appendDate + ")\n"
				};

				selection = MenuUtils.OpenSelectionMenu(options, backText, header, selection, spacing);

				switch(selection) {
					case 0:
						config.localSourcesConfig.appendPlatform = !config.localSourcesConfig.appendPlatform;
						break;
					case 1:
						config.localSourcesConfig.appendVersion = !config.localSourcesConfig.appendVersion;
						break;
					case 2:
						config.localSourcesConfig.appendDate = !config.localSourcesConfig.appendDate;
						break;
					case 3:
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

		private string[] GetConfigOptions() {
			return new string[] {
				"change sourceTypeFrom (" + config.sourceTypeFrom.ToString() + ")",
				"change sourceTypeTo (" + config.sourceTypeTo.ToString() + ")",
				"adjust sourceFrom (" + GetSourceInfoString(config.sourceTypeFrom, config.sourceFrom) + ")",
				"adjust sourceTo (" + GetSourceInfoString(config.sourceTypeTo, config.sourceTo) + ")",
				"adjust localSourcesConfig (" + GetLocalSourcesConfigInfoString() + ")",
				"adjust resultConfig (" + GetResultConfigInfoString() + ")\n",
				"save config\n"
			};
		}

		private string[] GetSourceConfigImplOptions(ACSourceConfigImpl sourceConfigImpl) {
			return new string[] {
				"change online.platform (" + sourceConfigImpl.online.platform + ")",
				"change online.version (" + sourceConfigImpl.online.version + ")",
				"toggle online.makeFile (" + sourceConfigImpl.online.makeFile + ")",
				"toggle online.makeLastCreated (" + sourceConfigImpl.online.makeLastCreated + ")\n",
				"change local.platform (" + sourceConfigImpl.local.platform + ")",
				"change local.version (" + sourceConfigImpl.local.version + ")",
				"change local.date (" + sourceConfigImpl.local.date + ")\n"
			};
		}

		private string GetSourceInfoString(ACSourceType sourceType, ACSourceConfigImpl configImpl) {
			List<string> values = new List<string>();
			if(sourceType == ACSourceType.online) {
				ACOnlineSource acosConfig = configImpl.online;
				values.Add("platform = " + acosConfig.platform);
				values.Add("version = " + acosConfig.version);
				values.Add("makeFile = " + acosConfig.makeFile);
				if(acosConfig.makeFile) {
					values.Add("makeLastCreated = " + acosConfig.makeLastCreated);
				}
			} else if(sourceType == ACSourceType.local) {
				ACLocalSource aclsConfig = configImpl.local;
				if(config.localSourcesConfig.appendPlatform) {
					values.Add(aclsConfig.platform);
				}
				if(config.localSourcesConfig.appendVersion) {
					values.Add(aclsConfig.version);
				}
				if(config.localSourcesConfig.appendDate) {
					values.Add(aclsConfig.date);
				}
			} else if(sourceType == ACSourceType.lastcreated) {
				values.Add(configImpl.lastcreated);
			}
			return string.Join(" | ", values);
		}

		private string GetLocalSourcesConfigInfoString() {
			ACLocalSourceConfig aclsConfig = config.localSourcesConfig;
			return "appendPlatform = " + aclsConfig.appendPlatform + " | appendVersion = " + aclsConfig.appendVersion + " | appendDate = " + aclsConfig.appendDate;
		}

		private string GetResultConfigInfoString() {
			ACResultConfig acrConfig = config.resultConfig;
			return "makeFile = " + acrConfig.makeFile + " | appendDate = " + acrConfig.appendDate;
		}
	}
}

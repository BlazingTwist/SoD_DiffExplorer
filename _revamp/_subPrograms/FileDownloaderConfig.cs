using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer._revamp._config._sourceConfig;
using SoD_DiffExplorer._revamp._config._downloadSettings;
using SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig;

namespace SoD_DiffExplorer._revamp._subPrograms
{
	class FileDownloaderConfig : YamlObject, IMenuObject
	{
		[YamlIgnore]
		IOnlineAddressDictConfig addressDictSupplier = null;

		public IMenuPropertyAccessor<SimpleOnlineSourcesConfig> onlineSourcesConfig = new MenuOptionProperty<SimpleOnlineSourcesConfig>(nameof(onlineSourcesConfig), new MenuPropertyObjectBehavior<SimpleOnlineSourcesConfig>());
		public IMenuPropertyAccessor<DownloadSettings> downloadSettings = new MenuOptionProperty<DownloadSettings>(nameof(downloadSettings), new MenuPropertyObjectBehavior<DownloadSettings>());
		public IMenuPropertyAccessor<DownloaderInterpreterConfig > interpreterConfig = new MenuOptionProperty<DownloaderInterpreterConfig >(nameof(interpreterConfig), new MenuPropertyObjectBehavior<DownloaderInterpreterConfig>());

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			this.addressDictSupplier = addressDictSupplier;
			onlineSourcesConfig.GetValue().Init(addressDictSupplier);
		}

		public string buildOutputDirectory(string fileName) {
			List<string> splittedPath = new List<string>();
			splittedPath.Add(downloadSettings.GetValue().targetDirectory.GetValue());

			if(downloadSettings.GetValue().appendPlatform.GetValue()) {
				splittedPath.Add(onlineSourcesConfig.GetValue().platform.GetValue());
			}

			if(downloadSettings.GetValue().appendVersion.GetValue()) {
				splittedPath.Add(onlineSourcesConfig.GetValue().version.GetValue());
			}

			if(downloadSettings.GetValue().appendDate.GetValue()) {
				splittedPath.Add(DateTime.Now.ToString("yyyy.MM.dd"));
			}

			splittedPath.AddRange(fileName.Split("/"));

			return Path.Combine(splittedPath.GetRange(0, splittedPath.Count - 1).ToArray());
		}

		public string GetFileAddress(string fileName) {
			foreach(KeyValuePair<string, string> pair in addressDictSupplier.GetOnlineAddressDict()) {
				if(fileName.StartsWith(pair.Key)) {
					fileName = pair.Value + fileName.Substring(pair.Key.Length);
					break;
				}
			}
			return GetFullBaseUrl() + fileName;
		}

		public string GetFullBaseUrl() {
			return string.Join('/',
				onlineSourcesConfig.GetValue().baseUrl,
				onlineSourcesConfig.GetValue().platform,
				onlineSourcesConfig.GetValue().version,
				onlineSourcesConfig.GetValue().baseUrlSuffix
				);
		}

		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
				{nameof(onlineSourcesConfig), onlineSourcesConfig.GetValue()},
				{nameof(downloadSettings), downloadSettings.GetValue()},
				{nameof(interpreterConfig), interpreterConfig.GetValue()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeYamlObjects(ref lines, startLine, ref endLine, currentTabDepth, GetObjectChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
				nameof(onlineSourcesConfig),
				nameof(downloadSettings),
				nameof(interpreterConfig)
				);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
				onlineSourcesConfig,
				downloadSettings,
				interpreterConfig
			};
		}
	}
}

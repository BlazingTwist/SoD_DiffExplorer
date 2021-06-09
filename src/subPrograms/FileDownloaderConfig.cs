using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.downloadSettings;
using SoD_DiffExplorer.config.onlineSourceInterpreterConfig;
using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.config.sourceConfig;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.subPrograms {
	[PublicAPI]
	public class FileDownloaderConfig : YamlObject, IMenuObject {
		[YamlIgnore] private IOnlineAddressDictConfig addressDictSupplier;

		public IMenuPropertyAccessor<SimpleOnlineSourcesConfig> onlineSourcesConfig = new MenuOptionProperty<SimpleOnlineSourcesConfig>(
				nameof(onlineSourcesConfig),
				new MenuPropertyObjectBehavior<SimpleOnlineSourcesConfig>());

		public IMenuPropertyAccessor<DownloadSettings> downloadSettings = new MenuOptionProperty<DownloadSettings>(
				nameof(downloadSettings),
				new MenuPropertyObjectBehavior<DownloadSettings>());

		public IMenuPropertyAccessor<DownloaderInterpreterConfig> interpreterConfig = new MenuOptionProperty<DownloaderInterpreterConfig>(
				nameof(interpreterConfig),
				new MenuPropertyObjectBehavior<DownloaderInterpreterConfig>());

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			this.addressDictSupplier = addressDictSupplier;
			onlineSourcesConfig.GetValue().Init(addressDictSupplier);
		}

		public string buildOutputDirectory(string fileName) {
			List<string> pathSplit = new List<string> { downloadSettings.GetValue().targetDirectory.GetValue() };

			if (downloadSettings.GetValue().appendPlatform.GetValue()) {
				pathSplit.Add(onlineSourcesConfig.GetValue().platform.GetValue());
			}

			if (downloadSettings.GetValue().appendVersion.GetValue()) {
				pathSplit.Add(onlineSourcesConfig.GetValue().version.GetValue());
			}

			if (downloadSettings.GetValue().appendDate.GetValue()) {
				pathSplit.Add(DateTime.Now.ToString("yyyy.MM.dd"));
			}

			pathSplit.AddRange(fileName.Split("/"));

			return Path.Combine(pathSplit.GetRange(0, pathSplit.Count - 1).ToArray());
		}

		public string GetFileAddress(string fileName) {
			foreach ((string key, string value) in addressDictSupplier.GetOnlineAddressDict()) {
				if (fileName.StartsWith(key)) {
					fileName = value + fileName.Substring(key.Length);
					break;
				}
			}

			return GetFullBaseUrl() + "/" + fileName;
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
					{ nameof(onlineSourcesConfig), onlineSourcesConfig.GetValue() },
					{ nameof(downloadSettings), downloadSettings.GetValue() },
					{ nameof(interpreterConfig), interpreterConfig.GetValue() }
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
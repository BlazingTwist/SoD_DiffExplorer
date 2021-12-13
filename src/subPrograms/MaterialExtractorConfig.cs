using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.downloadSettings;
using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.config.sourceConfig;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.subPrograms {
	[PublicAPI]
	public class MaterialExtractorConfig : YamlObject, IMenuObject {
		[YamlIgnore] private IOnlineAddressDictConfig addressDictSupplier;

		public IMenuPropertyAccessor<SimpleOnlineSourcesConfig> onlineSourcesConfig = new MenuOptionProperty<SimpleOnlineSourcesConfig>(
				nameof(onlineSourcesConfig),
				new MenuPropertyObjectBehavior<SimpleOnlineSourcesConfig>());

		public IMenuPropertyAccessor<DownloadSettings> downloadSettings = new MenuOptionProperty<DownloadSettings>(
				nameof(downloadSettings),
				new MenuPropertyObjectBehavior<DownloadSettings>());

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			this.addressDictSupplier = addressDictSupplier;
			onlineSourcesConfig.GetValue().Init(addressDictSupplier);
		}

		public string GetResultFile(string baseFileName) {
			if (downloadSettings.GetValue().appendDate.GetValue()) {
				baseFileName += "_" + DateTime.Now.ToString("yyyy.MM.dd");
			}
			if (downloadSettings.GetValue().appendPlatform.GetValue()) {
				baseFileName += "_" + onlineSourcesConfig.GetValue().platform.GetValue();
			}
			if (downloadSettings.GetValue().appendVersion.GetValue()) {
				baseFileName += "_" + onlineSourcesConfig.GetValue().version.GetValue();
			}
			baseFileName += ".txt";
			return Path.Combine(downloadSettings.GetValue().targetDirectory.GetValue(), baseFileName);
		}
		
		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
					{ nameof(onlineSourcesConfig), onlineSourcesConfig.GetValue() },
					{ nameof(downloadSettings), downloadSettings.GetValue() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeYamlObjects(ref lines, startLine, ref endLine, currentTabDepth, GetObjectChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
					nameof(onlineSourcesConfig),
					nameof(downloadSettings)
			);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
					onlineSourcesConfig,
					downloadSettings
			};
		}
	}
}
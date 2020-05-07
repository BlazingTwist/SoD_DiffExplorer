using SoD_DiffExplorer.menu;
using System.Collections.Generic;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer._revamp._config._programConfig;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class SimpleOnlineSourcesConfig : YamlObject, IMenuObject, IOnlineUrlHolder, IOnlineSourceHolder
	{
		public IMenuPropertyAccessor<string> baseUrl = new MenuOptionProperty<string>(nameof(baseUrl), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<string> platform = new MenuOptionProperty<string>(nameof(platform), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<string> version = new MenuOptionProperty<string>(nameof(version), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<string> baseUrlSuffix = new MenuOptionProperty<string>(nameof(baseUrlSuffix), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<EOnlineDataType> dataType = new MenuOptionPropertyEnum<EOnlineDataType>(nameof(dataType), new MenuPropertyEnumSelectionBehavior<EOnlineDataType>());
		public IOnlineSourcesConfig onlineSourceConfig = null;

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			onlineSourceConfig.Init(addressDictSupplier);
		}

		public Queue<string> GetDataFileURLs() {
			return onlineSourceConfig.GetDataFileURLs(this, this);
		}

		string IOnlineSourceHolder.GetPlatform() {
			return platform.GetValue();
		}

		string IOnlineSourceHolder.GetVersion() {
			return version.GetValue();
		}

		string IOnlineUrlHolder.GetBaseUrl() {
			return baseUrl.GetValue();
		}

		string IOnlineUrlHolder.GetBaseUrlSuffix() {
			return baseUrlSuffix.GetValue();
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(platform), platform.ToString()},
				{nameof(version), version.ToString()},
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
				nameof(platform) + " = " + platform.ToString(),
				nameof(version) + " = " + version.ToString()
				);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
				platform,
				version
			};
		}
	}
}

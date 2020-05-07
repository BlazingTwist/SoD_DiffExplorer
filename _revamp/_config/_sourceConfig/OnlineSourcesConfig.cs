using System.Collections.Generic;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer._revamp._config._programConfig;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class OnlineSourcesConfig : YamlObject, IMenuObject, IOnlineUrlHolder
	{
		public string baseUrl = null;
		public string baseUrlSuffix = null;
		public EOnlineDataType dataType = 0;
		public IOnlineSourcesConfig onlineSourceConfig = null;

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			onlineSourceConfig.Init(addressDictSupplier);
		}

		public Queue<string> GetDataFileURLs(OnlineSource onlineSource) {
			return onlineSourceConfig.GetDataFileURLs(this, onlineSource);
		}

		string IOnlineUrlHolder.GetBaseUrl() {
			return baseUrl;
		}

		string IOnlineUrlHolder.GetBaseUrlSuffix() {
			return baseUrlSuffix;
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return true;
		}

		string IMenuObject.GetInfoString() {
			return "no menu options, adjust the config file instead.";
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[0];
		}
	}
}

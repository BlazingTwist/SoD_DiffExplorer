using System.Collections.Generic;
using SoD_DiffExplorer._revamp._config._programConfig;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class DirectOnlineSourcesConfig : IOnlineSourcesConfig
	{
		public string dataContainer = null;

		void IOnlineSourcesConfig.Init(IOnlineAddressDictConfig addressDictSupplier) {
			//useless data for this type of onlineSourcesConfig
		}

		Queue<string> IOnlineSourcesConfig.GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			Queue<string> result = new Queue<string>();
			result.Enqueue(string.Join('/', urlHolder.GetBaseUrl(), sourceHolder.GetPlatform(), sourceHolder.GetVersion(), urlHolder.GetBaseUrlSuffix(), dataContainer));
			return result;
		}
	}
}

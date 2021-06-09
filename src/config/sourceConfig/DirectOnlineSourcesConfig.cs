using System.Collections.Generic;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.programConfig;

namespace SoD_DiffExplorer.config.sourceConfig {
	[PublicAPI]
	public class DirectOnlineSourcesConfig : IOnlineSourcesConfig {
		public string dataContainer;

		void IOnlineSourcesConfig.Init(IOnlineAddressDictConfig addressDictSupplier) {
			//useless data for this type of onlineSourcesConfig
		}

		Queue<string> IOnlineSourcesConfig.GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			Queue<string> result = new Queue<string>();
			result.Enqueue(string.Join('/', urlHolder.GetBaseUrl(), sourceHolder.GetPlatform(), sourceHolder.GetVersion(), urlHolder.GetBaseUrlSuffix(),
					dataContainer));
			return result;
		}
	}
}
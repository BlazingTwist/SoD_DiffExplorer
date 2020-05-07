using SoD_DiffExplorer._revamp._config._programConfig;
using System.Collections.Generic;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	interface IOnlineSourcesConfig
	{
		public void Init(IOnlineAddressDictConfig addressDictSupplier);

		public Queue<string> GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder);
	}
}

using System.Collections.Generic;
using SoD_DiffExplorer.config.programConfig;

namespace SoD_DiffExplorer.config.sourceConfig {
	public interface IOnlineSourcesConfig {
		public void Init(IOnlineAddressDictConfig addressDictSupplier);

		public Queue<string> GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder);
	}
}
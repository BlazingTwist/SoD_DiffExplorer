using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.config.programConfig {
	public interface IOnlineAddressDictConfig {
		public BetterDict<string, string> GetOnlineAddressDict();
	}
}
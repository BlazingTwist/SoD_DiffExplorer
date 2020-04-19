using SoD_DiffExplorer.filedownloader;
using SoD_DiffExplorer.addressablecompare;
using SoD_DiffExplorer.fireballcompare;
using SoD_DiffExplorer.squadtacticscompare;
using SoD_DiffExplorer.flightstatcompare;
using SoD_DiffExplorer.timedmissioncompare;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menuutils;
using System;

namespace SoD_DiffExplorer
{
	class ConfigHolder
	{
		public BetterDict<ConsoleKey, MenuControl> menuControlMapping = null;
		public BetterDict<string, string> onlineAddressDict = null;
		public FDConfig fileDownloaderConfig = null;
		public ACConfig addressableCompareConfig = null;
		public FCConfig fireballCompareConfig = null;
		public STCConfig squadTacticsCompareConfig = null;
		public FSCConfig flightStatsCompareConfig = null;
		public TMCConfig timedMissionCompareConfig = null;

		public void Initialize() {
			fileDownloaderConfig.configHolder = this;
			flightStatsCompareConfig.configHolder = this;
		}
	}
}

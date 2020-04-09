using SoD_DiffExplorer.filedownloader;
using SoD_DiffExplorer.addressablecompare;
using SoD_DiffExplorer.fireballcompare;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menuutils;
using System;

namespace SoD_DiffExplorer
{
	class ConfigHolder
	{
		public BetterDict<ConsoleKey, MenuControl> menuControlMapping;
		public FDConfig fileDownloaderConfig = null;
		public ACConfig addressableCompareConfig = null;
		public FCConfig fireballCompareConfig = null;
	}
}

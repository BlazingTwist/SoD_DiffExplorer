using System;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer.filedownloader;
using SoD_DiffExplorer.addressablecompare;
using SoD_DiffExplorer.fireballcompare;
using SoD_DiffExplorer.squadtacticscompare;
using SoD_DiffExplorer.menuutils;

namespace SoD_DiffExplorer
{
	class Program
	{
		public static ConfigHolder config;
		public static MenuUtils menuUtils;
		public static FileDownloader fileDownloader;
		public static AddressableComparer addressableComparer;
		public static FireballComparer fireballComparer;
		public static SquadTacticsComparer squadTacticsComparer;

		static void Main(string[] args) {
			try {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				using(StreamReader reader = File.OpenText("config.yaml")) {
					config = deserializer.Deserialize<ConfigHolder>(reader);
					config.Initialize();
				}
			} catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.WriteLine(e.StackTrace);
				return;
			}

			menuUtils = new MenuUtils(config.menuControlMapping);
			fileDownloader = new FileDownloader(config.fileDownloaderConfig, menuUtils);
			addressableComparer = new AddressableComparer(config.addressableCompareConfig, menuUtils);
			fireballComparer = new FireballComparer(config.fireballCompareConfig, menuUtils);
			squadTacticsComparer = new SquadTacticsComparer(config.squadTacticsCompareConfig, menuUtils);

			try{
				OpenMainMenu();
			}catch(Exception e) {
				Console.WriteLine("caught exception!");
				Console.WriteLine(e.ToString());
				Console.WriteLine(e.StackTrace);
				Console.WriteLine("Waiting for input...");
				Console.ReadKey(true);
			}
		}

		static void OpenMainMenu() {
			string[] options = new string[]{
				"Open FileDownloader",
				"Open AddressableComparer",
				"Open FireballStatComparer",
				"Open SquadTacticsStatComparer",
				"Open FlightStatComparer (not yet implemented)"
			};
			string backText = "quit";
			int spacing = 0;

			int selection = 0;
			while(true) {
				selection = menuUtils.OpenSelectionMenu(options, backText, selection, spacing);

				switch(selection) {
					case 0:
						fileDownloader.OpenFileDownloaderMenu();
						break;
					case 1:
						addressableComparer.OpenAddressableComparerMenu();
						break;
					case 2:
						fireballComparer.OpenFireballComparerMenu();
						break;
					case 3:
						squadTacticsComparer.OpenSquadTacticsComparerMenu();
						break;
					case 4:
						//TODO
						break;
					case 5:
						return;
				}
			}
		}
	}
}

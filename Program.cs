using System;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer.filedownloader;
using SoD_DiffExplorer.addressableCompare;
using SoD_DiffExplorer.menuutils;

namespace SoD_DiffExplorer
{
	class Program
	{
		public static ConfigHolder config;
		public static FileDownloader fileDownloader;
		public static AddressableComparer addressableComparer;

		static void Main(string[] args) {
			try {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				using(StreamReader reader = File.OpenText("config.yaml")) {
					config = deserializer.Deserialize<ConfigHolder>(reader);
				}
			} catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.WriteLine(e.StackTrace);
				return;
			}

			fileDownloader = new FileDownloader(config.fileDownloaderConfig);
			addressableComparer = new AddressableComparer(config.addressableCompareConfig);

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
				"Open FireballStatComparer (not yet implemented)",
				"Open SquadTacticsStatComparer (not yet implemented)",
				"Open FlightStatComparer (not yet implemented)"
			};
			string backText = "quit";
			int spacing = 0;

			int selection = 0;
			while(true) {
				selection = MenuUtils.OpenSelectionMenu(options, backText, selection, spacing);

				switch(selection) {
					case 0:
						fileDownloader.OpenFileDownloaderMenu();
						break;
					case 1:
						addressableComparer.OpenAddressableComparerMenu();
						break;
					case 2:
						//TODO
						break;
					case 3:
						//TODO
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

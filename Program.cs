using System;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer._revamp._subPrograms;

namespace SoD_DiffExplorer
{
	class Program
	{
		public static ConfigHolder config;

		static void Main(string[] args) {
			try {
				IDeserializer deserializer = new DeserializerBuilder()
					.WithTagMapping("tag:yaml.org,2002:fileDownloader", typeof(FileDownloader))
					.WithTagMapping("tag:yaml.org,2002:comparer", typeof(DataComparer))
					.Build();
				using(StreamReader reader = File.OpenText("config.yaml")) {
					config = deserializer.Deserialize<ConfigHolder>(reader);
				}
				if(!config.Initialize()) {
					return;
				}
			} catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.ReadKey(true);
				return;
			}

			try {
				config.OpenMainMenu();
			} catch(Exception e) {
				Console.WriteLine("caught exception!");
				Console.WriteLine(e.ToString());
				Console.WriteLine("Waiting for input...");
				Console.ReadKey(true);
			}
		}
	}
}

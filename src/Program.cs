using System;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer.subPrograms;

namespace SoD_DiffExplorer {
	public static class Program {
		private static ConfigHolder config;

		private static void Main() {
			try {
				IDeserializer deserializer = new DeserializerBuilder()
						.WithTagMapping("tag:yaml.org,2002:fileDownloader", typeof(FileDownloader))
						.WithTagMapping("tag:yaml.org,2002:comparer", typeof(DataComparer))
						.Build();
				using (StreamReader reader = File.OpenText("config.yaml")) {
					config = deserializer.Deserialize<ConfigHolder>(reader);
				}

				if (!config.Initialize()) {
					return;
				}
			} catch (Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e);
				Console.ReadKey(true);
				return;
			}

			try {
				config.OpenMainMenu();
			} catch (Exception e) {
				Console.WriteLine("caught exception!");
				Console.WriteLine(e.ToString());
				Console.WriteLine("Waiting for input...");
				Console.ReadKey(true);
			}
		}
	}
}
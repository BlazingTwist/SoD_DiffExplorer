using System;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer.filedownloader;
using System.Linq;

namespace SoD_DiffExplorer
{
	class Program
	{
		static void Main(string[] args) {
			//DateTime date = DateTime.Now;
			//Console.WriteLine(date.ToString("yyyy.MM.dd"));

			ConfigHolder config;
			try {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				using(StreamReader reader = File.OpenText("config.yaml")) {
					config = deserializer.Deserialize<ConfigHolder>(reader);
				}
				ISerializer serializer = new SerializerBuilder().Build();
				Console.WriteLine(serializer.Serialize(config));
				Console.ReadKey();
			} catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.WriteLine(e.StackTrace);
				return;
			}
			Console.WriteLine("doDownload = " + config.fileDownloaderConfig.doDownload);
			Console.WriteLine("baseUrl = " + config.fileDownloaderConfig.downloadURL.baseURL);
			Console.WriteLine("baseDirectory = " + config.fileDownloaderConfig.outputDirectory.baseDirectory);
			Console.WriteLine(string.Join(", ", config.fileDownloaderConfig.regexFilters));
			Console.WriteLine(string.Join(", ", config.fileDownloaderConfig.localeFilters));
			Console.WriteLine("dict = " + string.Join(", ", config.fileDownloaderConfig.onlineAddressDict.Select(item => item.Key + " = " + item.Value)));

			FileDownloader downloader = new FileDownloader(config.fileDownloaderConfig);
			downloader.OpenFileDownloaderMenu();

			/*String configString = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "app.conf"));
			Akka.Configuration.Config config = Akka.Configuration.ConfigurationFactory.ParseString(configString);
			Console.WriteLine(config.ToString());*/

		}
	}
}

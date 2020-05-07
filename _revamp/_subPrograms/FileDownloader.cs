using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer._revamp._config._sourceConfig;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.ObjectFactories;

namespace SoD_DiffExplorer._revamp._subPrograms
{
	class FileDownloader : ISubProgram
	{
		[YamlIgnore]
		private FileDownloaderConfig fileDownloader = null;

		[YamlIgnore]
		private MenuUtils menuUtils = null;

		public string configFilePath = null;
		public string programName = null;
		public string runText = null;
		public string backText = null;

		string ISubProgram.GetProgramName() {
			return programName;
		}

		bool ISubProgram.Init(MenuUtils menuUtils, ProgramConfig programConfig) {
			this.menuUtils = menuUtils;
			try {
				IDeserializer deserializer = new DeserializerBuilder()
					.WithTagMapping("tag:yaml.org,2002:directOnlineSourceConfig", typeof(DirectOnlineSourcesConfig))
					.WithTagMapping("tag:yaml.org,2002:queriedOnlineSourceConfig", typeof(QueriedOnlineSourceConfig))
					.WithNodeDeserializer(
						inner => new MenuObjectDeserializer(inner, new DefaultObjectFactory()),
						s => s.InsteadOf<ObjectNodeDeserializer>()
					)
					.Build();
				using(StreamReader reader = new StreamReader(configFilePath)) {
					fileDownloader = deserializer.Deserialize<FileDownloaderConfig>(reader);
				}

				fileDownloader.Init(programConfig);
				return true;
			}catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.ReadKey(true);
				return false;
			}
		}

		private void Run() {
			foreach(string fileName in BuildDownloadFileNames()) {
				DownloadFile(fileName);
			}
			Console.WriteLine("Press any key to continue.");
			Console.ReadKey(true);
		}

		private void DownloadFile(string fileName) {
			if(!fileDownloader.downloadSettings.GetValue().doDownload.GetValue()) {
				Console.WriteLine("would download: " + fileName);
				return;
			}
			string fileAddress = fileDownloader.GetFileAddress(fileName);
			string targetDirectory = fileDownloader.buildOutputDirectory(fileName);
			Console.WriteLine("downloading File: " + fileName);
			Console.WriteLine("from address: " + fileAddress);
			Console.WriteLine("to directory: " + targetDirectory);

			using(WebClient client = new WebClient()) {
				if(!Directory.Exists(targetDirectory)) {
					Console.WriteLine("\thad to create the directory.");
					Directory.CreateDirectory(targetDirectory);
				}
				try {
					Console.WriteLine("\tstarting download...");
					string[] fileNameSplit = fileName.Split('/');
					client.DownloadFile(fileAddress, Path.Combine(targetDirectory, fileNameSplit[fileNameSplit.Length - 1]));
				} catch(WebException) {
					Console.WriteLine("\tfailed to download file!");
					if(fileDownloader.downloadSettings.GetValue().pauseDownloadOnError.GetValue()) {
						Console.WriteLine("\twaiting for user acknowledgement. Press any key to continue...");
						Console.ReadKey(true);
					}
				}
			}

			//extra line for formatting
			Console.WriteLine();
		}

		private List<string> BuildDownloadFileNames() {
			EOnlineDataType onlineDataType = fileDownloader.onlineSourcesConfig.GetValue().dataType.GetValue();
			if(onlineDataType == EOnlineDataType.xmlFile) {
				return fileDownloader.interpreterConfig.GetValue().BuildXMLContent(fileDownloader.onlineSourcesConfig.GetValue().GetDataFileURLs());
			}else if(onlineDataType == EOnlineDataType.bundleFile) {
				return fileDownloader.interpreterConfig.GetValue().BuildBundleContent(fileDownloader.onlineSourcesConfig.GetValue().GetDataFileURLs());
			} else {
				throw new InvalidOperationException("OnlineDataType " + onlineDataType.ToString() + " not supported by FileDownloader!");
			}
		}

		private void SaveConfig() {
			List<string> lines = YamlUtils.GetAllConfigLines(configFilePath);
			int endLine = lines.Count - 1;
			if(!(fileDownloader as YamlObject).Save(ref lines, 0, ref endLine, 0)) {
				Console.WriteLine("failed to save config!");
				return;
			}

			Console.WriteLine("updating config values successful, writing to file...");
			using(StreamWriter writer = new StreamWriter(configFilePath, false)) {
				lines.ForEach(line => writer.WriteLine(line));
			}
			Console.WriteLine("successfully saved config");
		}

		void ISubProgram.OpenMainMenu(int spacing) {
			string[] options = new string[] {
				"Adjust Configuration",
				"Save Config",
				runText
			};

			int selection = 0;
			while(true) {
				selection = menuUtils.OpenSelectionMenu(options, backText, programName, selection, spacing);

				switch(selection) {
					case 0:
						menuUtils.OpenObjectMenu(programName, nameof(fileDownloader), fileDownloader, 0, spacing + MenuUtils.spacerWidth);
						break;
					case 1:
						SaveConfig();
						Console.ReadKey(true);
						break;
					case 2:
						Console.Clear();
						Run();
						break;
					case 3:
						return;
				}
			}
		}
	}
}

using SoD_DiffExplorer.menu;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.IO;
using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer._revamp._config._resultConfig;
using SoD_DiffExplorer.csutils;
using System.Linq;
using SoD_DiffExplorer._revamp._config._sourceConfig;
using SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.NodeDeserializers;

namespace SoD_DiffExplorer._revamp._subPrograms
{
	class DataComparer : ISubProgram
	{
		[YamlIgnore]
		private DataComparerConfig dataComparer = null;

		[YamlIgnore]
		private MenuUtils menuUtils = null;

		[YamlIgnore]
		public ConfigMapResult configMapResult = null;

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
					.WithTagMapping("tag:yaml.org,2002:storedMappingValue", typeof(StoredMappingValue))
					.WithTagMapping("tag:yaml.org,2002:fileUrlBasedMappingValue", typeof(FileUrlBasedMappingValue))
					.WithNodeDeserializer(
						inner => new MenuObjectDeserializer(inner, new DefaultObjectFactory()),
						s => s.InsteadOf<ObjectNodeDeserializer>()
					)
					.Build();
				using(StreamReader reader = File.OpenText(configFilePath)) {
					dataComparer = deserializer.Deserialize<DataComparerConfig>(reader);
				}

				dataComparer.Init(programConfig);
				return true;
			}catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.ReadKey(true);
				return false;
			}
		}

		private void Run() {
			Dictionary<string, Dictionary<string, List<string>>> valuesFrom = BuildContent(dataComparer.sourceConfigHolder.GetValue().sourceFrom.GetValue());
			Dictionary<string, Dictionary<string, List<string>>> valuesTo = BuildContent(dataComparer.sourceConfigHolder.GetValue().sourceTo.GetValue());
			configMapResult = new ConfigMapResult();
			configMapResult.LoadValuesFrom(valuesFrom);
			configMapResult.LoadValuesTo(valuesTo);
			ManageMakeFile(valuesFrom, dataComparer.sourceConfigHolder.GetValue().sourceFrom.GetValue());
			ManageMakeFile(valuesTo, dataComparer.sourceConfigHolder.GetValue().sourceTo.GetValue());
			string result = configMapResult.BuildResult(GetSecondaryResultKeys(), dataComparer.resultConfig.GetValue().resultFilter.GetValue());

			if(dataComparer.resultConfig.GetValue().makeFile.GetValue()) {
				string resultFile = GetResultFile();
				string resultDirectory = Path.GetDirectoryName(resultFile);
				if(!Directory.Exists(resultDirectory)) {
					Console.WriteLine("had to create directory: " + resultDirectory);
					Directory.CreateDirectory(resultDirectory);
				}
				using(StreamWriter writer = new StreamWriter(resultFile, false)) {
					writer.WriteLine(result);
				}
			}
			Console.WriteLine(result);
			Console.WriteLine("Press any key to continue.");
			Console.ReadKey(true);
		}

		private void ManageMakeFile(Dictionary<string, Dictionary<string, List<string>>> data, SourceConfig sourceConfig) {
			if(sourceConfig.sourceType.GetValue() == ESourceType.online && sourceConfig.online.GetValue().makeFile.GetValue()) {
				Console.WriteLine("parsing data to file from onlineSource...");
				string targetMakeFile = dataComparer.sourceConfigHolder.GetValue().GetLocalSourceFile(sourceConfig);
				string targetMakeFileDirectory = Path.GetDirectoryName(targetMakeFile);

				if(!Directory.Exists(targetMakeFileDirectory)) {
					Console.WriteLine("had to create directory: " + targetMakeFileDirectory);
					Directory.CreateDirectory(targetMakeFileDirectory);
				}

				using(StreamWriter writer = new StreamWriter(targetMakeFile, false)) {
					Console.WriteLine("writing to file: " + targetMakeFile);
					ISerializer serializer = new SerializerBuilder().Build();
					string yamltext = serializer.Serialize(data);
					writer.WriteLine(yamltext);
				}

				if(sourceConfig.online.GetValue().makeLastCreated.GetValue()) {
					Console.WriteLine("setting lastCreated to: " + targetMakeFile);
					SaveLastCreated(targetMakeFile);
				}
			}
		}

		private List<string> GetSecondaryResultKeys() {
			List<string> allowedValues = new List<string>();
			allowedValues.Add(dataComparer.onlineSourceInterpreterConfig.GetValue().mapConfigBy.GetOutputName());
			allowedValues.AddRange(dataComparer.onlineSourceInterpreterConfig.GetValue().configFilter.Where(filter => filter.doDisplay).Select(filter => filter.outputName));
			return allowedValues;
		}

		private Dictionary<string, Dictionary<string, List<string>>> BuildContent(SourceConfig sourceConfig) {
			if(sourceConfig.sourceType.GetValue() == ESourceType.online) {
				EOnlineDataType onlineDataType = dataComparer.sourceConfigHolder.GetValue().onlineSourcesConfig.GetValue().dataType;
				if(onlineDataType == EOnlineDataType.xmlFile) {
					return dataComparer.onlineSourceInterpreterConfig.GetValue().BuildXMLContent(dataComparer.sourceConfigHolder.GetValue().onlineSourcesConfig.GetValue().GetDataFileURLs(sourceConfig.online.GetValue()));
				} else if(onlineDataType == EOnlineDataType.bundleFile) {
					return dataComparer.onlineSourceInterpreterConfig.GetValue().BuildBundleContent(dataComparer.sourceConfigHolder.GetValue().onlineSourcesConfig.GetValue().GetDataFileURLs(sourceConfig.online.GetValue()));
				} else {
					throw new InvalidOperationException("OnlineDataType " + onlineDataType.ToString() + " not supported by DataComparer");
				}
			} else if(sourceConfig.sourceType.GetValue() == ESourceType.local) {
				return BuildLocalFileContent(dataComparer.sourceConfigHolder.GetValue().GetLocalSourceFile(sourceConfig));
			}else if(sourceConfig.sourceType.GetValue() == ESourceType.lastCreated) {
				return BuildLocalFileContent(dataComparer.sourceConfigHolder.GetValue().lastCreated.GetValue());
			} else {
				throw new InvalidOperationException("SourceType " + sourceConfig.sourceType.ToString() + " not supported!");
			}
		}

		private Dictionary<string, Dictionary<string, List<string>>> BuildLocalFileContent(string filePath) {
			using(StreamReader reader = new StreamReader(filePath)) {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				Console.WriteLine("parsing stats from local source: " + filePath);
				return deserializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(reader);
			}
		}

		private string GetResultFile() {
			string fileName = dataComparer.sourceConfigHolder.GetValue().localSourcesConfig.GetValue().targetFileName;
			if(dataComparer.resultConfig.GetValue().appendDate.GetValue()) {
				fileName += ("_" + DateTime.Now.ToString("yyyy.MM.dd"));
			}
			fileName += ("." + dataComparer.sourceConfigHolder.GetValue().localSourcesConfig.GetValue().targetFileExtension);
			return Path.Combine(dataComparer.resultConfig.GetValue().baseDirectory.GetValue(), fileName);
		}

		private void SaveLastCreated(string targetPath) {
			dataComparer.sourceConfigHolder.GetValue().lastCreated.SetValue(targetPath);

			List<string> lines = YamlUtils.GetAllConfigLines(configFilePath);
			int targetLine = YamlUtils.FindFieldIndex(lines, nameof(dataComparer.sourceConfigHolder) + "." + dataComparer.sourceConfigHolder.GetValue().lastCreated.GetFieldName());
			if(targetLine < 0 || targetLine >= lines.Count) {
				Console.WriteLine("could not save lastCreated! unable to find KeyValuePair");
				return;
			}
			lines[targetLine] = YamlUtils.UpdateLine(lines[targetLine], dataComparer.sourceConfigHolder.GetValue().lastCreated.GetFieldName(), targetPath);
			using(StreamWriter writer = new StreamWriter(configFilePath, false)) {
				lines.ForEach(line => writer.WriteLine(line));
			}
			Console.WriteLine("successfully saved lastCreated");
		}

		private void SaveConfig() {
			List<string> lines = YamlUtils.GetAllConfigLines(configFilePath);
			int endLine = lines.Count - 1;
			if(!(dataComparer as YamlObject).Save(ref lines, 0, ref endLine, 0)) {
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
						menuUtils.OpenObjectMenu(programName, nameof(dataComparer), dataComparer, 0, spacing + MenuUtils.spacerWidth);
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

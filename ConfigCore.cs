using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer
{
	class ConfigCore : IMenuObject
	{
		[YamlIgnore]
		private string configPath;

		public BetterDict<ConsoleKey, MenuControl> menuControlMapping = null;
		public BetterDict<string, string> onlineAddressDict = null;
		public MenuStyleConfig menuStyle = null;

		public static ConfigCore Init(string path) {
			try {
				IDeserializer deserializer = new DeserializerBuilder().Build();
				using(StreamReader reader = File.OpenText(path)) {
					ConfigCore core = deserializer.Deserialize<ConfigCore>(reader);
					core.configPath = path;
					return core;
				}
			} catch(Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e.ToString());
				Console.ReadKey(true);
				return null;
			}
		}

		public string GetInfoString() {
			return menuStyle.ToString();
		}

		public IMenuProperty[] GetOptions() {
			List<IMenuProperty> options = new List<IMenuProperty>();
			options.AddRange(menuStyle.GetOptions());
			options.Add(new MenuOption("Config", MenuUtils.GetSaveString, () => null, SaveConfig));
			return options.ToArray();
		}

		public void SaveConfig(MenuUtils menuUtils, string header, int spacing) {
			List<string> lines = YamlUtils.GetAllConfigLines(configPath);
			int endLine = lines.Count - 1;
			if(!YamlUtils.ChangeYamlObjects(ref lines, 0, ref endLine, 0, new BetterDict<string, YamlObject>{{nameof(menuStyle), menuStyle } })) {
				Console.WriteLine("failed to save menuStyle");
				Console.ReadKey(true);
				return;
			}

			using(StreamWriter writer = new StreamWriter(configPath, false)) {
				lines.ForEach(line => writer.WriteLine(line));
			}
			Console.WriteLine("successfully saved menuStyle");
			Console.ReadKey(true);
		}
	}
}

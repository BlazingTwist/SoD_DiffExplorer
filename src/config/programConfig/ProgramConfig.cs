using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.ObjectFactories;

namespace SoD_DiffExplorer.config.programConfig {
	[PublicAPI]
	public class ProgramConfig : IMenuObject, IOnlineAddressDictConfig {
		[YamlIgnore] private string configPath;

		public BetterDict<ConsoleKey, MenuControl> menuControlMapping;
		public BetterDict<string, string> onlineAddressDict;
		public MenuStyleConfig menuStyle;

		public static ProgramConfig Init(string path) {
			try {
				IDeserializer deserializer = new DeserializerBuilder()
						.WithNodeDeserializer(
								inner => new MenuObjectDeserializer(inner, new DefaultObjectFactory()),
								s => s.InsteadOf<ObjectNodeDeserializer>()
						)
						.Build();
				using StreamReader reader = File.OpenText(path);
				var core = deserializer.Deserialize<ProgramConfig>(reader);
				core.configPath = path;
				return core;
			} catch (Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e);
				Console.ReadKey(true);
				return null;
			}
		}

		BetterDict<string, string> IOnlineAddressDictConfig.GetOnlineAddressDict() {
			return onlineAddressDict;
		}

		string IMenuObject.GetInfoString() {
			return menuStyle.ToString();
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			List<IMenuProperty> options = new List<IMenuProperty>();
			options.AddRange(menuStyle.GetOptions());
			options.Add(new MenuOption("Config", MenuUtils.GetSaveString, () => null, SaveConfig));
			return options.ToArray();
		}

		private void SaveConfig(MenuUtils menuUtils, string header, int spacing) {
			List<string> lines = YamlUtils.GetAllConfigLines(configPath);
			int endLine = lines.Count - 1;
			if (!YamlUtils.ChangeYamlObjects(ref lines, 0, ref endLine, 0, new BetterDict<string, YamlObject> { { nameof(menuStyle), menuStyle } })) {
				Console.WriteLine("failed to save menuStyle");
				Console.ReadKey(true);
				return;
			}

			using (var writer = new StreamWriter(configPath, false)) {
				foreach (string line in lines) {
					writer.WriteLine(line);
				}
			}

			Console.WriteLine("successfully saved menuStyle");
			Console.ReadKey(true);
		}
	}
}
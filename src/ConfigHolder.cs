using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.subPrograms;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer {
	[PublicAPI]
	public class ConfigHolder {
		[YamlIgnore] public ProgramConfig programConfig;
		[YamlIgnore] public MenuUtils menuUtils;

		public string coreConfigPath;
		public List<ISubProgram> subPrograms;

		public bool Initialize() {
			programConfig = ProgramConfig.Init(coreConfigPath);
			if (programConfig == null) {
				return false;
			}

			Console.BackgroundColor = programConfig.menuStyle.normalBackgroundColor.GetValue();
			Console.ForegroundColor = programConfig.menuStyle.normalTextColor.GetValue();

			menuUtils = new MenuUtils(programConfig.menuControlMapping, programConfig.menuStyle);

			foreach (ISubProgram subProgram in subPrograms) {
				Console.WriteLine("Initializing " + subProgram.GetProgramName() + "...");
				if (!subProgram.Init(menuUtils, programConfig)) {
					return false;
				}
			}

			return true;
		}

		public void OpenMainMenu() {
			List<string> options = new List<string> { "Adjust Menu Style\n" };
			options.AddRange(subPrograms.Select(program => "Open " + program.GetProgramName()));
			string[] optionsArray = options.ToArray();

			const string header = "SoD_Analyzer:";
			const string backText = "quit";
			const int spacing = 1;

			int selection = 0;

			while (true) {
				selection = menuUtils.OpenSelectionMenu(optionsArray, backText, header, selection, spacing);

				if (selection == 0) {
					menuUtils.OpenObjectMenu("Program", "CoreConfig", programConfig, 0, spacing + MenuUtils.spacerWidth);
				} else if (selection >= optionsArray.Length) {
					return;
				} else {
					subPrograms[selection - 1].OpenMainMenu(spacing + MenuUtils.spacerWidth);
				}
			}
		}
	}
}
using System;
using System.Linq;
using YamlDotNet.Serialization;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer._revamp._subPrograms;
using System.Collections.Generic;
using SoD_DiffExplorer._revamp._config._programConfig;

namespace SoD_DiffExplorer
{
	class ConfigHolder
	{
		[YamlIgnore]
		public ProgramConfig programConfig;

		[YamlIgnore]
		public MenuUtils menuUtils;

		public string coreConfigPath = null;
		public List<ISubProgram> subPrograms = null;

		public bool Initialize() {
			programConfig = ProgramConfig.Init(coreConfigPath);
			if(programConfig == null) {
				return false;
			}
			Console.BackgroundColor = programConfig.menuStyle.normalBackgroundColor.GetValue();
			Console.ForegroundColor = programConfig.menuStyle.normalTextColor.GetValue();

			MenuUtils menuUtils = new MenuUtils(programConfig.menuControlMapping, programConfig.menuStyle);
			this.menuUtils = menuUtils;

			foreach(ISubProgram subProgram in subPrograms) {
				Console.WriteLine("Initializing " + subProgram.GetProgramName() + "...");
				if(!subProgram.Init(menuUtils, programConfig)) {
					return false;
				}
			}
			return true;
		}

		public void OpenMainMenu() {
			List<string> options = new List<string>();
			options.Add("Adjust Menu Style\n");
			options.AddRange(subPrograms.Select(program => "Open " + program.GetProgramName()));
			string[] optionsArray = options.ToArray();

			string header = "SoD_Analyzer:";
			string backText = "quit";
			int spacing = 1;

			int selection = 0;

			while(true) {
				selection = menuUtils.OpenSelectionMenu(optionsArray, backText, header, selection, spacing);

				if(selection == 0) {
					menuUtils.OpenObjectMenu("Program", "CoreConfig", programConfig, 0, spacing + MenuUtils.spacerWidth);
				}else if(selection >= optionsArray.Length) {
					return;
				} else {
					subPrograms[selection - 1].OpenMainMenu(spacing + MenuUtils.spacerWidth);
				}
			}
		}
	}
}

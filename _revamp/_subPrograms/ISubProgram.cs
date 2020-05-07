using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer.menu;

namespace SoD_DiffExplorer._revamp._subPrograms
{
	interface ISubProgram
	{
		public string GetProgramName();

		public bool Init(MenuUtils menuUtils, ProgramConfig programConfig);

		public void OpenMainMenu(int spacing);
	}
}

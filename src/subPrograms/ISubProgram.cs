using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.menu;

namespace SoD_DiffExplorer.subPrograms {
	public interface ISubProgram {
		public string GetProgramName();
		public bool Init(MenuUtils menuUtils, ProgramConfig programConfig);
		public void OpenMainMenu(int spacing);
	}
}
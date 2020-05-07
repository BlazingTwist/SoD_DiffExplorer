using SoD_DiffExplorer.csutils;
using System.Collections.Generic;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class OnlineInterpreterFilter : YamlObject
	{
		public string path = null;
		public EOnlineInterpreterPathType pathType = 0;
		public bool doDisplay = false;
		public string outputName = null;

		public void ToggleDoDisplay() {
			doDisplay = !doDisplay;
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(doDisplay), doDisplay.ToString()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startline, ref int endLine, int currentTablDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startline, ref endLine, currentTablDepth, GetValueChangeDict());
		}
	}
}

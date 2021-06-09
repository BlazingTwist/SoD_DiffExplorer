using System.Collections.Generic;
using JetBrains.Annotations;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	[PublicAPI]
	public class OnlineInterpreterFilter : YamlObject {
		public string path;
		public EOnlineInterpreterPathType pathType = 0;
		public bool doDisplay;
		public string outputName;

		public void ToggleDoDisplay() {
			doDisplay = !doDisplay;
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
					{ nameof(doDisplay), doDisplay.ToString() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}
	}
}
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	[PublicAPI]
	public class DownloadInterpreterFilter {
		public string basePath;
		public string valuePath;
		public EOnlineInterpreterPathType pathType = 0;
		public string fileNameModifierRegex;
		public string fileNameModifierReplacement;
		public bool optional;
		public List<string> valueRegexFilters;
	}
}
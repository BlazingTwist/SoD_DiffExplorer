using System.Collections.Generic;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class DownloadInterpreterFilter
	{
		public string basePath = null;
		public string valuePath = null;
		public EOnlineInterpreterPathType pathType = 0;
		public string fileNameModifierRegex = null;
		public string fileNameModifierReplacement = null;
		public bool optional = false;
		public List<string> valueRegexFilters = null;
	}
}

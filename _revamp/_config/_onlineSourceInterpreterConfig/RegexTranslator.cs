using System.Text.RegularExpressions;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class RegexTranslator
	{
		public string regex = null;
		public string replacement = null;

		public string Apply(string value) {
			return new Regex(regex).Replace(value, replacement);
		}
	}
}

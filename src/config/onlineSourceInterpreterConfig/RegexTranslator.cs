using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	[PublicAPI]
	public class RegexTranslator {
		public string regex;
		public string replacement;

		public string Apply(string value) {
			return new Regex(regex).Replace(value, replacement);
		}
	}
}
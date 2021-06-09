using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoD_DiffExplorer.utils {
	public static class CustomRegex {
		public static bool AllMatching(string value, List<string> regexes) {
			return regexes == null || regexes.All(regex => IsMatching(value, regex, Regex.IsMatch));
		}

		private static bool IsMatching(string value, string regex, Func<string, string, bool> isMatch) {
			if (regex == null) {
				return true;
			}

			if (regex.StartsWith("!")) {
				return !isMatch(value, regex.Remove(0, 1));
			}

			if (regex.StartsWith(@"\!")) {
				regex = regex.Remove(0, 1);
			}

			return isMatch(value, regex);
		}
	}
}
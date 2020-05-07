using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoD_DiffExplorer.csutils
{
	class CustomRegex
	{
		public static bool AllMatching(string value, List<string> regexes) {
			return !regexes.Any(regex => !IsMatching(value, regex, Regex.IsMatch));
		}

		public static bool IsMatching(string value, string regex) {
			return IsMatching(value, regex, Regex.IsMatch);
		}

		public static bool IsMatching(string value, string regex, Func<string, string, bool> isMatch) {
			if(regex.StartsWith("!")) {
				return !isMatch(value, regex.Remove(0, 1));
			} else {
				if(regex.StartsWith(@"\!")) {
					return isMatch(value, regex.Remove(0, 1));
				} else {
					return isMatch(value, regex);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SoD_DiffExplorer.csutils {
	internal class SemiNumericStringComparer : IComparer<string> {
		private static bool IsNumeric(string value) {
			return int.TryParse(value, out _);
		}

		public int Compare([AllowNull] string x, [AllowNull] string y) {
			const int xGreaterY = 1;
			const int yGreaterX = -1;

			bool IsNumericX = IsNumeric(x);
			bool IsNumericY = IsNumeric(y);

			if (IsNumericX && IsNumericY) {
				return Convert.ToInt32(x) - Convert.ToInt32(y);
			}

			if (IsNumericX) {
				return xGreaterY;
			}

			return IsNumericY
					? yGreaterX
					: string.Compare(x, y, true, CultureInfo.InvariantCulture);
		}
	}
}
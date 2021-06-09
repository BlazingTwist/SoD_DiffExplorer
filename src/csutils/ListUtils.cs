using System;
using System.Collections.Generic;
using System.Linq;

namespace SoD_DiffExplorer.csutils {
	internal static class ListUtils {
		public static List<T> CloneList<T>(this IEnumerable<T> target) where T : ICloneable {
			return target.Select(item => (T) item.Clone()).ToList();
		}
	}
}
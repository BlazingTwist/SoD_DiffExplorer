using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SoD_DiffExplorer.csutils
{
	static class ListUtils
	{
		public static List<T> CloneList<T>(this List<T> target) where T : ICloneable {
			return target.Select(item => (T)item.Clone()).ToList();
		}
	}
}

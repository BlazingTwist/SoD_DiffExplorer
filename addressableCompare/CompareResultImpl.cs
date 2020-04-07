using System;
using System.Collections.Generic;
using System.Text;

namespace SoD_DiffExplorer.addressableCompare
{
	class CompareResultImpl
	{
		public List<string> removedValues = new List<string>();
		public List<string> addedValues = new List<string>();

		public CompareResultImpl(List<string> from, List<string> to) {
			for(int i = (to.Count - 1); i >= 0; i--){
				if(!from.Contains(to[i])) {
					addedValues.Add(to[i]);
					to.RemoveAt(i);
				}
			}
			for(int i = (from.Count - 1); i >= 0; i--) {
				if(!to.Contains(from[i])) {
					removedValues.Add(from[i]);
					from.RemoveAt(i);
				}
			}
		}
	}
}

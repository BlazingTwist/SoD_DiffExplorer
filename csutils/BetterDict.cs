using System;
using System.Collections.Generic;
using System.Text;

namespace SoD_DiffExplorer.csutils
{
	public class BetterDict<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public BetterDict() {

		}

		public BetterDict(IDictionary<TKey, TValue> dict) : base(dict) {
			//nothing to do here.
		}

		public new TValue this[TKey key] {
			get {
				TValue value;
				return base.TryGetValue(key, out value) ? value : default(TValue);
			}
			set {
				base[key] = value;
			}
		}
	}
}

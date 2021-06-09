using System.Collections.Generic;
using JetBrains.Annotations;

namespace SoD_DiffExplorer.csutils {
	[PublicAPI]
	public class BetterDict<TKey, TValue> : Dictionary<TKey, TValue> {
		public BetterDict() { }

		public BetterDict(IDictionary<TKey, TValue> dict) : base(dict) { }

		public new TValue this[TKey key] {
			get => TryGetValue(key, out TValue value) ? value : default;
			set => base[key] = value;
		}
	}
}
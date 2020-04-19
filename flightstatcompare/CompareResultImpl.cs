using SoD_DiffExplorer.commonconfig;
using SoD_DiffExplorer.csutils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoD_DiffExplorer.flightstatcompare
{
	class CompareResultImpl
	{
		public List<string> dragonOrder = new List<string>();
		public List<ResultFilter> resultFilter;
		public BetterDict<int, string> flightTypesDict = null;
		public Dictionary<string, List<Dictionary<string, string>>> sameValues;
		public Dictionary<string, List<Dictionary<string, string>>> removedValues;
		public Dictionary<string, List<Dictionary<string, string>>> addedValues;
		public Dictionary<string, List<Dictionary<string, string>>> changedValuesFrom;
		public Dictionary<string, List<Dictionary<string, string>>> changedValuesTo;

		public CompareResultImpl(Dictionary<string, List<Dictionary<string, string>>> from, Dictionary<string, List<Dictionary<string, string>>> to, List<ResultFilter> displayFilters, BetterDict<int, string> flightTypesDict) {
			this.flightTypesDict = flightTypesDict;

			//build statorders
			foreach(string key in from.Keys) {
				if(!dragonOrder.Contains(key)) {
					dragonOrder.Add(key);
				}
			}
			foreach(string key in to.Keys) {
				if(!dragonOrder.Contains(key)) {
					dragonOrder.Add(key);
				}
			}
			dragonOrder.Sort();
			
			resultFilter = displayFilters.Where(filter => filter.isAllowed).ToList();

			//gather removed values
			removedValues = from.Where(kvp => !to.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//remove removals from from
			foreach(string key in removedValues.Keys) {
				from.Remove(key);
			}

			//gather added values
			addedValues = to.Where(kvp => !from.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//remove additions from to
			foreach(string key in addedValues.Keys) {
				to.Remove(key);
			}

			//gather same values
			sameValues = from.Where(kvp => AllValuesSame(kvp.Value, to[kvp.Key])).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//remove same values from from and to
			foreach(string key in sameValues.Keys) {
				from.Remove(key);
				to.Remove(key);
			}

			changedValuesFrom = from;
			changedValuesTo = to;
		}

		private bool AllValuesSame(List<Dictionary<string, string>> from, List<Dictionary<string, string>> to) {
			if(from.Count != to.Count) {
				return false;
			}
			for(int i = 0; i < from.Count; i++) {
				if(from[i].Any(kvp => !to[i].ContainsKey(kvp.Key) || to[i][kvp.Key] != kvp.Value)) {
					return false;
				}
				if(to[i].Any(kvp => !from[i].ContainsKey(kvp.Key) || from[i][kvp.Key] != kvp.Value)) {
					return false;
				}
			}
			return true;
		}

		public string FormatComparison(Dictionary<string, List<Dictionary<string, string>>> data) {
			StringBuilder result = new StringBuilder();

			foreach(string dragon in dragonOrder) {
				if(!data.ContainsKey(dragon)) {
					continue;
				}
				result.Append("\n\t").Append(dragon);
				List<Dictionary<string, string>> statList = data[dragon];
				for(int i = 0; i < statList.Count; i++) {
					if(i > 0) {
						result.Append("\n\t");
					}
					result.Append("\t").Append(flightTypesDict[i % flightTypesDict.Count]);
					AppendAllStats(ref result, statList[i]);
				}
			}

			return result.ToString();
		}

		public string FormatComparisonChange(Dictionary<string, List<Dictionary<string, string>>> from, Dictionary<string, List<Dictionary<string, string>>> to) {
			StringBuilder result = new StringBuilder();

			foreach(string dragon in dragonOrder) {
				if(!from.ContainsKey(dragon) || !to.ContainsKey(dragon)) {
					continue;
				}

				int loopLimit = to[dragon].Count > from[dragon].Count ? to[dragon].Count : from[dragon].Count;
				for(int i = 0; i < loopLimit; i++) {
					if(i < to[dragon].Count) {
						result.Append("\nto\t").Append(dragon).Append("\t").Append(flightTypesDict[i % flightTypesDict.Count]);
						AppendAllStats(ref result, to[dragon][i]);
					}

					if(i < from[dragon].Count) {
						result.Append("\nfrom\t");
						if(i >= to[dragon].Count) {
							//no to data available
							result.Append(dragon).Append("\t").Append(flightTypesDict[i % flightTypesDict.Count]);
							AppendAllStats(ref result, from[dragon][i]);
						} else {
							result.Append("\t");
							Dictionary<string, string> statsFrom = from[dragon][i];
							Dictionary<string, string> statsTo = to[dragon][i];

							foreach(string stat in resultFilter.Select(filter => filter.path)) {
								result.Append("\t");
								if(statsFrom.ContainsKey(stat)) {
									if(statsTo.ContainsKey(stat)) {
										if(statsFrom[stat] != statsTo[stat]) {
											result.Append(statsFrom[stat].Trim());
										}
									} else {
										result.Append(statsFrom[stat].Trim());
									}
								} else {
									if(statsTo.ContainsKey(stat)) {
										result.Append("null");
									}
								}
							}
						}
					}
				}

				//result.Append("\nto\t").Append(dragon);
				//List<Dictionary<string, string>> statListTo = to[dragon];
				//for(int i = 0; i < statListTo.Count; i++) {
				//	if(i > 0) {
				//		result.Append("\n\t");
				//	}
				//	result.Append("\t").Append(flightTypesDict[i % flightTypesDict.Count]);
				//	AppendAllStats(ref result, statListTo[i]);
				//}

				//result.Append("\nfrom\t");
				//List<Dictionary<string, string>> statListFrom = from[dragon];
				//for(int i = 0; i < statListFrom.Count; i++) {
				//	if(i > 0) {
				//		result.Append("\n\t");
				//	}
				//	result.Append("\t").Append(flightTypesDict[i % flightTypesDict.Count]);

				//	//append only statchanges
				//	if(to[dragon].Count <= i) {
				//		AppendAllStats(ref result, statListFrom[i]);
				//	} else {
				//		Dictionary<string, string> statsFrom = statListFrom[i];
				//		Dictionary<string, string> statsTo = statListTo[i];
				//		foreach(string stat in statOrder) {
				//			result.Append("\t");
				//			if(statsFrom.ContainsKey(stat)) {
				//				if(statsTo.ContainsKey(stat)) {
				//					if(statsFrom[stat] != statsTo[stat]) {
				//						result.Append(statsFrom[stat].Trim());
				//					}
				//				} else {
				//					result.Append(statsFrom[stat].Trim());
				//				}
				//			} else {
				//				if(statsTo.ContainsKey(stat)) {
				//					result.Append("null");
				//				}
				//			}
				//		}
				//	}
				//}
			}

			return result.ToString();
		}

		private void AppendAllStats(ref StringBuilder builder, Dictionary<string, string> dict) {
			foreach(string stat in resultFilter.Select(filter => filter.path)) {
				builder.Append("\t");
				if(dict.ContainsKey(stat)) {
					builder.Append(dict[stat].Trim());
				} else {
					builder.Append("null");
				}
			}
		}
	}
}

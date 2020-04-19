using SoD_DiffExplorer.commonconfig;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoD_DiffExplorer.fireballcompare
{
	class CompareResultImpl
	{
		public List<string> dragonOrder = new List<string>();
		public List<ResultFilter> resultFilter;
		public Dictionary<string, Dictionary<string, string>> sameValues;
		public Dictionary<string, Dictionary<string, string>> removedValues;
		public Dictionary<string, Dictionary<string, string>> addedValues;
		public Dictionary<string, Dictionary<string, string>> changedValuesFrom;
		public Dictionary<string, Dictionary<string, string>> changedValuesTo;

		public CompareResultImpl(Dictionary<string, Dictionary<string, string>> from, Dictionary<string, Dictionary<string, string>> to, List<ResultFilter> displayFilters) {
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

		private bool AllValuesSame(Dictionary<string, string> from, Dictionary<string, string> to) {
			if(from.Any(kvp => !to.ContainsKey(kvp.Key) || to[kvp.Key] != kvp.Value)) {
				return false;
			}
			if(to.Any(kvp => !from.ContainsKey(kvp.Key) || from[kvp.Key] != kvp.Value)) {
				return false;
			}
			return true;
		}

		public string FormatComparison(Dictionary<string, Dictionary<string, string>> data) {
			StringBuilder result = new StringBuilder();

			foreach(string dragon in dragonOrder) {
				if(!data.ContainsKey(dragon)) {
					continue;
				}
				result.Append("\n\t").Append(dragon);
				Dictionary<string, string> dragonDict = data[dragon];
				foreach(string stat in resultFilter.Select(filter => filter.path)) {
					result.Append("\t");
					if(dragonDict.ContainsKey(stat)) {
						result.Append(dragonDict[stat].Trim());
					} else {
						result.Append("null");
					}
				}
			}

			return result.ToString();
		}

		public string FormatComparisonChange(Dictionary<string, Dictionary<string, string>> from, Dictionary<string, Dictionary<string, string>> to) {
			StringBuilder result = new StringBuilder();

			foreach(string dragon in dragonOrder) {
				if(!from.ContainsKey(dragon) || !to.ContainsKey(dragon)) {
					continue;
				}
				result.Append("\nto\t").Append(dragon);

				Dictionary<string, string> statsTo = to[dragon];
				foreach(string stat in resultFilter.Select(filter => filter.path)) {
					result.Append("\t");
					if(statsTo.ContainsKey(stat)) {
						result.Append(statsTo[stat].Trim());
					} else {
						result.Append("null");
					}
				}

				result.Append("\nfrom\t");
				Dictionary<string, string> statsFrom = from[dragon];
				foreach(string stat in resultFilter.Select(filter => filter.path)) {
					result.Append("\t");
					if(statsFrom.ContainsKey(stat)) {
						if(statsTo.ContainsKey(stat)) {
							if(statsFrom[stat] != statsTo[stat]) {
								result.Append(statsFrom[stat].Trim());
							}
							//else no change, no print
						} else {
							result.Append(statsFrom[stat].Trim());
						}
					} else {
						if(statsTo.ContainsKey(stat)) {
							result.Append("null");
						}
						//else neither had any value -> no change, no print
					}
				}
			}

			return result.ToString();
		}
	}
}

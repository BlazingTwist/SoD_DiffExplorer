using SoD_DiffExplorer.commonconfig;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoD_DiffExplorer.timedmissioncompare
{
	class CompareResultImpl
	{
		public List<int> missionIDOrder = new List<int>();
		public List<ResultFilter> resultFilter;
		public Dictionary<int, Dictionary<string, List<string>>> sameValues;
		public Dictionary<int, Dictionary<string, List<string>>> removedValues;
		public Dictionary<int, Dictionary<string, List<string>>> addedValues;
		public Dictionary<int, Dictionary<string, List<string>>> changedValuesFrom;
		public Dictionary<int, Dictionary<string, List<string>>> changedValuesTo;

		public CompareResultImpl(Dictionary<int, Dictionary<string, List<string>>> from, Dictionary<int, Dictionary<string, List<string>>> to, List<ResultFilter> displayFilters) {
			//build statOrders
			foreach(int key in from.Keys) {
				if(!missionIDOrder.Contains(key)) {
					missionIDOrder.Add(key);
				}
			}
			foreach(int key in to.Keys) {
				if(!missionIDOrder.Contains(key)) {
					missionIDOrder.Add(key);
				}
			}
			missionIDOrder.Sort();

			resultFilter = displayFilters.Where(filter => filter.isAllowed).ToList();

			//gather removed values
			removedValues = from.Where(kvp => !to.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//remove removals from from
			foreach(int key in removedValues.Keys) {
				from.Remove(key);
			}

			//gather added values
			addedValues = to.Where(kvp => !from.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//remove additions from to
			foreach(int key in addedValues.Keys) {
				to.Remove(key);
			}

			//gather same values
			sameValues = from.Where(kvp => AllValuesSame(kvp.Value, to[kvp.Key])).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			//removed same values from from and to
			foreach(int key in sameValues.Keys) {
				from.Remove(key);
				to.Remove(key);
			}

			changedValuesFrom = from;
			changedValuesTo = to;
		}

		private bool AllValuesSame(Dictionary<string, List<string>> from, Dictionary<string, List<string>> to) {
			if(from.Any(kvp => !to.ContainsKey(kvp.Key))) {
				return false;
			}
			if(to.Any(kvp => !from.ContainsKey(kvp.Key))) {
				return false;
			}
			if(from.Any(kvp => !AllValuesSame(kvp.Value, to[kvp.Key]))) {
				return false;
			}
			return true;
		}

		private bool AllValuesSame(List<string> from, List<string> to) {
			if(from.Count != to.Count) {
				return false;
			}
			for(int i = 0; i < from.Count; i++) {
				if(from[i] != to[i]) {
					return false;
				}
			}
			return true;
		}

		public string FormatComparison(Dictionary<int, Dictionary<string, List<string>>> data) {
			StringBuilder result = new StringBuilder();

			foreach(int missionID in missionIDOrder) {
				if(!data.ContainsKey(missionID)) {
					continue;
				}
				Dictionary<string, List<string>> mission = data[missionID];

				int maxEntryNum = FindMaxEntryHeight(mission);
				result.Append("\n\t").Append(missionID);
				for(int i = 0; i < maxEntryNum; i++) {
					if(i > 0) {
						result.Append("\n\t");
					}
					AppendPropertiesAtIndex(ref result, mission, i);
				}
			}

			return result.ToString();
		}

		public string FormatComparisonChange(Dictionary<int, Dictionary<string, List<string>>> from, Dictionary<int, Dictionary<string, List<string>>> to) {
			StringBuilder result = new StringBuilder();

			foreach(int missionID in missionIDOrder) {
				if(!from.ContainsKey(missionID) || !to.ContainsKey(missionID)) {
					continue;
				}

				Dictionary<string, List<string>> missionTo = to[missionID];
				int maxEntryNumTo = FindMaxEntryHeight(missionTo);
				result.Append("\nto\t").Append(missionID);
				for(int i = 0; i < maxEntryNumTo; i++) {
					if(i > 0) {
						result.Append("\n\t");
					}
					AppendPropertiesAtIndex(ref result, missionTo, i);
				}

				result.Append("\nfrom\t");
				Dictionary<string, List<string>> missionFrom = from[missionID];
				int maxEntryNumFrom = FindMaxEntryHeight(missionFrom);
				for(int i = 0; i < maxEntryNumFrom; i++) {
					if(i > 0) {
						result.Append("\n\t");
					}
					foreach(ResultFilter filter in resultFilter) {
						string propertyKey = filter.path;
						result.Append("\t");
						if(!missionFrom.ContainsKey(propertyKey)) {
							if(missionTo.ContainsKey(propertyKey) && i == 0) {
								result.Append("[prop missing]");
							}
							//else no difference or ignoring multiListing
						} else {
							List<string> propValues = missionFrom[propertyKey];
							if(i < propValues.Count) {
								if(missionTo.ContainsKey(propertyKey) && i < missionTo[propertyKey].Count) {
									if(missionTo[propertyKey][i] != propValues[i]) {
										result.Append(StringToString(propValues[i]));
									}
									//else no difference
								} else {
									result.Append(StringToString(propValues[i]));
								}
							} else {
								if(missionTo.ContainsKey(propertyKey) && i < missionTo[propertyKey].Count) {
									result.Append("[entry missing]");
								}
								//else no difference
							}
						}
					}
				}
			}

			return result.ToString();
		}

		//sounds stupid, but outputs "null" for null values
		private string StringToString(string value) {
			if(value == null) {
				return "null";
			}
			return value;
		}

		private void AppendPropertiesAtIndex(ref StringBuilder builder, Dictionary<string, List<string>> data, int index) {
			foreach(ResultFilter filter in resultFilter) {
				string propertyKey = filter.path;
				builder.Append("\t");
				if(!data.ContainsKey(propertyKey)) {
					continue;
				}
				List<string> dataList = data[propertyKey];
				if(index >= dataList.Count) {
					continue;
				}
				builder.Append(StringToString(dataList[index]));
			}
		}

		private int FindMaxEntryHeight(Dictionary<string, List<string>> data) {
			int max = 0;
			foreach(List<string> list in data.Values) {
				if(list.Count > max) {
					max = list.Count;
				}
			}
			return max;
		}
	}
}

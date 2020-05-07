using SoD_DiffExplorer.csutils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoD_DiffExplorer._revamp._config._resultConfig
{
	class ConfigMapResult
	{
		public Dictionary<string, Dictionary<string, List<string>>> valuesFrom = null;
		public Dictionary<string, Dictionary<string, List<string>>> valuesTo = null;

		private List<string> primaryKeyOrder = null;
		private List<string> secondaryKeyOrder = null;
		private Dictionary<string, Dictionary<string, List<string>>> removedValues = null;
		private Dictionary<string, Dictionary<string, List<string>>> addedValues = null;
		private Dictionary<string, Dictionary<string, List<string>>> sameValues = null;
		private Dictionary<string, Dictionary<string, List<string>>> changedValuesFrom = null;
		private Dictionary<string, Dictionary<string, List<string>>> changedValuesTo = null;

		public void LoadValuesFrom(Dictionary<string, Dictionary<String, List<string>>> values) {
			valuesFrom = values;
		}

		public void LoadValuesTo(Dictionary<string, Dictionary<String, List<string>>> values) {
			valuesTo = values;
		}

		public string BuildResult(List<string> secondaryKeyOrder, ResultFilter resultFilter) {
			primaryKeyOrder = BuildPrimaryKeyOrder();
			this.secondaryKeyOrder = secondaryKeyOrder;
			removedValues = BuildRemovedValues();
			addedValues = BuildAddedValues();
			sameValues = BuildSameValues();
			changedValuesFrom = valuesFrom;
			valuesFrom = null;
			changedValuesTo = valuesTo;
			valuesTo = null;

			StringBuilder resultBuilder = new StringBuilder();
			resultBuilder.Append("\t").Append(string.Join('\t', secondaryKeyOrder));
			secondaryKeyOrder.RemoveAt(0); //remove mapConfigBy.OutputName, because handled by primaryKeyOrder
			if(resultFilter.displayAdditions.GetValue()) {
				resultBuilder.Append("\nnew").Append(BuildCompareText(addedValues));
			}
			if(resultFilter.displayDifferences.GetValue()) {
				resultBuilder.Append("\nchanged").Append(BuildCompareText(changedValuesTo, changedValuesFrom));
			}
			if(resultFilter.displayRemovals.GetValue()) {
				resultBuilder.Append("\nremoved").Append(BuildCompareText(removedValues));
			}
			if(resultFilter.displayCommons.GetValue()) {
				resultBuilder.Append("\nunchanged").Append(BuildCompareText(sameValues));
			}

			return resultBuilder.ToString();
		}

		private List<string> BuildPrimaryKeyOrder() {
			List<string> result = new List<string>();

			foreach(string key in valuesFrom.Keys) {
				if(!result.Contains(key)) {
					result.Add(key);
				}
			}
			foreach(string key in valuesTo.Keys) {
				if(!result.Contains(key)) {
					result.Add(key);
				}
			}

			result.Sort(new SemiNumericStringComparer());
			return result;
		}

		private Dictionary<string, Dictionary<string, List<string>>> BuildRemovedValues() {
			Dictionary<string, Dictionary<string, List<string>>> result = valuesFrom.Where(kvp => !valuesTo.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			foreach(string key in result.Keys) {
				valuesFrom.Remove(key);
			}

			return result;
		}

		private Dictionary<string, Dictionary<string, List<string>>> BuildAddedValues() {
			Dictionary<string, Dictionary<string, List<string>>> result = valuesTo.Where(kvp => !valuesFrom.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			foreach(string key in result.Keys) {
				valuesTo.Remove(key);
			}

			return result;
		}

		private Dictionary<string, Dictionary<string, List<string>>> BuildSameValues() {
			Dictionary<string, Dictionary<string, List<string>>> result = valuesFrom.Where(kvp => AllValuesSame(kvp.Value, valuesTo[kvp.Key])).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			foreach(string key in result.Keys) {
				valuesFrom.Remove(key);
				valuesTo.Remove(key);
			}

			return result;
		}

		private bool AllValuesSame(Dictionary<string, List<string>> x, Dictionary<string, List<string>> y) {
			if(x.Any(kvp => !y.ContainsKey(kvp.Key) || !AllValuesSame(kvp.Value, y[kvp.Key]))) {
				return false;
			}
			if(y.Any(kvp => !x.ContainsKey(kvp.Key))) {
				return false;
			}
			return true;
		}

		private bool AllValuesSame(List<string> x, List<string> y) {
			if(x.Count != y.Count) {
				return false;
			}
			for(int i = 0; i < x.Count; i++) {
				if(x[i] != y[i]) {
					return false;
				}
			}
			return true;
		}

		private string BuildCompareText(Dictionary<string, Dictionary<string, List<string>>> data) {
			StringBuilder result = new StringBuilder();

			foreach(string primaryKey in primaryKeyOrder) {
				if(!data.ContainsKey(primaryKey)) {
					continue;
				}
				Dictionary<string, List<string>> primaryValue = data[primaryKey];
				int targetCount = GetMaxDataCount(primaryValue);
				result.Append("\n\t").Append(primaryKey);
				for(int i = 0; i < targetCount; i++) {
					if(i != 0) {
						result.Append("\n\t");
					}
					foreach(string secondaryKey in secondaryKeyOrder) {
						result.Append("\t");
						if(!primaryValue.ContainsKey(secondaryKey)) {
							if(i == 0) {
								result.Append("null");
							}
							continue;
						}
						if(i >= primaryValue[secondaryKey].Count) {
							if(i == 0) {
								result.Append("null");
							}
							continue;
						}
						result.Append(NullToString(primaryValue[secondaryKey][i]));
					}
				}
			}

			return result.ToString();
		}

		private int GetMaxDataCount(Dictionary<string, List<string>> data) {
			if(data.Count == 0) {
				return 0;
			}
			return data.Max(kvp => kvp.Value.Count);
		}

		private string BuildCompareText(Dictionary<string, Dictionary<string, List<string>>> dataX, Dictionary<string, Dictionary<string, List<string>>> dataY) {
			StringBuilder result = new StringBuilder();

			foreach(string primaryKey in primaryKeyOrder) {
				if(!dataX.ContainsKey(primaryKey) || !dataY.ContainsKey(primaryKey)) {
					continue;
				}
				Dictionary<string, List<string>> primaryValueX = dataX[primaryKey];
				Dictionary<string, List<string>> primaryValueY = dataY[primaryKey];
				int targetCount = GetMaxDataCount(primaryValueX, primaryValueY);
				for(int i = 0; i < targetCount; i++) {
					result.Append("\nnow\t").Append(primaryKey);
					foreach(string secondaryKey in secondaryKeyOrder) {
						result.Append("\t");
						if(!primaryValueX.ContainsKey(secondaryKey)) {
							if(primaryValueY.ContainsKey(secondaryKey) && primaryValueY[secondaryKey].Count > i && primaryValueY[secondaryKey][i] != null) {
								result.Append("null");
							}
							continue;
						}
						if(i >= primaryValueX[secondaryKey].Count) {
							if(primaryValueY.ContainsKey(secondaryKey) && primaryValueY[secondaryKey].Count > i && primaryValueY[secondaryKey][i] != null) {
								result.Append("null");
							}
							continue;
						}
						result.Append(NullToString(primaryValueX[secondaryKey][i]));
					}
					result.Append("\nwas\t");
					foreach(string secondaryKey in secondaryKeyOrder) {
						result.Append("\t");
						if(!primaryValueY.ContainsKey(secondaryKey)) {
							if(primaryValueX.ContainsKey(secondaryKey)
								&& i < primaryValueX[secondaryKey].Count
								&& primaryValueX[secondaryKey][i] != null) {
								result.Append("null");
							}
							continue;
						}
						if(i >= primaryValueY[secondaryKey].Count) {
							if(primaryValueX.ContainsKey(secondaryKey)
								&& i < primaryValueX[secondaryKey].Count
								&& primaryValueX[secondaryKey][i] != null) {
								result.Append("null");
							}
							continue;
						}
						if(primaryValueY[secondaryKey][i] == null) {
							if(primaryValueX.ContainsKey(secondaryKey)
								&& i < primaryValueX[secondaryKey].Count
								&& primaryValueX[secondaryKey][i] != null) {
								result.Append("null");
							}
							continue;
						}
						if(!primaryValueX.ContainsKey(secondaryKey)
							|| i >= primaryValueX[secondaryKey].Count
							|| primaryValueY[secondaryKey][i] != primaryValueX[secondaryKey][i]) {
							result.Append(primaryValueY[secondaryKey][i]);
						}
					}
					result.Append("\n");
				}
			}
			return result.ToString();
		}

		private int GetMaxDataCount(Dictionary<string, List<string>> dataX, Dictionary<string, List<string>> dataY) {
			int maxX = GetMaxDataCount(dataX);
			int maxY = GetMaxDataCount(dataY);
			return maxX > maxY ? maxX : maxY;
		}

		private string NullToString(string value) {
			return value == null ? "null" : value;
		}
	}
}

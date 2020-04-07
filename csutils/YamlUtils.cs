using System;
using System.Collections.Generic;
using System.Linq;

namespace SoD_DiffExplorer.csutils
{
	class YamlUtils
	{
		public static bool ChangeSimpleValues(ref List<string> lines, BetterDict<string, string> changeDict) {
			foreach(KeyValuePair<string, string> pair in changeDict) {
				if(!ChangeSimpleValue(ref lines, pair.Key, pair.Value)) {
					Console.WriteLine("unable to find config key: " + pair.Key);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Updates a simple yaml field (e.g. "someField: false")
		/// </summary>
		/// <param name="lines">
		/// list of lines representing the yaml file
		/// </param>
		/// <param name="yamlPath">
		/// string representing the location of the value that is to be replaced (e.g. fileDownloaderConfig.downloadURL.baseURL)
		/// </param>
		/// <param name="targetValue">
		///	value to be written to the target path
		/// </param>
		/// <returns>
		/// whether a field at the given path was found or not
		/// </returns>
		public static bool ChangeSimpleValue(ref List<string> lines, string yamlPath, string targetValue) {
			int fieldIndex = FindFieldIndex(lines, yamlPath);
			if(fieldIndex < 0) {
				return false;
			} else {
				string[] lineSplit = lines[fieldIndex].Split(": ", 2);
				if(lineSplit.Length == 1) {
					lineSplit[0] = lineSplit[0].Replace(":", "");
				}
				lines[fieldIndex] = lineSplit[0] + ": " + targetValue;
				return true;
			}
		}

		public static bool ChangeSimpleListContents(ref List<string> lines, BetterDict<string, List<string>> changeDict) {
			foreach(KeyValuePair<string, List<string>> pair in changeDict) {
				if(!ChangeSimpleListContent(ref lines, pair.Key, pair.Value)) {
					Console.WriteLine("unable to find config key: " + pair.Key);
					return false;
				}
			}
			return true;
		}

		public static bool ChangeSimpleListContent(ref List<string> lines, string yamlPath, List<string> targetValues) {
			int fieldIndex = FindFieldIndex(lines, yamlPath);
			if(fieldIndex < 0) {
				Console.WriteLine("returning false because key not found, key was: " + yamlPath);
				return false;
			}

			bool allValuesInserted = false;
			int targetSpaces = CountStartingSpaces(lines[fieldIndex]);
			for(int i = fieldIndex + 1; i < lines.Count; i++) {
				string line = lines[i];
				if(string.IsNullOrWhiteSpace(line)) {
					continue;
				}
				int startingSpaces = CountStartingSpaces(line);
				string subLine = line.Substring(startingSpaces);
				if(subLine.StartsWith("#")) {
					continue;
				}
				if(startingSpaces != targetSpaces) {
					break;
				}
				if(!subLine.StartsWith("- ")) {
					break;
				}

				if(!allValuesInserted) {
					lines.InsertRange(i, targetValues.Select(item => new string(' ', targetSpaces) + "- " + item));
					allValuesInserted = true;
					i += targetValues.Count;
				}
				lines.RemoveAt(i);
				//hack to preserve forward order as I'm to lazy for iterators right now
				i--;
			}

			//figure out how to do lists...
			//search until less depth
			//replace at every -

			return allValuesInserted;
		}

		private static int FindFieldIndex(List<string> lines, string yamlPath) {
			string[] pathSplit = yamlPath.Split(".");

			int pathDepth = 0;
			string targetSectionName = pathSplit[0];
			for(int searchIndex = 0; searchIndex < lines.Count; searchIndex++) {
				string line = lines[searchIndex];
				if(string.IsNullOrWhiteSpace(line)) {
					continue;
				}
				int startingSpaces = CountStartingSpaces(line);
				int targetSpaces = pathDepth * 2;
				string subLine = line.Substring(startingSpaces);
				if(startingSpaces < targetSpaces && !subLine.StartsWith("#")) {
					Console.WriteLine("returning -1 because of line: " + line);
					return -1;
				}
				if(startingSpaces > targetSpaces) {
					continue;
				}
				if(!subLine.StartsWith(targetSectionName)) {
					continue;
				}
				if(pathDepth < (pathSplit.Length - 1)) {
					//not reached end of path yet
					pathDepth++;
					targetSectionName = pathSplit[pathDepth];
				} else {
					//done
					return searchIndex;
				}
			}

			Console.WriteLine("inable to find field: " + yamlPath + " reached end of file!");
			return -1;
		}

		private static int CountStartingSpaces(string target) {
			for(int i = 0; i < target.Length; i++) {
				if(target[i] != ' ') {
					return i;
				}
			}
			return target.Length;
		}
	}
}

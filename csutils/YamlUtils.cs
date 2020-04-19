using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SoD_DiffExplorer.csutils
{
	class YamlUtils
	{
		public static List<string> GetAllConfigLines() {
			List<string> lines = new List<string>();
			using(StreamReader reader = new StreamReader("config.yaml")) {
				string line;
				while((line = reader.ReadLine()) != null) {
					lines.Add(line);
				}
			}
			return lines;
		}

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
				//hack to preserve forward order as I'm too lazy for iterators right now
				i--;
			}

			//figure out how to do lists...
			//search until less depth
			//replace at every -

			return allValuesInserted;
		}

		//yamlpath being the path to the list containing the object
		public static bool ChangeSimpleObjectListContent(ref List<string> lines, string yamlPath, List<YamlObject> yamlObjects) {
			if(yamlObjects.Count == 0) {
				return true;
			}

			int fieldIndex = FindFieldIndex(lines, yamlPath);
			if(fieldIndex < 0) {
				Console.WriteLine("returning false, because key not found, key was: " + yamlPath);
				return false;
			}

			int printedObject = 0;
			int currentPropertyIndex = 0;
			string[] currentObjectKeys = yamlObjects[0].GetFieldNames();
			string[] currentObjectValues = yamlObjects[0].GetFieldValues();
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
				if(startingSpaces != targetSpaces && startingSpaces != (targetSpaces + 2)) {
					break;
				}
				if(startingSpaces == targetSpaces) {
					if(!subLine.StartsWith("- ")) {
						break;
					}
					if(currentPropertyIndex != 0) {
						//out of sync
						break;
					}
					string keySection = subLine.Substring(2);
					if(!keySection.StartsWith(currentObjectKeys[0])) {
						break;
					}
					string valueSection = keySection.Substring(currentObjectKeys[0].Length);
					if(valueSection.StartsWith(": ") || (valueSection.Length == 1 && valueSection.StartsWith(":"))) {
						lines[i] = new string(' ', targetSpaces) + "- " + currentObjectKeys[0] + ": " + currentObjectValues[0];
						currentPropertyIndex++;
					} else {
						break;
					}
				} else {
					if(currentPropertyIndex == 0) {
						//out of sync
						break;
					}
					if(!subLine.StartsWith(currentObjectKeys[currentPropertyIndex])) {
						break;
					}
					string valueSection = subLine.Substring(currentObjectKeys[currentPropertyIndex].Length);
					if(valueSection.StartsWith(": ") || (valueSection.Length == 1 && valueSection.StartsWith(":"))) {
						lines[i] = new string(' ', startingSpaces) + currentObjectKeys[currentPropertyIndex] + ": " + currentObjectValues[currentPropertyIndex];
						currentPropertyIndex++;
					} else {
						break;
					}
				}

				if(currentPropertyIndex == currentObjectKeys.Length) {
					currentPropertyIndex = 0;
					printedObject++;
					if(printedObject == yamlObjects.Count) {
						//done
						break;
					}
					currentObjectKeys = yamlObjects[printedObject].GetFieldNames();
					currentObjectValues = yamlObjects[printedObject].GetFieldValues();
				}
			}

			return printedObject == yamlObjects.Count;
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

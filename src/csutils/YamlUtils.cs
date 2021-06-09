using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoD_DiffExplorer.csutils {
	public static class YamlUtils {
		public static List<string> GetAllConfigLines() {
			List<string> lines = new List<string>();
			using var reader = new StreamReader("config.yaml");
			string line;
			while ((line = reader.ReadLine()) != null) {
				lines.Add(line);
			}

			return lines;
		}

		public static List<string> GetAllConfigLines(string configPath) {
			List<string> lines = new List<string>();
			using var reader = new StreamReader(configPath);
			string line;
			while ((line = reader.ReadLine()) != null) {
				lines.Add(line);
			}

			return lines;
		}

		private static int GetSegmentEndLineIndex(ref List<string> lines, int currentLine, int tabDepth) {
			int targetStartingSpaces = tabDepth * 2;
			for (int i = currentLine; i < lines.Count; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces < targetStartingSpaces) {
					return i - 1;
				}
			}

			return lines.Count - 1;
		}

		private static int GetListEntryEndLineIndex(ref List<string> lines, int currentLine, int tabDepth) {
			if (tabDepth < 1) {
				throw new InvalidDataException("tabDepth of ListEntry cannot be less than 1!");
			}

			int targetStartingSpaces = tabDepth * 2;
			for (int i = currentLine; i < lines.Count; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces < targetStartingSpaces) {
					//end of list
					return i - 1;
				}

				if (lineStartingSpaces == targetStartingSpaces && line[lineStartingSpaces - 2] == '-') {
					//start of next listEntry
					return i - 1;
				}
			}

			return lines.Count - 1;
		}

		public static bool ChangeYamlObjectLists(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth,
				BetterDict<string, List<YamlObject>> changeDict) {
			int targetStartingSpaces = currentTabDepth * 2;
			for (int i = startLine; i <= endLine; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces != targetStartingSpaces) {
					//shouldn't ever leave the targetSection!
					Console.WriteLine(lineStartingSpaces < targetStartingSpaces
							? "ChangeYamlObjectLists somehow left the target section!"
							: "ChangeYamlObjectLists somehow entered a subsection!");

					return false;
				}

				int segmentEndLine = GetSegmentEndLineIndex(ref lines, i + 1, currentTabDepth + 1);
				string lineKey = GetKeyValKey(line);
				if (!changeDict.ContainsKey(lineKey)) {
					//go to next segment
					i = segmentEndLine;
					continue;
				}

				List<YamlObject> targetList = changeDict[lineKey];
				int initialSegmentEndLine = segmentEndLine;
				if (!ChangeYamlObjectList(ref lines, i + 1, ref segmentEndLine, currentTabDepth + 1, targetList)) {
					Console.WriteLine("failed to save YamlList: " + lineKey);
					return false;
				}

				if (segmentEndLine != initialSegmentEndLine) {
					i = segmentEndLine;
					int delta = segmentEndLine - initialSegmentEndLine;
					endLine += delta;
				} else {
					i = initialSegmentEndLine;
				}
			}

			return true;
		}

		private static bool ChangeYamlObjectList(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth,
				IReadOnlyList<YamlObject> objectList) {
			int currentObjectIndex = 0;
			int targetStartingSpaces = currentTabDepth * 2;
			for (int i = startLine; i <= endLine; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces != targetStartingSpaces) {
					//shouldn't ever leave the targetSection!
					Console.WriteLine(lineStartingSpaces < targetStartingSpaces
							? "ChangeYamlObjectList somehow left the target section!"
							: "ChangeYamlObjectList somehow entered a subsection!");

					return false;
				}

				int entryEndLine = GetListEntryEndLineIndex(ref lines, i + 1, currentTabDepth);
				int initialEntryEndLine = entryEndLine;
				if (!objectList[currentObjectIndex].Save(ref lines, i, ref entryEndLine, currentTabDepth)) {
					Console.WriteLine("failed to save YamlListEntry #" + currentObjectIndex);
					return false;
				}

				currentObjectIndex++;

				if (entryEndLine != initialEntryEndLine) {
					i = entryEndLine;
					int delta = entryEndLine - initialEntryEndLine;
					endLine += delta;
				} else {
					i = initialEntryEndLine;
				}
			}

			return true;
		}

		public static bool ChangeYamlObjects(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth,
				BetterDict<string, YamlObject> changeDict) {
			int targetStartingSpaces = currentTabDepth * 2;
			for (int i = startLine; i <= endLine; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces != targetStartingSpaces) {
					//shouldn't ever leave the targetSection!
					Console.WriteLine(lineStartingSpaces < targetStartingSpaces
							? "ChangeYamlObjects somehow left the target section!"
							: "ChangeYamlObjects somehow entered a subsection!");

					return false;
				}

				int segmentEndLine = GetSegmentEndLineIndex(ref lines, i + 1, currentTabDepth + 1);
				string lineKey = GetKeyValKey(line);
				if (!changeDict.ContainsKey(lineKey)) {
					//go to next segment
					i = segmentEndLine;
					continue;
				}

				YamlObject targetObject = changeDict[lineKey];
				int initialSegmentEndLine = segmentEndLine;
				if (!targetObject.Save(ref lines, i + 1, ref segmentEndLine, currentTabDepth + 1)) {
					Console.WriteLine("Failed to save YamlObject: " + lineKey);
					return false;
				}

				if (segmentEndLine != initialSegmentEndLine) {
					i = segmentEndLine;
					int delta = segmentEndLine - initialSegmentEndLine;
					endLine += delta;
				} else {
					i = initialSegmentEndLine;
				}
			}

			return true;
		}

		public static bool ChangeSimpleValues(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth,
				BetterDict<string, string> changeDict) {
			foreach ((string key, string value) in changeDict) {
				if (!ChangeSimpleValue(ref lines, startLine, ref endLine, currentTabDepth, key, value)) {
					Console.WriteLine("unable to find config key: " + key + " | inserting automatically, formatting might look dumb");
					string spacing = new string(' ', currentTabDepth * 2);
					lines.Insert(startLine, (spacing + key + ": " + value));
					endLine++;
				}
			}

			return true;
		}

		private static bool ChangeSimpleValue(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth, string key, string value) {
			int targetStartingSpaces = currentTabDepth * 2;
			for (int i = startLine; i <= endLine; i++) {
				string line = lines[i];
				if (!IsKeyValLine(line)) {
					continue;
				}

				int lineStartingSpaces = CountStartingSpaces(line);
				if (lineStartingSpaces < targetStartingSpaces) {
					return false;
				}

				if (lineStartingSpaces > targetStartingSpaces) {
					continue;
				}

				string subLine = line.Substring(lineStartingSpaces);
				if (!subLine.StartsWith(key)) {
					continue;
				}

				lines[i] = UpdateLine(line, key, value);
				return true;
			}

			return false;
		}

		private static bool IsKeyValLine(string line) {
			if (string.IsNullOrWhiteSpace(line)) {
				return false;
			}

			return !line.Trim().StartsWith('#');
		}

		public static string UpdateLine(string line, string key, string value) {
			string spacing = GetKeyValIndent(line);
			string comment = GetKeyValComment(line);
			return spacing + key + ": " + value + comment;
		}

		private static string GetKeyValKey(string line) {
			int splitterOccurrences = line.Count(c => c == ':');
			if (splitterOccurrences <= 0) {
				throw new InvalidDataException("line is not a KeyVal line!");
			}

			if (splitterOccurrences == 1) {
				return line.Split(':')[0].Trim();
			}

			var result = new StringBuilder();
			line = line.Trim();
			bool currentlyInString = false;
			for (int i = 0; i < line.Length; i++) {
				if (line[i] == '"' && (i == 0 || line[i - 1] != '\\')) {
					currentlyInString = !currentlyInString;
					result.Append(line[i]);
					continue;
				}

				if (currentlyInString) {
					result.Append(line[i]);
					continue;
				}

				if (line[i] == ':' && (i + 1 == line.Length || line[i + 1] == ' ')) {
					return result.ToString();
				}

				result.Append(line[i]);
			}

			throw new InvalidDataException("line is not a KeyVal line!");
		}

		private static string GetKeyValIndent(string line) {
			int spaces = CountStartingSpaces(line);
			return line.Substring(0, spaces);
		}

		private static string GetKeyValComment(string line) {
			if (!line.Contains('#')) {
				return "";
			}

			var preCommentWhitespaces = new StringBuilder();
			StringBuilder comment = null;

			bool currentlyInString = false;
			bool reachedValueSection = false;
			bool foundComment = false;
			for (int i = 0; i < line.Length; i++) {
				if (line[i] == '"' && (i == 0 || line[i - 1] != '\\')) {
					currentlyInString = !currentlyInString;
					continue;
				}

				if (currentlyInString) {
					continue;
				}

				if (line[i] == ':' && (i + 1 == line.Length || line[i + 1] == ' ')) {
					reachedValueSection = true;
					continue;
				}

				if (!reachedValueSection) {
					continue;
				}

				if (!foundComment && char.IsWhiteSpace(line[i])) {
					preCommentWhitespaces.Append(line[i]);
					continue;
				}

				if (!foundComment && line[i] == '#') {
					foundComment = true;
					comment = new StringBuilder("#");
					continue;
				}

				if (!foundComment) {
					continue;
				}

				comment.Append(line[i]);
			}

			return foundComment ? comment.ToString() : "";
		}

		public static int FindFieldIndex(List<string> lines, string yamlPath) {
			string[] pathSplit = yamlPath.Split(".");

			int pathDepth = 0;
			string targetSectionName = pathSplit[0];
			for (int searchIndex = 0; searchIndex < lines.Count; searchIndex++) {
				string line = lines[searchIndex];
				if (string.IsNullOrWhiteSpace(line)) {
					continue;
				}

				int startingSpaces = CountStartingSpaces(line);
				int targetSpaces = pathDepth * 2;
				string subLine = line.Substring(startingSpaces);
				if (startingSpaces < targetSpaces && !subLine.StartsWith("#")) {
					Console.WriteLine("returning -1 because of line: " + line);
					return -1;
				}

				if (startingSpaces > targetSpaces) {
					continue;
				}

				if (!subLine.StartsWith(targetSectionName)) {
					continue;
				}

				if (pathDepth < (pathSplit.Length - 1)) {
					//not reached end of path yet
					pathDepth++;
					targetSectionName = pathSplit[pathDepth];
				} else {
					//done
					return searchIndex;
				}
			}

			Console.WriteLine("unable to find field: " + yamlPath + " reached end of file!");
			return -1;
		}

		private static int CountStartingSpaces(string target) {
			for (int i = 0; i < target.Length; i++) {
				if (target[i] != ' ') {
					if (target[i] == '-' && (i + 1) < target.Length && target[i + 1] == ' ') {
						return i + 2;
					}

					return i;
				}
			}

			return target.Length;
		}
	}
}
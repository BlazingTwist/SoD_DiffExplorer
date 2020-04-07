﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.menuutils
{
	class MenuUtils
	{
		public static BetterDict<ConsoleKey, MenuControl> menuControlMapping = new BetterDict<ConsoleKey, MenuControl> {
			{ConsoleKey.W, MenuControl.Up},
			{ConsoleKey.UpArrow, MenuControl.Up},
			{ConsoleKey.S, MenuControl.Down},
			{ConsoleKey.DownArrow, MenuControl.Down},
			{ConsoleKey.A, MenuControl.Left},
			{ConsoleKey.LeftArrow, MenuControl.Left},
			{ConsoleKey.D, MenuControl.Right},
			{ConsoleKey.RightArrow, MenuControl.Right},
			{ConsoleKey.Enter, MenuControl.Enter},
			{ConsoleKey.Backspace, MenuControl.Back},
			{ConsoleKey.Escape, MenuControl.Back}
		};

		public static string OpenEnumConfigEditor(string valueName, string previousValue, string[] values, int spacing) {
			StringBuilder header = new StringBuilder();
			header.Append("EnumSelection Controls:\n");
			header.Append("\tEscape to cancel\n");
			header.Append("\tEnter to select\n\n");
			header.Append("Currently modifying " + valueName + " (" + previousValue + ")");

			int selection = MenuUtils.OpenSelectionMenu(values, null, header.ToString(), 0, spacing);

			if(selection >= values.Length) {
				return previousValue;
			}
			return values[selection];
		}

		public static string OpenSimpleConfigEditor(string valueName, string previousValue) {
			Console.Clear();
			Console.WriteLine("TextEditor Controls:");
			Console.WriteLine("\tEscape to cancel");
			Console.WriteLine("\tctrl + x to clear");
			Console.WriteLine();
			Console.WriteLine("Currently modifying " + valueName + " (" + previousValue + ")");
			Console.Write("\tnew value = " + previousValue);

			string input = previousValue;
			int currentWriteIndex = previousValue.Length;
			while(true) {
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);

				if(keyInfo.Key == ConsoleKey.Enter) {
					return input;
				} else if(keyInfo.Key == ConsoleKey.Escape) {
					return previousValue;
				} else if(keyInfo.Key == ConsoleKey.Backspace) {
					if(input.Length > 0) {
						if(currentWriteIndex < input.Length) {
							string remains = input.Substring(currentWriteIndex);
							input = input.Remove(currentWriteIndex - 1) + remains;
							Console.Write("\b \b" + remains + ' ' + new String('\b', remains.Length + 1));
						} else {
							input = input.Remove(input.Length - 1);
							Console.Write("\b \b");
						}
						currentWriteIndex--;
					}
				} else if(keyInfo.Key == ConsoleKey.Delete) {
					if(currentWriteIndex < input.Length) {
						string remains = input.Substring(currentWriteIndex + 1);
						input = input.Remove(currentWriteIndex) + remains;
						int charsToDelete = input.Length - currentWriteIndex;
						Console.Write(new string(' ', charsToDelete) + new string('\b', charsToDelete) + remains + new string('\b', charsToDelete));
					}
				} else if(keyInfo.Key == ConsoleKey.X && keyInfo.Modifiers == ConsoleModifiers.Control) {
					if(input.Length > 0) {
						Console.Write(RepeatString("\b \b", input.Length));
						input = "";
						currentWriteIndex = 0;
					}
				} else if(keyInfo.Key == ConsoleKey.LeftArrow) {
					if(currentWriteIndex > 0) {
						currentWriteIndex--;
						Console.Write('\b');
					}
				} else if(keyInfo.Key == ConsoleKey.RightArrow) {
					if(currentWriteIndex < input.Length) {
						Console.Write(input[currentWriteIndex]);
						currentWriteIndex++;
					}
				} else if(!Char.IsControl(keyInfo.KeyChar)) {
					if(currentWriteIndex < input.Length) {
						string remains = input.Substring(currentWriteIndex);
						input = input.Remove(currentWriteIndex) + keyInfo.KeyChar + remains;
						Console.Write(keyInfo.KeyChar + remains + new String('\b', remains.Length));
					} else {
						input += keyInfo.KeyChar;
						Console.Write(keyInfo.KeyChar);
					}
					currentWriteIndex++;
				}
			}
		}

		public static void OpenStringListEditor(string listName, ref List<string> listValues) {
			List<string> backup = ListUtils.CloneList(listValues);

			int selection = 0;
			string lastOption = "add new entry";
			string spacing = "\t";
			while(true) {
				Console.Clear();
				Console.WriteLine("ListEditor Controls:");
				Console.WriteLine("\tctrl + r to restore initial values and exit");
				Console.WriteLine("\t(ctrl + x) or (delete) to delete an entry");
				Console.WriteLine("\tenter to edit an entry");
				Console.WriteLine("\t(escape) or (backspace) to leave (keeps changes)");
				Console.WriteLine();
				Console.WriteLine("Currently modifying " + listName);
				PrintOptions(listValues.Select(item => item.ToString()).ToArray(), lastOption, spacing, selection);
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);
				MenuControl control = menuControlMapping[keyInfo.Key];
				if(control == MenuControl.Undefined) {
					if(keyInfo.Key == ConsoleKey.R && keyInfo.Modifiers == ConsoleModifiers.Control) {
						listValues = backup;
						return;
					} else if((keyInfo.Key == ConsoleKey.X && keyInfo.Modifiers == ConsoleModifiers.Control) || keyInfo.Key == ConsoleKey.Delete) {
						if(selection < listValues.Count) {
							listValues.RemoveAt(selection);
						}
					}
				} else if(control == MenuControl.Up) {
					if(selection > 0) {
						selection--;
					}
				} else if(control == MenuControl.Down) {
					if(selection < listValues.Count) {
						selection++;
					}
				} else if(control == MenuControl.Left || control == MenuControl.Back) {
					return;
				} else if(control == MenuControl.Right || control == MenuControl.Enter) {
					if(selection < listValues.Count) {
						listValues[selection] = OpenSimpleConfigEditor(listName + " #" + selection, listValues[selection]);
					} else {
						string newListEntry = OpenSimpleConfigEditor(listName + " #" + selection, "");
						if(!String.IsNullOrWhiteSpace(newListEntry)) {
							listValues.Add(newListEntry);
							selection++;
						}
					}
				}
			}
		}

		public static int OpenSelectionMenu(string[] options, string goBackText, string header, int selection, int spaceDepth) {
			string spacing = new String(' ', spaceDepth);

			int maxSelection = goBackText == null ? (options.Length - 1) : options.Length;
			while(true) {
				Console.Clear();
				if(header != null) {
					Console.WriteLine(header);
				}
				PrintOptions(options, goBackText, spacing, selection);
				MenuControl control = menuControlMapping[Console.ReadKey(true).Key];
				if(control == MenuControl.Up) {
					if(selection > 0) {
						selection--;
					}
				} else if(control == MenuControl.Down) {
					if(selection < maxSelection) {
						selection++;
					}
				} else if(control == MenuControl.Left || control == MenuControl.Back) {
					return options.Length;
				} else if(control == MenuControl.Right || control == MenuControl.Enter) {
					return selection;
				}
			}
		}

		public static int OpenSelectionMenu(string[] options, string goBackText, int selection, int spaceDepth) {
			return OpenSelectionMenu(options, goBackText, null, selection, spaceDepth);
		}

		private static void PrintOptions(string[] options, string goBackText, string spacing, int highlightLine) {
			for(int i = 0; i < options.Length; i++) {
				if(highlightLine == i) {
					PrintHighlightetText(options[i], spacing);
				} else {
					Console.WriteLine(spacing + options[i]);
				}
			}

			if(goBackText != null) {
				if(highlightLine == options.Length) {
					PrintHighlightetText(goBackText, spacing);
				} else {
					Console.WriteLine(spacing + goBackText);
				}
			}
		}

		private static void PrintHighlightetText(string text, string spacing) {
			Console.Write(spacing);
			Console.BackgroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(text);
			Console.ResetColor();
		}

		private static string RepeatString(string repeat, int count) {
			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < count; i++) {
				builder.Append(repeat);
			}
			return builder.ToString();
		}
	}
}

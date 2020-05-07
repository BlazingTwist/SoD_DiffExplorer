using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.menu
{
	class MenuUtils
	{
		public const int spacerWidth = 1;
		public static string GetChangeString(string name) {
			return "change " + name;
		}
		public static string GetToggleString(string name) {
			return "toggle " + name;
		}
		public static string GetConfigureString(string name) {
			return "configure " + name;
		}
		public static string GetFileSelectionString(string name) {
			return "select " + name + " from local files";
		}

		public static string GetSaveString(string name) {
			return "Save " + name;
		}

		private BetterDict<ConsoleKey, MenuControl> menuControlMapping;
		private MenuStyleConfig styleConfig;

		public MenuUtils(BetterDict<ConsoleKey, MenuControl> menuControlMapping, MenuStyleConfig styleConfig) {
			this.menuControlMapping = menuControlMapping;
			this.styleConfig = styleConfig;
		}

		public string AddTabsUntilTargetWidth(string spacing, string baseString, int targetWidth) {
			switch(styleConfig.textStyle.GetValue()) {
				case MenuTextStyle.split:
					int strLen = (spacing.Length + baseString.Length);
					if(strLen >= targetWidth) {
						return baseString;
					}
					int targetTabs = (targetWidth + (targetWidth % 8)) / 8;
					int textTabEquivalent = (strLen - (strLen % 8)) / 8;
					return baseString + new string('\t', targetTabs - textTabEquivalent);
				case MenuTextStyle.connected:
				case MenuTextStyle.full_line:
					return baseString + new string(' ', targetWidth - baseString.Length);
				default:
					throw new InvalidDataException("unknown menuTextStyle: " + styleConfig.textStyle.ToString());
			}
		}

		public void OpenObjectMenu(string header, string objectName, IMenuObject menuObject, int selection = 0, int spacing = 0) {
			string displayHeader = header + "." + objectName;
			string displaySpacing = new string(' ', spacing);
			string displayBackText = "Go back to " + header;

			while(true) {
				Console.Clear();
				Console.WriteLine(displayHeader + "\n");
				IMenuProperty[] options = menuObject.GetOptions();
				int maxSelection = options.Length;
				PrintOptions(BuildOptions(displaySpacing, options), displayBackText, displaySpacing, selection);
				MenuControl control = menuControlMapping[Console.ReadKey(true).Key];
				switch(control) {
					case MenuControl.Up:
						if(selection > 0) {
							selection--;
						}
						break;
					case MenuControl.Down:
						if(selection < maxSelection) {
							selection++;
						}
						break;
					case MenuControl.Left:
					case MenuControl.Back:
						return;
					case MenuControl.Right:
					case MenuControl.Enter:
						if(selection == maxSelection) {
							return;
						}
						options[selection].OnClick(this, displayHeader, spacing + spacerWidth);
						break;
				}
			}
		}

		private string[] BuildOptions(string spacing, IMenuProperty[] options) {
			if(options.Length == 0) {
				return new string[0];
			}
			int maxBaseTextWidth = options.Max(option => option.GetOptionText().Length) + spacing.Length;
			return options.Select(option => {
				if(option.GetInfoText() == null) {
					return option.GetOptionText();
				}
				return AddTabsUntilTargetWidth(spacing, option.GetOptionText(), maxBaseTextWidth + 8) + "(" + option.GetInfoText() + ")";
			}).ToArray();
		}

		public string OpenFileSelectionMenu(string baseDirectory, string previousValue, int spacing) {
			string[] options;
			string header;
			string backText;
			if(Directory.Exists(baseDirectory)) {
				options = Directory.GetFiles(baseDirectory);
				header = "Found " + options.Length + " available files:";
				backText = "cancel (" + previousValue + ")";
			} else {
				options = new string[0];
				header = "BaseDirectory (" + baseDirectory + ") does not exist!";
				backText = "go back";
			}
			int selection = OpenSelectionMenu(options, backText, header, FindCurrentEnumSelection(previousValue, options), spacing);

			if(selection >= options.Length) {
				return previousValue;
			}
			/*if(selection == (options.Length - 1)) {
				return options[selection].Substring(0, options[selection].Length - 1);
			}*/
			return options[selection];
		}

		public string OpenEnumConfigEditor(string valueName, string previousValue, string[] values, int spacing) {
			StringBuilder header = new StringBuilder();
			header.Append("EnumSelection Controls:\n");
			header.Append("\tEscape to cancel\n");
			header.Append("\tEnter to select\n\n");
			header.Append("Currently modifying " + valueName + " (" + previousValue + ")");

			int selection = OpenSelectionMenu(values, null, header.ToString(), FindCurrentEnumSelection(previousValue, values), spacing);

			if(selection >= values.Length) {
				return previousValue;
			}
			return values[selection];
		}

		private int FindCurrentEnumSelection(string selected, string[] available) {
			for(int i = 0; i < available.Length; i++) {
				if(available[i] == selected) {
					return i;
				}
			}
			return 0;
		}

		private string GetTextEditorControlsText(string modifyText) {
			StringBuilder result = new StringBuilder();
			result.Append("TextEditor Controls:\n");
			result.Append("\t(enter) to accept\n");
			result.Append("\t(escape) to cancel\n");
			result.Append("\t(ctrl + x) to clear all text\n\n");
			result.Append("Currently modifying ").Append(modifyText);
			return result.ToString();
		}

		public string OpenSimpleConfigEditor(string header, string valueName, string previousValue) {
			Console.Clear();
			Console.WriteLine(GetTextEditorControlsText(header + "." + valueName + " (" + previousValue) + ")");
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

		private string GetListEditorControlsText(string listName) {
			StringBuilder result = new StringBuilder();
			result.Append("ListEditor Controls:\n");
			result.Append("\t(ctrl + r) to restore initial values and exit\n");
			result.Append("\t(ctrl + x) or (delete) to delete an entry\n");
			result.Append("\t(enter) to edit an entry\n");
			result.Append("\t(escape) or (backspace) to leave (keeps changes)\n\n");
			result.Append("Currently modifying ").Append(listName);
			return result.ToString();
		}

		public void OpenStringListEditor(string listName, ref List<string> listValues) {
			List<string> backup = ListUtils.CloneList(listValues);

			int selection = 0;
			string lastOption = "add new entry";
			string spacing = "\t";
			while(true) {
				Console.Clear();
				Console.WriteLine(GetListEditorControlsText(listName));
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
						string val = listValues[selection];
						listValues[selection] = OpenSimpleConfigEditor(listName + " #" + selection, "\b", val);
					} else {
						string newListEntry = OpenSimpleConfigEditor(listName + " #" + selection, "\b", "");
						if(!String.IsNullOrWhiteSpace(newListEntry)) {
							listValues.Add(newListEntry);
							selection++;
						}
					}
				}
			}
		}

		public int OpenSelectionMenu(string[] options, string goBackText, string header, int selection, int spaceDepth) {
			string spacing = new String(' ', spaceDepth);

			int maxSelection = goBackText == null ? (options.Length - 1) : options.Length;
			while(true) {
				Console.Clear();
				if(header != null) {
					Console.WriteLine(header + "\n");
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

		public int OpenSelectionMenu(string[] options, string goBackText, int selection, int spaceDepth) {
			return OpenSelectionMenu(options, goBackText, null, selection, spaceDepth);
		}

		private void PrintOptions(string[] options, string goBackText, string spacing, int highlightLine) {
			List<string> realOptions = new List<string>();
			realOptions.AddRange(options);
			if(goBackText != null) {
				if(realOptions.Count != 0) {
					realOptions[realOptions.Count - 1] += "\n";
				}
				realOptions.Add(goBackText);
			}

			if(highlightLine < 0 || highlightLine >= realOptions.Count) {
				StringBuilder optionsString = new StringBuilder();
				for(int i = 0; i < realOptions.Count; i++) {
					optionsString.Append(spacing).Append(realOptions[i]).Append("\n");
				}
				Console.Write(optionsString.ToString());
			} else {
				StringBuilder optionsString = new StringBuilder();
				for(int i = 0; i < highlightLine; i++) {
					optionsString.Append(spacing).Append(realOptions[i]).Append("\n");
				}
				Console.Write(optionsString.ToString());

				PrintHightlightedText(highlightLine, realOptions.ToArray(), spacing);

				optionsString = new StringBuilder();
				for(int i = (highlightLine + 1); i < realOptions.Count; i++) {
					optionsString.Append(spacing).Append(realOptions[i]).Append("\n");
				}
				Console.Write(optionsString.ToString());
			}
		}

		private void PrintHightlightedText(int selection, string[] options, string spacing) {
			switch(styleConfig.textStyle.GetValue()) {
				case MenuTextStyle.split:
				case MenuTextStyle.connected:
					PrintHighlightedTextNormal(options[selection], spacing);
					break;
				case MenuTextStyle.full_line:
					PrintHighlightedTextFullLine(selection, options, spacing);
					break;
				default:
					throw new InvalidDataException("unknown menuTextStyle: " + styleConfig.textStyle.ToString());
			}
		}

		private void PrintHighlightedTextNormal(string text, string spacing) {
			Console.Write(spacing);
			Console.BackgroundColor = styleConfig.selectedBackgroundColor.GetValue();
			Console.ForegroundColor = styleConfig.selectedTextColor.GetValue();
			Console.WriteLine(text);
			Console.BackgroundColor = styleConfig.normalBackgroundColor.GetValue();
			Console.ForegroundColor = styleConfig.normalTextColor.GetValue();
		}

		private void PrintHighlightedTextFullLine(int selection, string[] options, string spacing) {
			string targetText;

			string selected = options[selection];
			int maxWidth = options.Max(option => option.Length);
			int selectedWidth = selected.Length;
			int spacingWidth = spacing.Length;
			if(options[selection].EndsWith('\n')) {
				targetText = spacing + selected.Substring(0, selectedWidth - 1) + new string(' ', spacingWidth + maxWidth + 1 - selectedWidth) + "\n";
			} else {
				targetText = spacing + selected + new string(' ', spacingWidth + maxWidth - selectedWidth);
			}
			Console.BackgroundColor = styleConfig.selectedBackgroundColor.GetValue();
			Console.ForegroundColor = styleConfig.selectedTextColor.GetValue();
			Console.WriteLine(targetText);
			Console.BackgroundColor = styleConfig.normalBackgroundColor.GetValue();
			Console.ForegroundColor = styleConfig.normalTextColor.GetValue();
		}

		private string RepeatString(string repeat, int count) {
			StringBuilder builder = new StringBuilder();
			for(int i = 0; i < count; i++) {
				builder.Append(repeat);
			}
			return builder.ToString();
		}
	}
}

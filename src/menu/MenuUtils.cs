using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.menu {
	public class MenuUtils {
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

		public static string GetSaveString(string name) {
			return "Save " + name;
		}

		private readonly BetterDict<ConsoleKey, MenuControl> menuControlMapping;
		private readonly MenuStyleConfig styleConfig;

		public MenuUtils(BetterDict<ConsoleKey, MenuControl> menuControlMapping, MenuStyleConfig styleConfig) {
			this.menuControlMapping = menuControlMapping;
			this.styleConfig = styleConfig;
		}

		private string AddTabsUntilTargetWidth(string spacing, string baseString, int targetWidth) {
			switch (styleConfig.textStyle.GetValue()) {
				case MenuTextStyle.split:
					int strLen = (spacing.Length + baseString.Length);
					if (strLen >= targetWidth) {
						return baseString;
					}

					int targetTabs = (targetWidth + (targetWidth % 8)) / 8;
					int textTabEquivalent = (strLen - (strLen % 8)) / 8;
					return baseString + new string('\t', targetTabs - textTabEquivalent);
				case MenuTextStyle.connected:
				case MenuTextStyle.full_line:
					return baseString + new string(' ', targetWidth - baseString.Length);
				default:
					throw new InvalidDataException("unknown menuTextStyle: " + styleConfig.textStyle);
			}
		}

		public void OpenObjectMenu(string header, string objectName, IMenuObject menuObject, int selection = 0, int spacing = 0) {
			string displayHeader = header + "." + objectName;
			string displaySpacing = new string(' ', spacing);
			string displayBackText = "Go back to " + header;

			while (true) {
				Console.Clear();
				Console.WriteLine(displayHeader + "\n");
				IMenuProperty[] options = menuObject.GetOptions();
				int maxSelection = options.Length;
				PrintOptions(BuildOptions(displaySpacing, options), displayBackText, displaySpacing, selection);
				MenuControl control = menuControlMapping[Console.ReadKey(true).Key];
				switch (control) {
					case MenuControl.Up:
						if (selection > 0) {
							selection--;
						}

						break;
					case MenuControl.Down:
						if (selection < maxSelection) {
							selection++;
						}

						break;
					case MenuControl.Left:
					case MenuControl.Back:
						return;
					case MenuControl.Right:
					case MenuControl.Enter:
						if (selection == maxSelection) {
							return;
						}

						options[selection].OnClick(this, displayHeader, spacing + spacerWidth);
						break;
				}
			}
		}

		private string[] BuildOptions(string spacing, IMenuProperty[] options) {
			if (options.Length == 0) {
				return new string[0];
			}

			int maxBaseTextWidth = options.Max(option => option.GetOptionText().Length) + spacing.Length;
			return options.Select(option => {
				if (option.GetInfoText() == null) {
					return option.GetOptionText();
				}

				return AddTabsUntilTargetWidth(spacing, option.GetOptionText(), maxBaseTextWidth + 8) + "(" + option.GetInfoText() + ")";
			}).ToArray();
		}

		public string OpenFileSelectionMenu(string baseDirectory, string previousValue, int spacing) {
			string[] options;
			string header;
			string backText;
			if (Directory.Exists(baseDirectory)) {
				options = Directory.GetFiles(baseDirectory);
				header = "Found " + options.Length + " available files:";
				backText = "cancel (" + previousValue + ")";
			} else {
				options = new string[0];
				header = "BaseDirectory (" + baseDirectory + ") does not exist!";
				backText = "go back";
			}

			int selection = OpenSelectionMenu(options, backText, header, FindCurrentEnumSelection(previousValue, options), spacing);

			return selection >= options.Length
					? previousValue
					: options[selection];
		}

		public string OpenEnumConfigEditor(string valueName, string previousValue, string[] values, int spacing) {
			var header = new StringBuilder();
			header.Append("EnumSelection Controls:\n");
			header.Append("\tEscape to cancel\n");
			header.Append("\tEnter to select\n\n");
			header.Append("Currently modifying " + valueName + " (" + previousValue + ")");

			int selection = OpenSelectionMenu(values, null, header.ToString(), FindCurrentEnumSelection(previousValue, values), spacing);

			return selection >= values.Length
					? previousValue
					: values[selection];
		}

		private static int FindCurrentEnumSelection(string selected, IReadOnlyList<string> available) {
			for (int i = 0; i < available.Count; i++) {
				if (available[i] == selected) {
					return i;
				}
			}

			return 0;
		}

		private static string GetTextEditorControlsText(string modifyText) {
			var result = new StringBuilder();
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
			while (true) {
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);

				// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
				switch (keyInfo.Key) {
					case ConsoleKey.Enter:
						return input;
					case ConsoleKey.Escape:
						return previousValue;
					case ConsoleKey.Backspace: {
						if (input.Length > 0) {
							if (currentWriteIndex < input.Length) {
								string remains = input.Substring(currentWriteIndex);
								input = input.Remove(currentWriteIndex - 1) + remains;
								Console.Write("\b \b" + remains + ' ' + new String('\b', remains.Length + 1));
							} else {
								input = input.Remove(input.Length - 1);
								Console.Write("\b \b");
							}

							currentWriteIndex--;
						}

						break;
					}
					case ConsoleKey.Delete: {
						if (currentWriteIndex < input.Length) {
							string remains = input.Substring(currentWriteIndex + 1);
							input = input.Remove(currentWriteIndex) + remains;
							int charsToDelete = input.Length - currentWriteIndex;
							Console.Write(new string(' ', charsToDelete) + new string('\b', charsToDelete) + remains + new string('\b', charsToDelete));
						}

						break;
					}
					case ConsoleKey.X when keyInfo.Modifiers == ConsoleModifiers.Control: {
						if (input.Length > 0) {
							Console.Write(RepeatString("\b \b", input.Length));
							input = "";
							currentWriteIndex = 0;
						}

						break;
					}
					case ConsoleKey.LeftArrow: {
						if (currentWriteIndex > 0) {
							currentWriteIndex--;
							Console.Write('\b');
						}

						break;
					}
					case ConsoleKey.RightArrow: {
						if (currentWriteIndex < input.Length) {
							Console.Write(input[currentWriteIndex]);
							currentWriteIndex++;
						}

						break;
					}
					default: {
						if (!char.IsControl(keyInfo.KeyChar)) {
							if (currentWriteIndex < input.Length) {
								string remains = input.Substring(currentWriteIndex);
								input = input.Remove(currentWriteIndex) + keyInfo.KeyChar + remains;
								Console.Write(keyInfo.KeyChar + remains + new string('\b', remains.Length));
							} else {
								input += keyInfo.KeyChar;
								Console.Write(keyInfo.KeyChar);
							}

							currentWriteIndex++;
						}

						break;
					}
				}
			}
		}

		public int OpenSelectionMenu(string[] options, string goBackText, string header, int selection, int spaceDepth) {
			string spacing = new string(' ', spaceDepth);

			int maxSelection = goBackText == null ? (options.Length - 1) : options.Length;
			while (true) {
				Console.Clear();
				if (header != null) {
					Console.WriteLine(header + "\n");
				}

				PrintOptions(options, goBackText, spacing, selection);
				MenuControl control = menuControlMapping[Console.ReadKey(true).Key];
				switch (control) {
					case MenuControl.Up: {
						if (selection > 0) {
							selection--;
						}

						break;
					}
					case MenuControl.Down: {
						if (selection < maxSelection) {
							selection++;
						}

						break;
					}
					case MenuControl.Left:
					case MenuControl.Back:
						return options.Length;
					case MenuControl.Right:
					case MenuControl.Enter:
						return selection;
					case MenuControl.Undefined:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void PrintOptions(string[] options, string goBackText, string spacing, int highlightLine) {
			List<string> realOptions = new List<string>();
			realOptions.AddRange(options);
			if (goBackText != null) {
				if (realOptions.Count != 0) {
					realOptions[^1] += "\n";
				}

				realOptions.Add(goBackText);
			}

			if (highlightLine < 0 || highlightLine >= realOptions.Count) {
				var optionsString = new StringBuilder();
				foreach (string t in realOptions) {
					optionsString.Append(spacing).Append(t).Append("\n");
				}

				Console.Write(optionsString.ToString());
			} else {
				var optionsString = new StringBuilder();
				for (int i = 0; i < highlightLine; i++) {
					optionsString.Append(spacing).Append(realOptions[i]).Append("\n");
				}

				Console.Write(optionsString.ToString());

				PrintHighlightedText(highlightLine, realOptions.ToArray(), spacing);

				optionsString = new StringBuilder();
				for (int i = (highlightLine + 1); i < realOptions.Count; i++) {
					optionsString.Append(spacing).Append(realOptions[i]).Append("\n");
				}

				Console.Write(optionsString.ToString());
			}
		}

		private void PrintHighlightedText(int selection, string[] options, string spacing) {
			switch (styleConfig.textStyle.GetValue()) {
				case MenuTextStyle.split:
				case MenuTextStyle.connected:
					PrintHighlightedTextNormal(options[selection], spacing);
					break;
				case MenuTextStyle.full_line:
					PrintHighlightedTextFullLine(selection, options, spacing);
					break;
				default:
					throw new InvalidDataException("unknown menuTextStyle: " + styleConfig.textStyle);
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

		private void PrintHighlightedTextFullLine(int selection, IReadOnlyList<string> options, string spacing) {
			string targetText;

			string selected = options[selection];
			int maxWidth = options.Max(option => option.Length);
			int selectedWidth = selected.Length;
			int spacingWidth = spacing.Length;
			if (options[selection].EndsWith('\n')) {
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
			var builder = new StringBuilder();
			for (int i = 0; i < count; i++) {
				builder.Append(repeat);
			}

			return builder.ToString();
		}
	}
}
using System;

namespace SoD_DiffExplorer.menu {
	public class MenuOption : IMenuProperty {
		private readonly string fieldName;
		private readonly Func<string, string> optionTextProvider;
		private readonly Func<string> infoTextProvider;
		private readonly Action<MenuUtils, string, int> onClick;

		public MenuOption(string fieldName, Func<string, string> optionTextProvider, Func<string> infoTextProvider, Action<MenuUtils, string, int> onClick) {
			this.fieldName = fieldName;
			this.optionTextProvider = optionTextProvider;
			this.infoTextProvider = infoTextProvider;
			this.onClick = onClick;
		}

		void IMenuProperty.ParseValue(object value) {
			throw new NotImplementedException("Can't parse to MenuOption!");
		}

		Type IMenuProperty.GetInnerType() {
			throw new NotImplementedException("MenuOption has no type!");
		}

		string IMenuProperty.GetFieldName() {
			return fieldName;
		}

		string IMenuProperty.GetOptionText() {
			return optionTextProvider(fieldName);
		}

		string IMenuProperty.GetInfoText() {
			return infoTextProvider();
		}

		void IMenuProperty.OnClick(MenuUtils menuUtils, string header, int spacing) {
			onClick(menuUtils, header, spacing);
		}
	}
}
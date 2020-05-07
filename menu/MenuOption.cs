using System;

namespace SoD_DiffExplorer.menu
{
	class MenuOption : IMenuProperty
	{
		public string fieldName = null;
		public Func<string, string> optionTextProvider = null;
		public Func<string> infoTextProvider = null;
		public Action<MenuUtils, string, int> onClick = null;

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

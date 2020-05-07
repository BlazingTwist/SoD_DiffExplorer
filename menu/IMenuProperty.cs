using System;

namespace SoD_DiffExplorer.menu
{
	interface IMenuProperty
	{
		public string GetFieldName();

		public string GetOptionText();

		public string GetInfoText();

		public void ParseValue(object value);

		public Type GetInnerType();

		public void OnClick(MenuUtils menuUtils, string header, int spacing);
	}
}

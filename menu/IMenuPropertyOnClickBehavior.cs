using System;

namespace SoD_DiffExplorer.menu
{
	interface IMenuPropertyOnClickBehavior<T>
	{
		public Func<string, string> GetOptionText();

		public string GetInfoText(IMenuPropertyAccessor<T> property);

		public void OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<T> property, string header, int spacing);
	}
}

using System;

namespace SoD_DiffExplorer.menu
{
	class MenuPropertyCustomBehavior<T> : IMenuPropertyOnClickBehavior<T>
	{
		Func<string, string> optionTextProvider = null;
		Action<MenuUtils, IMenuPropertyAccessor<T>, string, int> onClick = null;

		public MenuPropertyCustomBehavior(Func<string, string> optionTextProvider, Action<MenuUtils, IMenuPropertyAccessor<T>, string, int> onClick) {
			this.optionTextProvider = optionTextProvider;
			this.onClick = onClick;
		}

		Func<string, string> IMenuPropertyOnClickBehavior<T>.GetOptionText() {
			return optionTextProvider;
		}

		string IMenuPropertyOnClickBehavior<T>.GetInfoText(IMenuPropertyAccessor<T> property) {
			if(typeof(T) is IMenuObject) {
				return (property.GetValue() as IMenuObject).GetInfoString();
			}
			return property.GetValue().ToString();
		}

		void IMenuPropertyOnClickBehavior<T>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<T> property, string header, int spacing) {
			onClick(menuUtils, property, header, spacing);
		}
	}
}

using System;

namespace SoD_DiffExplorer.menu {
	public class MenuPropertyCustomBehavior<T> : IMenuPropertyOnClickBehavior<T> {
		private readonly Func<string, string> optionTextProvider;
		private readonly Action<MenuUtils, IMenuPropertyAccessor<T>, string, int> onClick;

		public MenuPropertyCustomBehavior(Func<string, string> optionTextProvider, Action<MenuUtils, IMenuPropertyAccessor<T>, string, int> onClick) {
			this.optionTextProvider = optionTextProvider;
			this.onClick = onClick;
		}

		Func<string, string> IMenuPropertyOnClickBehavior<T>.GetOptionText() {
			return optionTextProvider;
		}

		string IMenuPropertyOnClickBehavior<T>.GetInfoText(IMenuPropertyAccessor<T> property) {
			return property.GetValue().ToString();
		}

		void IMenuPropertyOnClickBehavior<T>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<T> property, string header, int spacing) {
			onClick(menuUtils, property, header, spacing);
		}
	}
}
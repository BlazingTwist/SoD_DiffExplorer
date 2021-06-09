using System;

namespace SoD_DiffExplorer.menu {
	public class MenuPropertyObjectBehavior<T> : IMenuPropertyOnClickBehavior<T>
			where T : IMenuObject {
		Func<string, string> IMenuPropertyOnClickBehavior<T>.GetOptionText() {
			return MenuUtils.GetConfigureString;
		}

		string IMenuPropertyOnClickBehavior<T>.GetInfoText(IMenuPropertyAccessor<T> property) {
			return property.GetValue().GetInfoString();
		}

		void IMenuPropertyOnClickBehavior<T>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<T> property, string header, int spacing) {
			menuUtils.OpenObjectMenu(header, property.GetFieldName(), property.GetValue(), 0, spacing);
		}
	}
}
using System;

namespace SoD_DiffExplorer.menu {
	public class MenuPropertyToggleBehavior : IMenuPropertyOnClickBehavior<bool> {
		Func<string, string> IMenuPropertyOnClickBehavior<bool>.GetOptionText() {
			return MenuUtils.GetToggleString;
		}

		string IMenuPropertyOnClickBehavior<bool>.GetInfoText(IMenuPropertyAccessor<bool> property) {
			return property.GetValue().ToString();
		}

		void IMenuPropertyOnClickBehavior<bool>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<bool> property, string header, int spacing) {
			property.SetValue(!property.GetValue());
		}
	}
}
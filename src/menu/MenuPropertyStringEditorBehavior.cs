using System;

namespace SoD_DiffExplorer.menu {
	public class MenuPropertyStringEditorBehavior : IMenuPropertyOnClickBehavior<string> {
		Func<string, string> IMenuPropertyOnClickBehavior<string>.GetOptionText() {
			return MenuUtils.GetChangeString;
		}

		string IMenuPropertyOnClickBehavior<string>.GetInfoText(IMenuPropertyAccessor<string> property) {
			return property.GetValue();
		}

		void IMenuPropertyOnClickBehavior<string>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<string> property, string header, int spacing) {
			property.SetValue(menuUtils.OpenSimpleConfigEditor(header, property.GetFieldName(), property.GetValue()));
		}
	}
}
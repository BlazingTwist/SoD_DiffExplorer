using System;

namespace SoD_DiffExplorer.menu {
	public class MenuPropertyEnumSelectionBehavior<T> : IMenuPropertyOnClickBehavior<T>
			where T : struct, Enum {
		Func<string, string> IMenuPropertyOnClickBehavior<T>.GetOptionText() {
			return MenuUtils.GetChangeString;
		}

		string IMenuPropertyOnClickBehavior<T>.GetInfoText(IMenuPropertyAccessor<T> property) {
			return property.GetValue().ToString();
		}

		void IMenuPropertyOnClickBehavior<T>.OnClick(MenuUtils menuUtils, IMenuPropertyAccessor<T> property, string header, int spacing) {
			property.SetValue(Enum.Parse<T>(menuUtils.OpenEnumConfigEditor(header + "." + property.GetFieldName(), property.GetValue().ToString(),
					Enum.GetNames(typeof(T)), spacing)));
		}
	}
}
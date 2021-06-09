using System;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.menu {
	public class MenuOptionPropertyEnum<T> : IMenuPropertyAccessor<T>
			where T : struct, Enum {
		[YamlIgnore] private readonly string fieldName;
		[YamlIgnore] private readonly IMenuPropertyOnClickBehavior<T> onClickBehavior;
		private T value;

		public MenuOptionPropertyEnum(string fieldName, IMenuPropertyOnClickBehavior<T> onClickBehavior) {
			this.fieldName = fieldName;
			this.onClickBehavior = onClickBehavior;
		}

		public override string ToString() {
			return value.ToString();
		}

		string IMenuProperty.GetFieldName() {
			return fieldName;
		}

		T IMenuPropertyAccessor<T>.GetValue() {
			return value;
		}

		void IMenuPropertyAccessor<T>.SetValue(T value) {
			this.value = value;
		}

		void IMenuProperty.ParseValue(object value) {
			this.value = (T) Convert.ChangeType(value, typeof(T));
		}

		Type IMenuProperty.GetInnerType() {
			return typeof(T);
		}

		void IMenuProperty.OnClick(MenuUtils menuUtils, string header, int spacing) {
			onClickBehavior.OnClick(menuUtils, this, header, spacing);
		}

		string IMenuProperty.GetOptionText() {
			return onClickBehavior.GetOptionText()(fieldName);
		}

		string IMenuProperty.GetInfoText() {
			return onClickBehavior.GetInfoText(this);
		}
	}
}
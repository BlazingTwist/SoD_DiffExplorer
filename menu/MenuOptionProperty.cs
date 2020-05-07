using YamlDotNet.Serialization;
using YamlDotNet.Core;
using System;

namespace SoD_DiffExplorer.menu
{
	class MenuOptionProperty<T> : IMenuPropertyAccessor<T>
	{
		[YamlIgnore]
		public string fieldName = null;

		[YamlIgnore]
		public IMenuPropertyOnClickBehavior<T> onClickBehavior = null;

		public T value;

		public MenuOptionProperty(string fieldName, IMenuPropertyOnClickBehavior<T> onClickBehavior) {
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
			this.value = (T)Convert.ChangeType(value, typeof(T));
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

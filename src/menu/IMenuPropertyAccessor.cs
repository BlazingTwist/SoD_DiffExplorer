namespace SoD_DiffExplorer.menu {
	public interface IMenuPropertyAccessor<T> : IMenuProperty {
		public T GetValue();
		public void SetValue(T value);
	}
}
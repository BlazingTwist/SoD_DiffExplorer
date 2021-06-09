namespace SoD_DiffExplorer.menu {
	public interface IMenuObject {
		public string GetInfoString();
		public IMenuProperty[] GetOptions();
	}
}
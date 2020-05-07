namespace SoD_DiffExplorer.menu
{
	interface IMenuObject
	{
		public string GetInfoString();
		public IMenuProperty[] GetOptions();
	}
}

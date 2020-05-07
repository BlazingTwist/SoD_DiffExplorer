using YamlDotNet.Serialization;
namespace SoD_DiffExplorer.menu
{
	interface IMenuPropertyAccessor<T> : IMenuProperty
	{
		public T GetValue();

		public void SetValue(T value);
	}
}

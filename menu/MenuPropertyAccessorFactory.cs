using SoD_DiffExplorer.csutils;
using System;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.menu
{
	class MenuPropertyAccessorFactory : IObjectFactory
	{
		private readonly IObjectFactory fallback;

		public MenuPropertyAccessorFactory(IObjectFactory fallback) {
			this.fallback = fallback;
		}

		object IObjectFactory.Create(Type type) {
			if(type is IMenuPropertyAccessor<bool>) {
				return new MenuOptionProperty<bool>("", new MenuPropertyToggleBehavior());
			}else if(type is IMenuPropertyAccessor<string>) {
				return new MenuOptionProperty<bool>("", new MenuPropertyToggleBehavior());
			}else if(type is IMenuPropertyAccessor<Enum>) {
				return new MenuOptionProperty<bool>("", new MenuPropertyToggleBehavior());
			}else if(type is IMenuPropertyAccessor<YamlObject>) {
				return new MenuOptionProperty<bool>("", new MenuPropertyToggleBehavior());
			} else {
				return fallback.Create(type);
			}
		}
	}
}

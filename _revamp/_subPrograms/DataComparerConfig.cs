using System;
using System.Collections.Generic;
using SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig;
using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer._revamp._config._resultConfig;
using SoD_DiffExplorer._revamp._config._sourceConfig;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;

namespace SoD_DiffExplorer._revamp._subPrograms
{
	class DataComparerConfig : YamlObject, IMenuObject
	{
		public IMenuPropertyAccessor<SourceConfigHolder> sourceConfigHolder = new MenuOptionProperty<SourceConfigHolder>(nameof(sourceConfigHolder), new MenuPropertyObjectBehavior<SourceConfigHolder>());
		public IMenuPropertyAccessor<ResultConfig> resultConfig = new MenuOptionProperty<ResultConfig>(nameof(resultConfig), new MenuPropertyObjectBehavior<ResultConfig>());
		public IMenuPropertyAccessor<OnlineSourceInterpreterConfig> onlineSourceInterpreterConfig = new MenuOptionProperty<OnlineSourceInterpreterConfig>(nameof(onlineSourceInterpreterConfig), new MenuPropertyObjectBehavior<OnlineSourceInterpreterConfig>());

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			sourceConfigHolder.GetValue().Init(addressDictSupplier);
		}

		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
				{nameof(sourceConfigHolder), sourceConfigHolder.GetValue()},
				{nameof(resultConfig), resultConfig.GetValue()},
				{nameof(onlineSourceInterpreterConfig), onlineSourceInterpreterConfig.GetValue()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeYamlObjects(ref lines, startLine, ref endLine, currentTabDepth, GetObjectChangeDict());
		}

		string IMenuObject.GetInfoString() {
			//not supposed to be called!
			throw new NotImplementedException();
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[]{
				sourceConfigHolder,
				resultConfig,
				onlineSourceInterpreterConfig
			};
		}
	}
}

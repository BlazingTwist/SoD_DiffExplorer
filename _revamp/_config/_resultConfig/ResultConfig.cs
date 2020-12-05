using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using System.Collections.Generic;

namespace SoD_DiffExplorer._revamp._config._resultConfig
{
	class ResultConfig : YamlObject, IMenuObject
	{
		public IMenuPropertyAccessor<string> baseDirectory = new MenuOptionProperty<string>(nameof(baseDirectory), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<bool> makeFile = new MenuOptionProperty<bool>(nameof(makeFile), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> appendDate = new MenuOptionProperty<bool>(nameof(appendDate), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> appendTime = new MenuOptionProperty<bool>(nameof(appendTime), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<ResultFilter> resultFilter = new MenuOptionProperty<ResultFilter>(nameof(resultFilter), new MenuPropertyObjectBehavior<ResultFilter>());

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(makeFile), makeFile.ToString()},
				{nameof(appendDate), appendDate.ToString()},
				{nameof(appendTime), appendTime.ToString()}
			};
		}

		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
				{nameof(resultFilter), resultFilter.GetValue()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			if(!YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict())) {
				return false;
			}
			if(!YamlUtils.ChangeYamlObjects(ref lines, startLine, ref endLine, currentTabDepth, GetObjectChangeDict())) {
				return false;
			}
			return true;
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
				nameof(makeFile) + " = " + makeFile,
				nameof(appendDate) + " = " + appendDate,
				nameof(appendTime) + " = " + appendTime,
				nameof(resultFilter)
				);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
				makeFile,
				appendDate,
				appendTime,
				resultFilter
			};
		}
	}
}

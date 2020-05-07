using SoD_DiffExplorer.menu;
using System.Collections.Generic;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class LocalSourcesConfig : YamlObject, IMenuObject
	{
		public string baseDirectory = null;
		public string targetFileName = null;
		public string targetFileExtension = null;
		public IMenuPropertyAccessor<bool> appendPlatform = new MenuOptionProperty<bool>(nameof(appendPlatform), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> appendVersion = new MenuOptionProperty<bool>(nameof(appendVersion), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> appendDate = new MenuOptionProperty<bool>(nameof(appendDate), new MenuPropertyToggleBehavior());

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(appendPlatform), appendPlatform.ToString()},
				{nameof(appendVersion), appendVersion.ToString()},
				{nameof(appendDate), appendDate.ToString()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
				nameof(appendPlatform) + " = " + appendPlatform,
				nameof(appendVersion) + " = " + appendVersion,
				nameof(appendDate) + " = " + appendDate
				);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[]{
				appendPlatform,
				appendVersion,
				appendDate
			};
		}
	}
}

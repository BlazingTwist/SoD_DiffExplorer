using SoD_DiffExplorer.csutils;
using System.Collections.Generic;
using SoD_DiffExplorer.menu;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class LocalSource : YamlObject, IMenuObject
	{
		public IMenuPropertyAccessor<string> platform = new MenuOptionProperty<string>(nameof(platform), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<string> version = new MenuOptionProperty<string>(nameof(version), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<string> date = new MenuOptionProperty<string>(nameof(date), new MenuPropertyStringEditorBehavior());

		private BetterDict<string, string> GetChangeDict() {
			return new BetterDict<string, string> {
				{nameof(platform), platform.ToString()},
				{nameof(version), version.ToString()},
				{nameof(date), date.ToString()}
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetChangeDict());
		}

		public string GetShortInfoString() {
			return string.Join(", ",
				platform.ToString(),
				version.ToString(),
				date.ToString()
				);
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
				nameof(platform) + " = " + platform,
				nameof(version) + " = " + version,
				nameof(date) + " = " + date
				);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
				platform,
				version,
				date
			};
		}
	}
}

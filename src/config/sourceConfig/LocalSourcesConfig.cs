using System.Collections.Generic;
using JetBrains.Annotations;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.sourceConfig {
	[PublicAPI]
	public class LocalSourcesConfig : YamlObject, IMenuObject {
		public string baseDirectory;
		public string targetFileName;
		public string targetFileExtension;

		public IMenuPropertyAccessor<bool> appendPlatform = new MenuOptionProperty<bool>(
				nameof(appendPlatform),
				new MenuPropertyToggleBehavior());

		public IMenuPropertyAccessor<bool> appendVersion = new MenuOptionProperty<bool>(
				nameof(appendVersion),
				new MenuPropertyToggleBehavior());

		public IMenuPropertyAccessor<bool> appendDate = new MenuOptionProperty<bool>(
				nameof(appendDate),
				new MenuPropertyToggleBehavior());

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
					{ nameof(appendPlatform), appendPlatform.ToString() },
					{ nameof(appendVersion), appendVersion.ToString() },
					{ nameof(appendDate), appendDate.ToString() }
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
			return new IMenuProperty[] {
					appendPlatform,
					appendVersion,
					appendDate
			};
		}
	}
}
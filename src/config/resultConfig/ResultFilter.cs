using System.Collections.Generic;
using JetBrains.Annotations;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.resultConfig {
	[PublicAPI]
	public class ResultFilter : YamlObject, IMenuObject {
		public IMenuPropertyAccessor<bool> displayAdditions = new MenuOptionProperty<bool>(nameof(displayAdditions), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> displayDifferences = new MenuOptionProperty<bool>(nameof(displayDifferences), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> displayRemovals = new MenuOptionProperty<bool>(nameof(displayRemovals), new MenuPropertyToggleBehavior());
		public IMenuPropertyAccessor<bool> displayCommons = new MenuOptionProperty<bool>(nameof(displayCommons), new MenuPropertyToggleBehavior());

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
					{ displayAdditions.GetFieldName(), displayAdditions.ToString() },
					{ displayDifferences.GetFieldName(), displayDifferences.ToString() },
					{ displayRemovals.GetFieldName(), displayRemovals.ToString() },
					{ displayCommons.GetFieldName(), displayCommons.ToString() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
					nameof(displayAdditions) + " = " + displayAdditions,
					nameof(displayDifferences) + " = " + displayDifferences,
					nameof(displayRemovals) + " = " + displayRemovals,
					nameof(displayCommons) + " = " + displayCommons
			);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
					displayAdditions,
					displayDifferences,
					displayRemovals,
					displayCommons
			};
		}
	}
}
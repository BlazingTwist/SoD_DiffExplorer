﻿using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.sourceConfig {
	[PublicAPI]
	public class OnlineSource : YamlObject, IMenuObject, IOnlineSourceHolder {
		public IMenuPropertyAccessor<string> platform = new MenuOptionProperty<string>(
				nameof(platform),
				new MenuPropertyStringEditorBehavior());

		public IMenuPropertyAccessor<string> version = new MenuOptionProperty<string>(
				nameof(version),
				new MenuPropertyStringEditorBehavior());

		public IMenuPropertyAccessor<bool> makeFile = new MenuOptionProperty<bool>(
				nameof(makeFile),
				new MenuPropertyToggleBehavior());

		public IMenuPropertyAccessor<bool> makeLastCreated = new MenuOptionProperty<bool>(
				nameof(makeLastCreated),
				new MenuPropertyToggleBehavior());

		string IOnlineSourceHolder.GetPlatform() {
			return platform.GetValue();
		}

		string IOnlineSourceHolder.GetVersion() {
			return version.GetValue();
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
					{ nameof(platform), platform.ToString() },
					{ nameof(version), version.ToString() },
					{ nameof(makeFile), makeFile.ToString() },
					{ nameof(makeLastCreated), makeLastCreated.ToString() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		public string GetShortInfoString() {
			var result = new StringBuilder();
			result.Append("{").Append(platform).Append(", ").Append(version).Append(", ").Append(makeFile);
			if (makeFile.GetValue()) {
				result.Append(", ").Append(makeLastCreated);
			}

			result.Append("}");
			return result.ToString();
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ",
					nameof(platform) + " = " + platform,
					nameof(version) + " = " + version,
					nameof(makeFile) + " = " + makeFile,
					nameof(makeLastCreated) + " = " + makeLastCreated
			);
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
					platform,
					version,
					makeFile,
					makeLastCreated
			};
		}
	}
}
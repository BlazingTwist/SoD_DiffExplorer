using System.Collections.Generic;
using JetBrains.Annotations;
using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;

namespace SoD_DiffExplorer.config.downloadSettings {
	[PublicAPI]
	public class DownloadSettings : YamlObject, IMenuObject {
		public IMenuPropertyAccessor<bool> pauseDownloadOnError = new MenuOptionProperty<bool>(
				nameof(pauseDownloadOnError),
				new MenuPropertyToggleBehavior());

		public IMenuPropertyAccessor<bool> doDownload = new MenuOptionProperty<bool>(
				nameof(doDownload),
				new MenuPropertyToggleBehavior());

		public IMenuPropertyAccessor<string> targetDirectory = new MenuOptionProperty<string>(
				nameof(targetDirectory),
				new MenuPropertyStringEditorBehavior());

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
					{ pauseDownloadOnError.GetFieldName(), pauseDownloadOnError.ToString() },
					{ doDownload.GetFieldName(), doDownload.ToString() },
					{ appendPlatform.GetFieldName(), appendPlatform.ToString() },
					{ appendVersion.GetFieldName(), appendVersion.ToString() },
					{ appendDate.GetFieldName(), appendDate.ToString() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		string IMenuObject.GetInfoString() {
			return string.Join(" | ", nameof(pauseDownloadOnError), nameof(doDownload), nameof(appendPlatform), nameof(appendVersion), nameof(appendDate));
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
					pauseDownloadOnError,
					doDownload,
					appendPlatform,
					appendVersion,
					appendDate
			};
		}
	}
}
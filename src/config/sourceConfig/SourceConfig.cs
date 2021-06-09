using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.config.sourceConfig {
	[PublicAPI]
	public class SourceConfig : YamlObject, IMenuObject {
		[YamlIgnore] private IMenuPropertyAccessor<string> lastCreated;

		[YamlIgnore] private MenuOption lastCreatedOption;

		public IMenuPropertyAccessor<ESourceType> sourceType = new MenuOptionPropertyEnum<ESourceType>(
				nameof(sourceType),
				new MenuPropertyEnumSelectionBehavior<ESourceType>());

		public IMenuPropertyAccessor<OnlineSource> online = new MenuOptionProperty<OnlineSource>(
				nameof(online),
				new MenuPropertyObjectBehavior<OnlineSource>());

		public IMenuPropertyAccessor<LocalSource> local = new MenuOptionProperty<LocalSource>(
				nameof(local),
				new MenuPropertyObjectBehavior<LocalSource>());

		public void Init(IMenuPropertyAccessor<string> lastCreated, MenuOption lastCreatedOption) {
			this.lastCreated = lastCreated;
			this.lastCreatedOption = lastCreatedOption;
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
					{ nameof(sourceType), sourceType.GetValue().ToString() }
			};
		}

		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
					{ online.GetFieldName(), online.GetValue() },
					{ local.GetFieldName(), local.GetValue() }
			};
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict())
					&& YamlUtils.ChangeYamlObjects(ref lines, startLine, ref endLine, currentTabDepth, GetObjectChangeDict());
		}

		string IMenuObject.GetInfoString() {
			var result = new StringBuilder();
			result.Append(nameof(sourceType)).Append(" = ").Append(sourceType);
			if (sourceType.GetValue() == ESourceType.online) {
				result.Append(" | ").Append(online.GetValue().GetShortInfoString());
			} else if (sourceType.GetValue() == ESourceType.local) {
				result.Append(" | ").Append(local.GetValue().GetShortInfoString());
			} else if (sourceType.GetValue() == ESourceType.lastCreated) {
				result.Append(" | ").Append(Path.GetFileName(lastCreated.GetValue()));
			}

			return result.ToString();
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			List<IMenuProperty> result = new List<IMenuProperty> { sourceType };

			if (sourceType.GetValue() == ESourceType.online) {
				result.Add(online);
			} else if (sourceType.GetValue() == ESourceType.local) {
				result.Add(local);
			} else if (sourceType.GetValue() == ESourceType.lastCreated) {
				result.Add(lastCreatedOption);
			}

			return result.ToArray();
		}
	}
}
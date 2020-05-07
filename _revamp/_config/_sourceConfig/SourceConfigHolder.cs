using SoD_DiffExplorer.csutils;
using SoD_DiffExplorer.menu;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SoD_DiffExplorer._revamp._config._programConfig;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class SourceConfigHolder : YamlObject, IMenuObject
	{
		public IMenuPropertyAccessor<string> lastCreated = new MenuOptionProperty<string>(nameof(lastCreated), new MenuPropertyStringEditorBehavior());
		public IMenuPropertyAccessor<SourceConfig> sourceFrom = new MenuOptionProperty<SourceConfig>(nameof(sourceFrom), new MenuPropertyObjectBehavior<SourceConfig>());
		public IMenuPropertyAccessor<SourceConfig> sourceTo = new MenuOptionProperty<SourceConfig>(nameof(sourceTo), new MenuPropertyObjectBehavior<SourceConfig>());
		public IMenuPropertyAccessor<OnlineSourcesConfig> onlineSourcesConfig = new MenuOptionProperty<OnlineSourcesConfig>(nameof(onlineSourcesConfig), new MenuPropertyObjectBehavior<OnlineSourcesConfig>());
		public IMenuPropertyAccessor<LocalSourcesConfig> localSourcesConfig = new MenuOptionProperty<LocalSourcesConfig>(nameof(localSourcesConfig), new MenuPropertyObjectBehavior<LocalSourcesConfig>());

		public void Init(IOnlineAddressDictConfig addressDictSupplier) {
			sourceFrom.GetValue().Init(lastCreated, new MenuOption(nameof(lastCreated), MenuUtils.GetConfigureString, lastCreated.ToString, OnLastCreatedClicked));
			sourceTo.GetValue().Init(lastCreated, new MenuOption(nameof(lastCreated), MenuUtils.GetConfigureString, lastCreated.ToString, OnLastCreatedClicked));
			onlineSourcesConfig.GetValue().Init(addressDictSupplier);
		}

		public string GetLocalSourceFile(SourceConfig sourceConfig) {
			string fileName = localSourcesConfig.GetValue().targetFileName.ToString();
			if(sourceConfig.sourceType.GetValue() == ESourceType.online) {
				if(localSourcesConfig.GetValue().appendPlatform.GetValue()) {
					fileName += ("_" + sourceConfig.online.GetValue().platform);
				}
				if(localSourcesConfig.GetValue().appendVersion.GetValue()) {
					fileName += ("_" + sourceConfig.online.GetValue().version);
				}
				if(localSourcesConfig.GetValue().appendDate.GetValue()) {
					fileName += ("_" + DateTime.Now.ToString("yyyy.MM.dd"));
				}
			} else if(sourceConfig.sourceType.GetValue() == ESourceType.local) {
				if(localSourcesConfig.GetValue().appendPlatform.GetValue()) {
					fileName += ("_" + sourceConfig.local.GetValue().platform);
				}
				if(localSourcesConfig.GetValue().appendVersion.GetValue()) {
					fileName += ("_" + sourceConfig.local.GetValue().version);
				}
				if(localSourcesConfig.GetValue().appendDate.GetValue()) {
					fileName += ("_" + sourceConfig.local.GetValue().date);
				}
			} else {
				//undefined behaviour
				throw new InvalidOperationException("SourceType " + sourceConfig.sourceType.GetValue() + " not supported!");
			}
			fileName += ("." + localSourcesConfig.GetValue().targetFileExtension);

			return Path.Combine(localSourcesConfig.GetValue().baseDirectory, fileName);
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(lastCreated), lastCreated.ToString()}
			};
		}

		private BetterDict<string, YamlObject> GetObjectChangeDict() {
			return new BetterDict<string, YamlObject> {
				{sourceFrom.GetFieldName(), sourceFrom.GetValue()},
				{sourceTo.GetFieldName(), sourceTo.GetValue()}
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

		private void OnLastCreatedClicked(MenuUtils menuUtils, string header, int spacing) {
			lastCreated.SetValue(menuUtils.OpenFileSelectionMenu(localSourcesConfig.GetValue().baseDirectory, lastCreated.GetValue(), spacing));
		}

		string IMenuObject.GetInfoString() {
			StringBuilder result = new StringBuilder();
			result.Append(nameof(sourceFrom));
			result.Append(" | ").Append(nameof(sourceTo));
			result.Append(" | ").Append(nameof(onlineSourcesConfig));
			result.Append(" | ").Append(nameof(localSourcesConfig));
			return result.ToString();
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[] {
				sourceFrom,
				sourceTo,
				onlineSourcesConfig,
				localSourcesConfig
			};
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AssetsTools.NET;
using JetBrains.Annotations;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	[PublicAPI]
	public class StoredMappingValue : IMappingValue {
		public string path;
		public string outputName;

		List<string> IMappingValue.GetMapValues(string fileUrl, XDocument document, XElement targetElement) {
			return XMLUtils.FindNodeValuesAtPath(targetElement, path.Split(':')).ToList();
		}

		List<string> IMappingValue.GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField,
				AssetToolUtils assetToolUtils) {
			return AssetToolUtils.GetFieldAtPath(assetFile, targetField, path.Split(':')).Select(field => field.GetValue().AsString()).ToList();
		}

		string IMappingValue.GetOutputName() {
			return outputName;
		}
	}
}
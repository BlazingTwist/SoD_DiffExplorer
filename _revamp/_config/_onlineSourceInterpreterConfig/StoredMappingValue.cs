using AssetsTools.NET;
using SoD_DiffExplorer.csutils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class StoredMappingValue : IMappingValue
	{
		public string path = null;
		public string outputName = null;

		List<string> IMappingValue.GetMapValues(string fileUrl, XDocument document, XElement targetElement) {
			return XMLUtils.FindNodeValuesAtPath(targetElement, path.Split(':')).ToList();
		}

		List<string> IMappingValue.GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField, AssetToolUtils assetToolUtils) {
			return assetToolUtils.GetFieldAtPath(assetFile, targetField, path.Split(':')).Select(field => field.GetValue().AsString()).ToList();
		}

		string IMappingValue.GetOutputName() {
			return outputName;
		}
	}
}

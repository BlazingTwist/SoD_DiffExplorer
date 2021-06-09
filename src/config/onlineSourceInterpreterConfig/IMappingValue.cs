using System.Collections.Generic;
using System.Xml.Linq;
using AssetsTools.NET;
using SoD_DiffExplorer.csutils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	public interface IMappingValue {
		public List<string> GetMapValues(string fileUrl, XDocument document, XElement targetElement);

		public List<string> GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField,
				AssetToolUtils assetToolUtils);

		public string GetOutputName();
	}
}
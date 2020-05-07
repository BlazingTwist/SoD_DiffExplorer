using AssetsTools.NET;
using SoD_DiffExplorer.csutils;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	interface IMappingValue
	{
		public List<string> GetMapValues(string fileUrl, XDocument document, XElement targetElement);

		public List<string> GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField, AssetToolUtils assetToolUtils);

		public string GetOutputName();
	}
}

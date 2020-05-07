using AssetsTools.NET;
using SoD_DiffExplorer.csutils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SoD_DiffExplorer._revamp._config._onlineSourceInterpreterConfig
{
	class FileUrlBasedMappingValue : IMappingValue
	{
		public string fileUrlRegex = null;
		public string fileUrlReplacement = null;
		public string outputName = null;

		List<string> IMappingValue.GetMapValues(string fileUrl, XDocument document, XElement targetElement) {
			return new List<string>{
				new Regex(fileUrlRegex).Replace(fileUrl, fileUrlReplacement)
			};
		}

		List<string> IMappingValue.GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField, AssetToolUtils assetToolUtils) {
			return new List<string>{
				new Regex(fileUrlRegex).Replace(fileUrl, fileUrlReplacement)
			};
		}

		string IMappingValue.GetOutputName() {
			return outputName;
		}
	}
}

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AssetsTools.NET;
using JetBrains.Annotations;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	[PublicAPI]
	public class FileUrlBasedMappingValue : IMappingValue {
		public string fileUrlRegex;
		public string fileUrlReplacement;
		public string outputName;

		List<string> IMappingValue.GetMapValues(string fileUrl, XDocument document, XElement targetElement) {
			return new List<string> {
					new Regex(fileUrlRegex).Replace(fileUrl, fileUrlReplacement)
			};
		}

		List<string> IMappingValue.GetMapValues(string fileUrl, AssetFile assetFile, AssetTypeValueField baseField, AssetTypeValueField targetField,
				AssetToolUtils assetToolUtils) {
			return new List<string> {
					new Regex(fileUrlRegex).Replace(fileUrl, fileUrlReplacement)
			};
		}

		string IMappingValue.GetOutputName() {
			return outputName;
		}
	}
}
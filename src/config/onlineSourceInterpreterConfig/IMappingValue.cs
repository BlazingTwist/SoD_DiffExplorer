﻿using System.Collections.Generic;
using System.Xml.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {
	public interface IMappingValue {
		public List<string> GetMapValues(string fileUrl, XDocument document, XElement targetElement);

		public List<string> GetMapValues(
				string fileUrl,
				AssetsFileInstance assetFile,
				AssetTypeValueField baseField,
				AssetTypeValueField targetField,
				AssetToolUtils assetToolUtils
		);

		public string GetOutputName();
	}
}
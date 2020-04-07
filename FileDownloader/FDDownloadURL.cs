using System;
using System.Collections.Generic;
using System.Text;

namespace SoD_DiffExplorer.filedownloader
{
	class FDDownloadURL
	{
		public string baseURL = null;
		public string platform = null;
		public string version = null;
		public string baseSuffix = null;
		public string assetInfo = null;

		public string GetFullBaseUrl() {
			return baseURL + "/" + platform + "/" + version + "/" + baseSuffix + "/";
		}

		public string GetAssetInfoUrl() {
			return GetFullBaseUrl() + assetInfo;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;

namespace SoD_DiffExplorer.filedownloader
{
	class FDOutputDirectory
	{
		public string baseDirectory = null;
		public bool appendPlatform = false;
		public bool appendVersion = false;
		public bool appendDate = false;

		public string buildOutputDirectory(FDDownloadURL urlConfig, string fileName) {
			List<string> splittedPath = new List<string>();
			splittedPath.Add(baseDirectory);

			if(appendPlatform) {
				splittedPath.Add(urlConfig.platform);
			}

			if(appendVersion) {
				splittedPath.Add(urlConfig.version);
			}

			if(appendDate) {
				splittedPath.Add(DateTime.Now.ToString("yyyy.MM.dd"));
			}

			splittedPath.AddRange(fileName.Split("/"));

			return Path.Combine(splittedPath.GetRange(0, splittedPath.Count - 1).ToArray());
		}
	}
}

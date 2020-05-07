using SoD_DiffExplorer._revamp._config._programConfig;
using SoD_DiffExplorer.csutils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer._revamp._config._sourceConfig
{
	class QueriedOnlineSourceConfig : IOnlineSourcesConfig
	{
		[YamlIgnore]
		IOnlineAddressDictConfig addressDictSupplier = null;

		public string assetInfo = null;
		public string assetFileNamePath = null;
		public List<string> dataContainerRegexFilters = null;

		void IOnlineSourcesConfig.Init(IOnlineAddressDictConfig addressDictSupplier) {
			this.addressDictSupplier = addressDictSupplier;
		}

		Queue<string> IOnlineSourcesConfig.GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			Queue<string> result = new Queue<string>();
			Console.WriteLine("gathering online source addresses...");
			XDocument document = XMLUtils.LoadDocumentFromURL(GetAssetInfoUrl(urlHolder, sourceHolder));
			foreach(string fileName in XMLUtils.FindNodeValuesAtPath(document.Root, assetFileNamePath.Split(':'))) {
				if(!CustomRegex.AllMatching(fileName, dataContainerRegexFilters)) {
					continue;
				}
				KeyValuePair<string, string> addressKey = addressDictSupplier.GetOnlineAddressDict().First(kvp => fileName.StartsWith(kvp.Key));
				string actualFileName = addressKey.Value + fileName.Substring(addressKey.Key.Length);
				result.Enqueue(GetOnlineBaseUrl(urlHolder, sourceHolder) + "/" + actualFileName);
			}
			return result;
		}

		private string GetOnlineBaseUrl(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			return string.Join('/', urlHolder.GetBaseUrl(), sourceHolder.GetPlatform(), sourceHolder.GetVersion(), urlHolder.GetBaseUrlSuffix());
		}

		private string GetAssetInfoUrl(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			return string.Join('/', GetOnlineBaseUrl(urlHolder, sourceHolder), assetInfo);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.csutils;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.config.sourceConfig {
	[PublicAPI]
	public class QueriedOnlineSourceConfig : IOnlineSourcesConfig {
		[YamlIgnore] private IOnlineAddressDictConfig addressDictSupplier;
		public string assetInfo;
		public string assetFileNamePath;
		public List<string> dataContainerRegexFilters;

		void IOnlineSourcesConfig.Init(IOnlineAddressDictConfig addressDictSupplier) {
			this.addressDictSupplier = addressDictSupplier;
		}

		Queue<string> IOnlineSourcesConfig.GetDataFileURLs(IOnlineUrlHolder urlHolder, IOnlineSourceHolder sourceHolder) {
			Queue<string> result = new Queue<string>();
			Console.WriteLine("gathering online source addresses...");
			XDocument document = XMLUtils.LoadDocumentFromURL(GetAssetInfoUrl(urlHolder, sourceHolder));
			foreach (
					string actualFileName in
					from fileName in XMLUtils.FindNodeValuesAtPath(document.Root, assetFileNamePath.Split(':'))
					where CustomRegex.AllMatching(fileName, dataContainerRegexFilters)
					let addressKey = addressDictSupplier.GetOnlineAddressDict().First(kvp => fileName.StartsWith(kvp.Key))
					select addressKey.Value + fileName.Substring(addressKey.Key.Length)) {
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
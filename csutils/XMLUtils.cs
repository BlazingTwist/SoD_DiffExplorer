using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoD_DiffExplorer.csutils
{
	class XMLUtils
	{
		public static XDocument LoadDocumentFromURL(string url) {
			XDocument document;
			using(WebClient client = new WebClient()) {
				using(Stream stream = client.OpenRead(url)) {
					using(StreamReader reader = new StreamReader(stream, Encoding.UTF8)) {
						using(XmlReader xReader = XmlReader.Create(reader)) {
							document = XDocument.Load(xReader);
						}
					}
				}
			}
			return document;
		}

		public static List<XElement> FindNodesAtPath(XElement node, string[] path) {
			List<XElement> currentScope = new List<XElement>{node};
			foreach(string pathName in path) {
				List<XElement> found = new List<XElement>();
				foreach(IEnumerable<XElement> list in currentScope.Select(node => node.Elements())) {
					foreach(XElement child in list.Where(listChild => listChild.Name.LocalName == pathName)) {
						found.Add(child);
					}
				}
				currentScope = found;
			}
			return currentScope;
		}

		public static List<string> FindNodeValuesAtPath(XElement node, string[] path) {
			IEnumerable<XElement> currentScope = new List<XElement>{node};
			for(int i = 0; i < path.Length; i++) {
				string pathName = path[i];
				List<XElement> found = new List<XElement>();
				if(pathName.StartsWith('@')) {
					pathName = pathName.Substring(1);
					List<string> result = new List<string>();
					foreach(IEnumerable<XAttribute> list in currentScope.Select(node => node.Attributes())){
						result.AddRange(list.Where(attr => attr.Name.LocalName == pathName).Select(attr => attr.Value));
					}
					return result;
				}else if(i == (path.Length - 1)) {
					List<string> result = new List<string>();
					foreach(IEnumerable<XElement> list in currentScope.Select(node => node.Elements())) {
						result.AddRange(list.Where(elem => elem.Name.LocalName == pathName).Select(elem => elem.Value));
					}
					return result;
				} else {
					foreach(IEnumerable<XElement> list in currentScope.Select(node => node.Elements())) {
						foreach(XElement child in list.Where(listChild => listChild.Name.LocalName == pathName)) {
							found.Add(child);
						}
					}
				}
				currentScope = found;
			}

			return null;
		}

		public static bool IsMatchingPathConstraints(XElement node, List<string> pathConstraints) {
			return !pathConstraints.Any(path => FindNodesAtPath(node, path.Split(':')).Count == 0);
		}
	}
}

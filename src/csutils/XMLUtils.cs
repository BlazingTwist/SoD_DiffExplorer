using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoD_DiffExplorer.csutils {
	public static class XMLUtils {
		public static XDocument LoadDocumentFromURL(string url) {
			using var client = new WebClient();
			using Stream stream = client.OpenRead(url);
			using var reader = new StreamReader(stream!, Encoding.UTF8);
			using var xReader = XmlReader.Create(reader);
			return XDocument.Load(xReader);
		}

		public static List<XElement> FindNodesAtPath(XElement node, string[] path) {
			List<XElement> currentScope = new List<XElement> { node };
			foreach (string pathName in path) {
				List<XElement> found = currentScope
						.Select(node2 => node2.Elements())
						.SelectMany(list => list.Where(listChild => listChild.Name.LocalName == pathName))
						.ToList();
				currentScope = found;
			}

			return currentScope;
		}

		public static IEnumerable<string> FindNodeValuesAtPath(XElement node, string[] path) {
			IEnumerable<XElement> currentScope = new List<XElement> { node };
			for (int i = 0; i < path.Length; i++) {
				string pathName = path[i];
				if (pathName.StartsWith('@')) {
					pathName = pathName.Substring(1);
					List<string> result = new List<string>();
					foreach (IEnumerable<XAttribute> list in currentScope.Select(node2 => node2.Attributes())) {
						result.AddRange(list.Where(attr => attr.Name.LocalName == pathName).Select(attr => attr.Value));
					}

					return result;
				}

				if (i == (path.Length - 1)) {
					List<string> result = new List<string>();
					foreach (IEnumerable<XElement> list in currentScope.Select(node2 => node2.Elements())) {
						result.AddRange(list.Where(elem => elem.Name.LocalName == pathName).Select(elem => elem.Value));
					}

					return result;
				}

				List<XElement> found = currentScope
						.Select(node2 => node2.Elements())
						.SelectMany(list => list.Where(listChild => listChild.Name.LocalName == pathName))
						.ToList();

				currentScope = found;
			}

			return null;
		}

		public static bool IsMatchingPathConstraints(XElement node, IEnumerable<string> pathConstraints) {
			return pathConstraints == null || pathConstraints.All(path => path == null || FindNodesAtPath(node, path.Split(':')).Count != 0);
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using JetBrains.Annotations;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;

namespace SoD_DiffExplorer.config.onlineSourceInterpreterConfig {

	[PublicAPI]
	public class DownloaderInterpreterConfig : YamlObject, IMenuObject {

		public string configPath;
		public string fileNamePath;
		public List<string> pathConstraints;
		public List<string> fileNameRegexFilters;
		public DownloadInterpreterFilter subFilter;

		public List<string> BuildXMLContent(Queue<string> fileUrls) {
			List<string> result = new List<string>();

			foreach (string fileUrl in fileUrls) {
				Console.WriteLine("loading xml data from url: " + fileUrl);
				XDocument document = XMLUtils.LoadDocumentFromURL(fileUrl);

				if (!XMLUtils.IsMatchingPathConstraints(document.Root, pathConstraints)) {
					Console.WriteLine("skipping file, because of nonMatching pathConstraints: " + fileUrl);
					continue;
				}

				List<XElement> primaryConfigElements = XMLUtils.FindNodesAtPath(document.Root, configPath.Split(':'));
				Console.WriteLine("found " + primaryConfigElements.Count + " elements at path: " + configPath);
				foreach (XElement primaryElement in primaryConfigElements) {
					List<string> primaryMapValues = XMLUtils.FindNodeValuesAtPath(primaryElement, fileNamePath.Split(':')).ToList();
					if (primaryMapValues.Count != 1) {
						Console.WriteLine("Could not uniquely identify node at mappingPath: " + fileNamePath);
						throw new InvalidOperationException("make sure your mappingPath can uniquely identify a node when loading from xml files!");
					}

					string mapValue = primaryMapValues[0].Trim();

					if (!CustomRegex.AllMatching(mapValue, fileNameRegexFilters)) {
						continue;
					}

					ApplySubFilters(mapValue, document.Root, primaryElement).ForEach(value => {
						if (!result.Contains(value)) {
							result.Add(value);
						}
					});
				}
			}

			return result;
		}

		private List<string> ApplySubFilters(string mapValue, XElement absoluteRoot, XElement relativeRoot) {
			List<string> result = new List<string>();

			XElement targetRoot = subFilter.pathType switch {
					EOnlineInterpreterPathType.relative => relativeRoot,
					EOnlineInterpreterPathType.absolute => absoluteRoot,
					_ => throw new InvalidOperationException("pathType " + subFilter.pathType + " is not supported by DownloaderInterpreterConfig.cs")
			};

			List<XElement> basePathElements = XMLUtils.FindNodesAtPath(targetRoot, subFilter.basePath.Split(':'));
			List<string> filterValues = new List<string>();

			foreach (XElement element in basePathElements) {
				filterValues.AddRange(XMLUtils.FindNodeValuesAtPath(element, subFilter.valuePath.Split(':')).Select(value => value.Trim()));
			}

			if (subFilter.optional && (basePathElements.Count == 0 || basePathElements.Count > filterValues.Count)) {
				//this file has no filtered basePathElements
				//or this file has some filtered basePathElements without a filterValue
				result.Add(mapValue);
			}

			var fileNameModifierRegex = new Regex(subFilter.fileNameModifierRegex);
			result.AddRange(filterValues
					.Where(value => CustomRegex.AllMatching(value, subFilter.valueRegexFilters))
					.Select(value =>
							fileNameModifierRegex.Replace(mapValue, subFilter.fileNameModifierReplacement.Replace("${value}", value))
					));

			return result;
		}

		public List<string> BuildBundleContent(Queue<string> fileUrls) {
			List<string> result = new List<string>();

			foreach (string fileUrl in fileUrls) {
				Console.WriteLine("loading bundle data from url: " + fileUrl);
				using WebClient client = new WebClient();
				using MemoryStream stream = new MemoryStream(client.DownloadData(fileUrl));
				AssetToolUtils assetToolUtils = new AssetToolUtils();
				Console.WriteLine("Download done, building AssetsFileInstance...");

				foreach (AssetsFileInstance file in assetToolUtils.BuildAssetsFileInstance(stream)) {
					try {
						BuildBundleContent(ref result, fileUrl, file, assetToolUtils);
					} catch (Exception e) {
						Console.WriteLine("failed to parse bundle! blame AssetTools...");
						Console.WriteLine("Exception: " + e.Message);
						Console.WriteLine(e.StackTrace);
					}
				}

				assetToolUtils.CloseActiveStreams();
			}

			return result;
		}

		private void BuildBundleContent(ref List<string> result, string fileUrl, AssetsFileInstance file, AssetToolUtils assetToolUtils) {
			foreach (AssetFileInfo asset in file.file.GetAssetsOfType(AssetClassID.MonoBehaviour)) {
				AssetTypeValueField baseField = assetToolUtils.assetsManager.GetBaseField(file, asset);
				List<AssetTypeValueField> fields = assetToolUtils.GetFieldAtPath(file, baseField, configPath.Split(':'));
				if (fields.Count == 0) {
					continue;
				}
				if (!assetToolUtils.IsMatchingPathConstraints(file, baseField, pathConstraints)) {
					continue;
				}
				Console.WriteLine("found " + fields.Count + " matches in mono-behaviour at path: " + configPath);

				foreach (AssetTypeValueField field in fields) {
					List<AssetTypeValueField> fileNameFields = assetToolUtils.GetFieldAtPath(file, field, fileNamePath.Split(':'));
					Console.WriteLine("found " + fileNameFields.Count + " fileNameFields in targetField at path: " + fileNamePath);

					foreach (string value in fileNameFields
							.Select(fileNameField => fileNameField.Value.AsString.Trim())
							.Where(fileName => CustomRegex.AllMatching(fileName, fileNameRegexFilters))
							.SelectMany(fileName => ApplySubFilters(fileName, baseField, field, file, assetToolUtils))) {
						if (!result.Contains(value)) {
							result.Add(value);
						}
					}
				}
			}
		}

		private List<string> ApplySubFilters(
				string mapValue,
				AssetTypeValueField absoluteRoot,
				AssetTypeValueField relativeRoot,
				AssetsFileInstance file,
				AssetToolUtils assetUtils
		) {
			List<string> result = new List<string>();

			AssetTypeValueField targetRoot = subFilter.pathType switch {
					EOnlineInterpreterPathType.relative => relativeRoot,
					EOnlineInterpreterPathType.absolute => absoluteRoot,
					_ => throw new InvalidOperationException("pathType " + subFilter.pathType + " is not supported by DownloaderInterpreterConfig.cs")
			};

			List<AssetTypeValueField> basePathFields = assetUtils.GetFieldAtPath(file, targetRoot, subFilter.basePath.Split(':'));
			List<string> filterValues = new List<string>();

			foreach (List<AssetTypeValueField> pathFields in basePathFields
					.Select(field => assetUtils.GetFieldAtPath(file, field, subFilter.valuePath.Split(':')))) {
				filterValues.AddRange(pathFields.Where(pathField => pathField.Value != null).Select(pathField => pathField.Value.AsString.Trim()));
			}

			if (subFilter.optional && (basePathFields.Count == 0 || basePathFields.Count > filterValues.Count)) {
				//this file has no filtered basePathElements
				//or this file has some filtered basePathElements without a filterValue
				result.Add(mapValue);
			}

			Regex fileNameModifierRegex = new Regex(subFilter.fileNameModifierRegex);
			result.AddRange(filterValues
					.Where(value => CustomRegex.AllMatching(value, subFilter.valueRegexFilters))
					.Select(value =>
							fileNameModifierRegex.Replace(mapValue, subFilter.fileNameModifierReplacement.Replace("${value}", value))
					));

			return result;
		}

		bool YamlObject.Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return true;
		}

		string IMenuObject.GetInfoString() {
			return "no menu options, adjust the config file instead.";
		}

		IMenuProperty[] IMenuObject.GetOptions() {
			return new IMenuProperty[0];
		}

	}

}

using System;
using SoD_DiffExplorer.menuutils;
using System.IO;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace SoD_DiffExplorer.filedownloader
{
	class FileDownloader
	{
		private FDConfig config;

		public FileDownloader(FDConfig config) {
			this.config = config;
		}

		private void RunFileDownload() {
			Console.WriteLine("FileDownloader started running...");
			HtmlNodeCollection nodeCollection = ParseAssetInfo();

			Console.WriteLine("received nodeCollection, querying...");
			Queue<string> downloadQueue = new Queue<string>();
			foreach(HtmlNode node in nodeCollection) {
				string fileName = node.GetAttributeValue("N", null);
				if(fileName == null) {
					Console.WriteLine("fileName was null, not sure why. skipping...");
					continue;
				}

				if(!config.DoRegexCheck(fileName)) {
					continue;
				}

				HashSet<string> localizations = GetAvailableLocalizations(node);
				if(localizations.Count == 0) {
					downloadQueue.Enqueue(fileName);
				} else {
					foreach(string localization in localizations) {
						string localizedFileName = GetLocalizedFileName(fileName, localization);
						if(!config.DoLocaleCheck(localization)) {
							Console.WriteLine("skipping file: " + localizedFileName + " reason: failed to match localefilter");
						} else {
							downloadQueue.Enqueue(localizedFileName);
						}
					}
				}
			}

			Console.WriteLine("finished querying, beginning downloads...");
			foreach(string file in downloadQueue) {
				if(config.doDownload) {
					DownloadFile(file);
				} else {
					Console.WriteLine("would download: " + file + " but set to logging only.");
					Console.WriteLine("\ttarget directory: " + config.outputDirectory.buildOutputDirectory(config.downloadURL, file));
				}
			}

			Console.WriteLine("finished downloading! press any key to return to the FileDownloaderMenu.");
			Console.ReadKey(true);
		}

		private HtmlNodeCollection ParseAssetInfo() {
			HtmlDocument document = new HtmlDocument();
			using(WebClient client = new WebClient()) {
				string downloadURL = config.downloadURL.GetAssetInfoUrl();
				Console.WriteLine("trying to download assetInfo from " + downloadURL);
				document.Load(client.OpenRead(downloadURL));
			}
			return document.DocumentNode.SelectNodes("//a");
		}

		private HashSet<string> GetAvailableLocalizations(HtmlNode node) {
			HashSet<string> result = new HashSet<string>();
			foreach(HtmlNode childNode in node.ChildNodes) {
				string localeAttr = childNode.GetAttributeValue("L", null);
				if(localeAttr != null) {
					result.Add(localeAttr);
				}
			}
			return result;
		}

		private string GetLocalizedFileName(string fileName, string locale) {
			if(fileName.Contains(".")) {
				string[] fileSplit = fileName.Split(".");
				fileSplit[fileSplit.Length - 2] += ("." + locale);
				return string.Join(".", fileSplit);
			} else {
				return fileName + "." + locale;
			}
		}

		private void DownloadFile(string fileName) {
			string fileAddress = config.GetFileAddress(fileName);
			using(WebClient client = new WebClient()) {
				string targetDirectory = config.outputDirectory.buildOutputDirectory(config.downloadURL, fileName);
				if(!Directory.Exists(targetDirectory)) {
					Console.WriteLine("creating directory: " + targetDirectory);
					Directory.CreateDirectory(targetDirectory);
				}
				try {
					Console.WriteLine("starting to download file: " + fileName + " from address: " + fileAddress);
					string[] fileNameSplit = fileName.Split("/");
					client.DownloadFile(fileAddress, Path.Combine(targetDirectory, fileNameSplit[fileNameSplit.Length - 1]));
				} catch(WebException) {
					Console.WriteLine("failed to download: " + fileAddress);
					if(config.pauseDownloadOnError) {
						Console.WriteLine("waiting for user acknowledgement. Press any key to continue...");
						Console.ReadKey(true);
					}
				}
			}
		}

		public void OpenFileDownloaderMenu() {
			string[] options = new string[] { "Adjust Configuration", "Run Download" };
			string backText = "Back to Main Menu";
			int spacing = 1;

			int selection = 0;
			while(true) {
				selection = MenuUtils.OpenSelectionMenu(options, backText, selection, spacing);

				if(selection == 0) {
					OpenConfigMenu();
				} else if(selection == 1) {
					Console.Clear();
					RunFileDownload();
				} else if(selection == 2) {
					return;
				}
			}
		}

		private void OpenConfigMenu() {
			string backText = "Back to File Downloader";
			int spacing = 2;

			int selection = 0;
			while(true) {
				string[] options = new string[] {
					"toggle pauseDownloadOnError (" + config.pauseDownloadOnError + ")",
					"toggle doDownload (" + config.doDownload + ")",
					"change platform (" + config.downloadURL.platform + ")",
					"change version (" + config.downloadURL.version + ")",
					"toggle appendPlatform (" + config.outputDirectory.appendPlatform + ")",
					"toggle appendVersion (" + config.outputDirectory.appendVersion + ")",
					"toggle appendDate (" + config.outputDirectory.appendDate + ")",
					"adjust regexFilters (" + config.regexFilters.Count + " active filters)",
					"adjust localeFilters (" + config.localeFilters.Count + " active filters)\n",
					"save config\n"
				};

				selection = MenuUtils.OpenSelectionMenu(options, backText, selection, spacing);

				switch(selection) {
					case 0:
						config.pauseDownloadOnError = !config.pauseDownloadOnError;
						break;
					case 1:
						config.doDownload = !config.doDownload;
						break;
					case 2:
						config.downloadURL.platform = MenuUtils.OpenSimpleConfigEditor("fileDownloaderConfig.downloadURL.platform", config.downloadURL.platform);
						break;
					case 3:
						config.downloadURL.version = MenuUtils.OpenSimpleConfigEditor("fileDownloaderConfig.downloadURL.version", config.downloadURL.version);
						break;
					case 4:
						config.outputDirectory.appendPlatform = !config.outputDirectory.appendPlatform;
						break;
					case 5:
						config.outputDirectory.appendVersion = !config.outputDirectory.appendVersion;
						break;
					case 6:
						config.outputDirectory.appendDate = !config.outputDirectory.appendDate;
						break;
					case 7:
						MenuUtils.OpenStringListEditor("fileDownloaderConfig.regexFilters", ref config.regexFilters);
						break;
					case 8:
						MenuUtils.OpenStringListEditor("fileDownloaderConfig.localeFilters", ref config.localeFilters);
						break;
					case 9:
						Console.Clear();
						config.SaveConfig();
						break;
					case 10:
						return;

				}
			}
		}
	}
}

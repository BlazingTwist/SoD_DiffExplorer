using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using JetBrains.Annotations;
using SoD_DiffExplorer.config.programConfig;
using SoD_DiffExplorer.config.sourceConfig;
using SoD_DiffExplorer.menu;
using SoD_DiffExplorer.utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.ObjectFactories;

namespace SoD_DiffExplorer.subPrograms {
	[PublicAPI]
	public class MaterialExtractor : ISubProgram {
		[YamlIgnore] private MaterialExtractorConfig materialExtractor;
		[YamlIgnore] private MenuUtils menuUtils;

		public string configFilePath;
		public string programName;
		public string runText;
		public string backText;

		string ISubProgram.GetProgramName() {
			return programName;
		}

		bool ISubProgram.Init(MenuUtils menuUtils, ProgramConfig programConfig) {
			this.menuUtils = menuUtils;
			try {
				IDeserializer deserializer = new DeserializerBuilder()
						.WithTagMapping("tag:yaml.org,2002:directOnlineSourceConfig", typeof(DirectOnlineSourcesConfig))
						.WithTagMapping("tag:yaml.org,2002:queriedOnlineSourceConfig", typeof(QueriedOnlineSourceConfig))
						.WithNodeDeserializer(
								inner => new MenuObjectDeserializer(inner, new DefaultObjectFactory()),
								s => s.InsteadOf<ObjectNodeDeserializer>()
						)
						.Build();
				using (var reader = new StreamReader(configFilePath)) {
					materialExtractor = deserializer.Deserialize<MaterialExtractorConfig>(reader);
				}

				materialExtractor.Init(programConfig);
				return true;
			} catch (Exception e) {
				Console.WriteLine("Encountered an exception during parsing of the config!");
				Console.WriteLine("Exception: " + e);
				Console.ReadKey(true);
				return false;
			}
		}

		private void Run() {
			EOnlineDataType onlineDataType = materialExtractor.onlineSourcesConfig.GetValue().dataType.GetValue();
			switch (onlineDataType) {
				case EOnlineDataType.bundleFile:
					Dictionary<string, List<string>> materialToColorList = new Dictionary<string, List<string>>();
					foreach (string dataFileUrL in materialExtractor.onlineSourcesConfig.GetValue().GetDataFileURLs()) {
						if (!materialExtractor.downloadSettings.GetValue().doDownload.GetValue()) {
							Console.WriteLine("would download: " + dataFileUrL);
							continue;
						}

						List<string> pathConstraints = new List<string> {
								"m_SavedProperties:m_Colors:Array:data:first=_PrimaryColor",
								"m_SavedProperties:m_Colors:Array:data:first=_SecondaryColor",
								"m_SavedProperties:m_Colors:Array:data:first=_TertiaryColor"
						};

						try {
							Console.WriteLine("loading bundle data from url: " + dataFileUrL);
							using var client = new WebClient();
							using var stream = new MemoryStream(client.DownloadData(dataFileUrL));
							var assetToolUtils = new AssetToolUtils();
							Console.WriteLine("Download done, building AssetsFileInstance...");
							foreach (AssetFile file in assetToolUtils.BuildAssetsFileInstance(stream)) {
								foreach (AssetFileInfoEx info in file.fileInstance.table.assetFileInfo) {
									ClassDatabaseType type = AssetHelper.FindAssetClassByID(file.classDBFile, info.curFileType);
									if (type == null) {
										continue;
									}

									string typeName = type.name.GetString(file.classDBFile);
									if (typeName != "Material") {
										continue;
									}

									AssetTypeValueField baseField = AssetToolUtils.GetATI(file, info).GetBaseField();
									string materialName = AssetToolUtils.GetFieldAtPath(file, baseField, "m_Name".Split(":")).FirstOrDefault()?.GetValue()?.AsString();
									if (materialName == null) {
										continue;
									}
									
									Console.WriteLine($"\tchecking Material '{materialName}'");

									if (!AssetToolUtils.IsMatchingPathConstraints(file, baseField, pathConstraints)) {
										Console.WriteLine("\t\tMaterial did not match path constraints!");
										continue;
									}

									List<string> colors = new List<string> { "", "", "" };
									foreach (AssetTypeValueField colorDataField in AssetToolUtils.GetFieldAtPath(file, baseField, "m_SavedProperties:m_Colors:Array:data".Split(":"))) {
										string red = AssetToolUtils.GetFieldAtPath(file, colorDataField, "second:r".Split(":")).FirstOrDefault()?.GetValue()?.AsString();
										string green = AssetToolUtils.GetFieldAtPath(file, colorDataField, "second:g".Split(":")).FirstOrDefault()?.GetValue()?.AsString();
										string blue = AssetToolUtils.GetFieldAtPath(file, colorDataField, "second:b".Split(":")).FirstOrDefault()?.GetValue()?.AsString();
										string alpha = AssetToolUtils.GetFieldAtPath(file, colorDataField, "second:a".Split(":")).FirstOrDefault()?.GetValue()?.AsString();
										if (red == null || green == null || blue == null || alpha == null) {
											Console.WriteLine("\t\trgba was null!");
											continue;
										}
										string colorString = $"r={red};g={green};b={blue};a={alpha}";

										if (AssetToolUtils.IsMatchingPathConstraints(file, colorDataField, "first=_PrimaryColor")) {
											colors[0] = colorString;
										} else if (AssetToolUtils.IsMatchingPathConstraints(file, colorDataField, "first=_SecondaryColor")) {
											colors[1] = colorString;
										} else if (AssetToolUtils.IsMatchingPathConstraints(file, colorDataField, "first=_TertiaryColor")) {
											colors[2] = colorString;
										}
									}
									materialToColorList[materialName] = colors;
								}
							}
						} catch (Exception e) {
							Console.WriteLine($"\tEncountered an exception! {e}");
							if (materialExtractor.downloadSettings.GetValue().pauseDownloadOnError.GetValue()) {
								Console.WriteLine("\twaiting for user acknowledgement. Press any key to continue...");
								Console.ReadKey(true);
							}
						}
					}
					
					string targetFile = materialExtractor.GetResultFile("extraction");
					string targetDirectory = Path.GetDirectoryName(targetFile);
					if (!Directory.Exists(targetDirectory)) {
						Console.WriteLine("Had to create directory: " + targetDirectory);
						Directory.CreateDirectory(targetDirectory);
					}

					using (StreamWriter writer = new StreamWriter(targetFile, false)) {
						List<string> header = new List<string> { "MaterialName", "Primary", "Secondary", "Tertiary" };
						Console.WriteLine(string.Join("\t", header));
						writer.WriteLine(string.Join("\t", header));
						foreach ((string materialName, List<string> colorsInOrder) in materialToColorList) {
							Console.WriteLine(materialName + "\t" + string.Join("\t", colorsInOrder));
							writer.WriteLine(materialName + "\t" + string.Join("\t", colorsInOrder));
						}
					}
					
					break;
				case EOnlineDataType.xmlFile:
				default:
					throw new InvalidOperationException("OnlineDataType " + onlineDataType + " not supported by MaterialExtractor!");
			}

			Console.WriteLine("Press any key to continue.");
			Console.ReadKey(true);
		}

		private void PrintContent(AssetTypeValueField field, int tabDepth) {
			Console.WriteLine($"{new string('\t', tabDepth)}{field.GetName()} = {field.GetValue()?.AsString()}");
			if (field.GetChildrenCount() <= 0) {
				return;
			}
			foreach (AssetTypeValueField child in field.GetChildrenList()) {
				PrintContent(child, tabDepth + 1);
			}
		}

		private void SaveConfig() {
			List<string> lines = YamlUtils.GetAllConfigLines(configFilePath);
			int endLine = lines.Count - 1;
			if (!(materialExtractor as YamlObject).Save(ref lines, 0, ref endLine, 0)) {
				Console.WriteLine("failed to save config!");
				return;
			}

			Console.WriteLine("updating config values successful, writing to file...");
			using (var writer = new StreamWriter(configFilePath, false)) {
				foreach (string line in lines) {
					writer.WriteLine(line);
				}
			}

			Console.WriteLine("successfully saved config");
		}

		void ISubProgram.OpenMainMenu(int spacing) {
			string[] options = {
					"Adjust Configuration",
					"Save Config",
					runText
			};

			int selection = 0;
			while (true) {
				selection = menuUtils.OpenSelectionMenu(options, backText, programName, selection, spacing);

				switch (selection) {
					case 0:
						menuUtils.OpenObjectMenu(programName, nameof(materialExtractor), materialExtractor, 0, spacing + MenuUtils.spacerWidth);
						break;
					case 1:
						SaveConfig();
						Console.ReadKey(true);
						break;
					case 2:
						Console.Clear();
						Run();
						break;
					case 3:
						return;
				}
			}
		}
	}
}
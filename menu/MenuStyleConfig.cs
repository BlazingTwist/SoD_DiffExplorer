using System;
using System.Collections.Generic;
using System.Reflection;
using SoD_DiffExplorer.csutils;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.menu
{
	enum MenuTextStyle
	{
		split,
		connected,
		full_line
	}

	class MenuStyleConfig : YamlObject, IMenuObject
	{
		public IMenuPropertyAccessor<ConsoleColor> normalBackgroundColor =
			new MenuOptionPropertyEnum<ConsoleColor>(
				nameof(normalBackgroundColor),
				new MenuPropertyCustomBehavior<ConsoleColor>(MenuUtils.GetChangeString, OnNormalBackgroundColorClicked));
		
		public IMenuPropertyAccessor<ConsoleColor> normalTextColor =
			new MenuOptionPropertyEnum<ConsoleColor>(
				nameof(normalTextColor),
				new MenuPropertyCustomBehavior<ConsoleColor>(MenuUtils.GetChangeString, OnNormalTextColorClicked));
		
		public IMenuPropertyAccessor<ConsoleColor> selectedBackgroundColor =
			new MenuOptionPropertyEnum<ConsoleColor>(
				nameof(selectedBackgroundColor),
				new MenuPropertyEnumSelectionBehavior<ConsoleColor>());
		
		public IMenuPropertyAccessor<ConsoleColor> selectedTextColor =
			new MenuOptionPropertyEnum<ConsoleColor>(
				nameof(selectedTextColor),
				new MenuPropertyEnumSelectionBehavior<ConsoleColor>());
		
		public IMenuPropertyAccessor<MenuTextStyle> textStyle =
			new MenuOptionPropertyEnum<MenuTextStyle>(
				nameof(textStyle),
				new MenuPropertyEnumSelectionBehavior<MenuTextStyle>());

		public string GetInfoString() {
			return string.Join(" | ",
				nameof(normalBackgroundColor), normalBackgroundColor.ToString(),
				nameof(normalTextColor), normalTextColor.ToString(),
				nameof(selectedBackgroundColor), selectedBackgroundColor.ToString(),
				nameof(selectedTextColor), selectedTextColor.ToString(),
				nameof(textStyle), textStyle.ToString()
				);
		}

		private static void OnConsoleColorClicked<EnumType>(MenuUtils menuUtils, IMenuPropertyAccessor<EnumType> colorAccessor, string header, int spacing)
			where EnumType : struct, Enum {
			colorAccessor.SetValue(
				Enum.Parse<EnumType>(
					menuUtils.OpenEnumConfigEditor(
						header + "." + colorAccessor.GetFieldName(),
						colorAccessor.GetValue().ToString(),
						Enum.GetNames(typeof(EnumType)),
						spacing
					)
				)
			);
		}

		private static void OnNormalBackgroundColorClicked(MenuUtils menuUtils, IMenuPropertyAccessor<ConsoleColor> colorAccessor, string header, int spacing) {
			OnConsoleColorClicked(menuUtils, colorAccessor, header, spacing);
			Console.BackgroundColor = colorAccessor.GetValue();
		}

		private static void OnNormalTextColorClicked(MenuUtils menuUtils, IMenuPropertyAccessor<ConsoleColor> colorAccessor, string header, int spacing) {
			OnConsoleColorClicked(menuUtils, colorAccessor, header, spacing);
			Console.ForegroundColor = colorAccessor.GetValue();
		}

		public IMenuProperty[] GetOptions() {
			return new IMenuProperty[] {
				normalBackgroundColor,
				normalTextColor,
				selectedBackgroundColor,
				selectedTextColor,
				textStyle
			};
		}

		private BetterDict<string, string> GetValueChangeDict() {
			return new BetterDict<string, string> {
				{nameof(normalBackgroundColor), normalBackgroundColor.ToString()},
				{nameof(normalTextColor), normalTextColor.ToString()},
				{nameof(selectedBackgroundColor), selectedBackgroundColor.ToString()},
				{nameof(selectedTextColor), selectedTextColor.ToString()},
				{nameof(textStyle), textStyle.ToString()}
			};
		}

		public bool Save(ref List<string> lines, int startLine, ref int endLine, int currentTabDepth) {
			return YamlUtils.ChangeSimpleValues(ref lines, startLine, ref endLine, currentTabDepth, GetValueChangeDict());
		}

		/*void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer) {
			parser.Consume<MappingStart>();
			ResolveMapping(parser);
			ResolveMapping(parser);
			ResolveMapping(parser);
			ResolveMapping(parser);
			ResolveMapping(parser);
			parser.Consume<MappingEnd>();
		}

		private void ResolveMapping(IParser parser) {
			Scalar key = parser.Consume<Scalar>();

			FieldInfo[] infos = typeof(MenuStyleConfig).GetFields();
			foreach(FieldInfo info in infos) {
				if(info.Name == key.Value) {
					(info.GetValue(this) as IMenuProperty).ParseValue(parser.Consume<Scalar>().Value);
					return;
				}
			}
		}

		void IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer) {
			throw new NotImplementedException();
		}*/
	}
}

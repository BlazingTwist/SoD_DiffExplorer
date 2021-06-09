using System;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.menu {
	public class MenuObjectDeserializer : INodeDeserializer {
		private readonly INodeDeserializer objectDeserializerFallback;
		private readonly IObjectFactory objectFactory;

		public MenuObjectDeserializer(INodeDeserializer fallback, IObjectFactory objectFactory) {
			objectDeserializerFallback = fallback;
			this.objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value) {
			Console.WriteLine("called deserializer... expectedType is " + expectedType.FullName);
			if (typeof(IMenuObject).IsAssignableFrom(expectedType)) {
				Console.WriteLine("\tis what we're looking for!");
				value = objectFactory.Create(expectedType);
				Console.WriteLine("\tcould create instance!");

				FieldInfo[] relevantFields = expectedType.GetFields(
						BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
				).Where(info => !Attribute.IsDefined(info, typeof(YamlIgnoreAttribute))).ToArray();

				Console.WriteLine("\tgot relevant fields! # = " + relevantFields.Length);

				reader.Consume<MappingStart>();

				while (!reader.TryConsume(out MappingEnd _)) {
					var key = reader.Consume<Scalar>();
					Console.WriteLine("\tlooking for key = " + key.Value);

					FieldInfo targetField = relevantFields.First(info => info.Name == key.Value);
					if (targetField == null) {
						throw new YamlException("YamlFile has unknown field: " + key.Value + " \nnot found in type: " + expectedType.Name);
					}

					Console.WriteLine("\tfound matching field: " + targetField.FieldType.FullName);

					if (typeof(IMenuProperty).IsAssignableFrom(targetField.FieldType)) {
						Console.WriteLine("\treading menuProperty");
						var targetProperty = (targetField.GetValue(value) as IMenuProperty);
						targetProperty.ParseValue(nestedObjectDeserializer(reader, targetProperty.GetInnerType()));
					} else {
						targetField.SetValue(value, nestedObjectDeserializer(reader, targetField.FieldType));
					}
				}

				Console.WriteLine("done");
				return true;
			}

			return objectDeserializerFallback.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
		}
	}
}
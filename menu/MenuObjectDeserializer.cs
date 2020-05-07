using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace SoD_DiffExplorer.menu
{
	class MenuObjectDeserializer : INodeDeserializer
	{
		INodeDeserializer objectDeserializerFallback = null;
		IObjectFactory objectFactory = null;

		public MenuObjectDeserializer(INodeDeserializer fallback, IObjectFactory objectFactory) {
			this.objectDeserializerFallback = fallback;
			this.objectFactory = objectFactory;
		}

		bool INodeDeserializer.Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value) {
			Console.WriteLine("called deserializer... expectedType is " + expectedType.FullName);
			if(typeof(IMenuObject).IsAssignableFrom(expectedType)) {
				Console.WriteLine("\tis what we're looking for!");
				value = objectFactory.Create(expectedType);
				Console.WriteLine("\tcould create instance!");

				FieldInfo[] relevantFields = expectedType.GetFields(
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
					).Where(info => !Attribute.IsDefined(info, typeof(YamlIgnoreAttribute))).ToArray();

				Console.WriteLine("\tgot relevant fields! # = " + relevantFields.Length);

				reader.Consume<MappingStart>();

				while(!reader.TryConsume<MappingEnd>(out var _)) {
					Scalar key = reader.Consume<Scalar>();
					Console.WriteLine("\tlooking for key = " + key.Value);

					FieldInfo targetField = relevantFields.Where(info => info.Name == key.Value).First();
					if(targetField == null) {
						throw new YamlException("YamlFile has unknown field: " + key.Value + " \nnot found in type: " + expectedType.Name);
					}

					Console.WriteLine("\tfound matching field: " + targetField.FieldType.FullName);

					if(typeof(IMenuProperty).IsAssignableFrom(targetField.FieldType)) {
						Console.WriteLine("\treading menuProperty");
						IMenuProperty targetProperty = (targetField.GetValue(value) as IMenuProperty);
						targetProperty.ParseValue(nestedObjectDeserializer(reader, targetProperty.GetInnerType()));
					} else {
						targetField.SetValue(value, nestedObjectDeserializer(reader, targetField.FieldType));
					}
				}
				Console.WriteLine("done");
				return true;
			} else {
				return objectDeserializerFallback.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
			}
		}
	}
}

using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace FubuCore.Binding
{
	public class ArrayTypeConverter : IConverterFamily
	{
		public bool Matches(PropertyInfo property)
		{
			var type = property.PropertyType;
			return type.IsArray && type.HasElementType &&
				TypeDescriptor.GetConverter(type.GetElementType())
					.CanConvertFrom(typeof (string));
		}

		public ValueConverter Build(IValueConverterRegistry registry, PropertyInfo property)
		{
			var type = property.PropertyType.GetElementType();
			var typeConverter = TypeDescriptor.GetConverter(type);
			return context =>
			{
				var dataArray = context.PropertyValue.ToString().Split(',')
					.Select(s => typeConverter.ConvertFrom(s)).ToArray();
				var array = Array.CreateInstance(type, dataArray.Length);
				Array.Copy(dataArray, array, dataArray.Length);
				return array;
			};
		}
	}
}
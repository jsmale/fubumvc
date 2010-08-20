using System;
using System.Linq;
using System.Reflection;
using FubuCore.Binding;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuCore.Testing.Binding
{
	[TestFixture]
	public class ArrayTypeConverterTester
	{
		private ValueConverterRegistry _registry;
		private ArrayTypeConverter _arrayTypeConverter;
		private PropertyInfo _property;
		private IBindingContext _context;
		private string _propertyValue;

		private class PropertyHolder
		{
			public DateTime[] Dates { get; set; }
			public int[] Ints { get; set; }
		}

		[SetUp]
		public void SetUp()
		{
			_arrayTypeConverter = new ArrayTypeConverter();
			_registry = new ValueConverterRegistry(new IConverterFamily[] { _arrayTypeConverter });
			_context = MockRepository.GenerateMock<IBindingContext>();
		}

		[Test]
		public void should_match_property()
		{
			_property = typeof(PropertyHolder).GetProperty("Dates");

			_arrayTypeConverter.Matches(_property).ShouldBeTrue();
		}

		[Test]
		public void should_convert_to_date_array()
		{
			_property = typeof(PropertyHolder).GetProperty("Dates");
			_propertyValue = "1/1/1950,2/6/2010";
			_context.Expect(c => c.PropertyValue).Return(_propertyValue);

			var values = (DateTime[])_arrayTypeConverter.Build(_registry, _property)(_context);
			values.ShouldContain(new DateTime(1950, 1, 1));
			values.ShouldContain(new DateTime(2010, 2, 6));
		}

		[Test]
		public void should_convert_to_int_array()
		{
			_property = typeof(PropertyHolder).GetProperty("Ints");
			_propertyValue = "1,3,42";
			_context.Expect(c => c.PropertyValue).Return(_propertyValue);

			var values = (int[])_arrayTypeConverter.Build(_registry, _property)(_context);
			values.ShouldContain(1);
			values.ShouldContain(3);
			values.ShouldContain(42);
		}
	}
}
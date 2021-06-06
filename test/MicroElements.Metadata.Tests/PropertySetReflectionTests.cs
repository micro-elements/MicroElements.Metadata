using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertySetReflectionTests
    {
        [Fact]
        public void PropertySetEvaluatorTest()
        {
            PropertySetEvaluator.GetPropertySet(typeof(ConcretePropertySet)).GetProperties().Should().NotBeNullOrEmpty();
            PropertySetEvaluator.GetPropertySet(typeof(SomeMeta), property: "PropertySet").GetProperties().Should().NotBeNullOrEmpty();
            PropertySetEvaluator.GetPropertySet(typeof(SomeMeta), method: "GetProperties").GetProperties().Should().NotBeNullOrEmpty();
        }
    }

    public static class SomeMeta
    {
        public static readonly IProperty<int> IntProperty = new Property<int>("IntProperty");
        public static readonly IProperty<string> StringProperty = new Property<string>("StringProperty");

        public static IPropertySet PropertySet { get; } = new ConcretePropertySet();

        public static IEnumerable<IProperty> GetProperties()
        {
            yield return SomeMeta.IntProperty;
            yield return SomeMeta.StringProperty;
        }
    }

    public class ConcretePropertySet : IPropertySet
    {
        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties()
        {
            yield return SomeMeta.IntProperty;
            yield return SomeMeta.StringProperty;
        }
    }
}

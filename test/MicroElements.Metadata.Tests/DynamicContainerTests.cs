using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class DynamicContainerTests
    {
        [Fact]
        public void DynamicContainer()
        {
            var propertyContainer = new MutablePropertyContainer();
            propertyContainer.SetValue("PropertyA", "ValueA");
            propertyContainer.SetValue(new Property<int>("PropertyB"), 42);

            dynamic dynamicContainer = propertyContainer.AsDynamic();

            object valueA = dynamicContainer.PropertyA;
            valueA.Should().Be("ValueA");

            object valueB = dynamicContainer.PropertyB;
            valueB.Should().Be(42);
        }

        [Fact]
        public void not_defined_and_not_found_property_value_should_be_null()
        {
            var propertyContainer = new MutablePropertyContainer();
            propertyContainer.Add(new PropertyValue<int>(new Property<int>("Defined"), 0, ValueSource.DefaultValue));
            propertyContainer.Add(new PropertyValue<int>(new Property<int>("NotDefined"), 0, ValueSource.NotDefined));

            dynamic dynamicContainer = propertyContainer.AsDynamic();

            object defined = dynamicContainer.Defined;
            defined.Should().Be(0);

            object notDefined = dynamicContainer.NotDefined;
            notDefined.Should().BeNull();

            object notFoundProperty = dynamicContainer.NotFoundProperty;
            notFoundProperty.Should().BeNull();
        }
    }
}

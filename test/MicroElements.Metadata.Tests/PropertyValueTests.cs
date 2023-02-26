using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyValueTests
    {
        [Fact]
        public void not_defined_property_value_should_return_null()
        {
            IPropertyValue<int> intDefined = new PropertyValue<int>(new Property<int>("IntDefined"), 1);
            intDefined.ValueUntyped.Should().Be(1);

            IPropertyValue<int> intDefined0 = new PropertyValue<int>(new Property<int>("IntDefined"), 0);
            intDefined0.ValueUntyped.Should().Be(0);

            IPropertyValue<int> intNotDefined = new PropertyValue<int>(new Property<int>("IntNotDefined"), 0, ValueSource.NotDefined);
            intNotDefined.ValueUntyped.Should().Be(null);
        }
    }
}

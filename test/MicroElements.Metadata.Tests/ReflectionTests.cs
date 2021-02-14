using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class ReflectionTests
    {
        [Fact]
        public void CreatePropertyValue()
        {
            var expression = PropertyValueFactory.NewPropertyValue(typeof(string));
            var factory = expression.Compile();

            IPropertyValue propertyValue = factory.Invoke(new Property<string>("Name"), "Alex", ValueSource.Defined);
            propertyValue.Should().NotBeNull();
        }

        [Fact]
        public void PropertyNullabilityType()
        {
            Property<string?> propertyA = new Property<string?>("A");
            Property<string> propertyB = new Property<string>("B");

            propertyA.Type.Should().Be(typeof(string));
            propertyB.Type.Should().Be(typeof(string));

            string? nullString = null;

            new PropertyValue<string?>(propertyA, nullString);
            new PropertyValue<string>(propertyB, nullString);

            new MutablePropertyContainer().SetValue(propertyA, nullString);
            new MutablePropertyContainer().SetValue(propertyB, nullString);
        }
    }
}

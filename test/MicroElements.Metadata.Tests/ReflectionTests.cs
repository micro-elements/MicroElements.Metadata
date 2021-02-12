using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class ReflectionTests
    {
        [Fact]
        public void CreatePropertyValue()
        {
            var expression = PropertyValueFactory.NewPropertyValue<string>();
            var factory = expression.Compile();

            IPropertyValue propertyValue = factory.Invoke(new Property<string>("Name"), "Alex", ValueSource.Defined);
            propertyValue.Should().NotBeNull();
        }
    }
}

using System;
using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests.examples
{
    public class PropertyContainerUsage
    {
        public class EntityMeta
        {
            public static readonly IProperty<DateTime> CreatedAt = new Property<DateTime>("CreatedAt");
            public static readonly IProperty<string> Description = new Property<string>("Description");
        }

        [Fact]
        public void simple_set_and_get_value()
        {
            IPropertyContainer propertyContainer = new MutablePropertyContainer()
                .WithValue(EntityMeta.CreatedAt, DateTime.Today)
                .WithValue(EntityMeta.Description, "description");

            propertyContainer.GetValue(EntityMeta.CreatedAt).Should().Be(DateTime.Today);
            propertyContainer.GetValue(EntityMeta.Description).Should().Be("description");
        }

        [Fact]
        public void get_property_value()
        {
            IPropertyContainer propertyContainer = new MutablePropertyContainer()
                .WithValue(EntityMeta.CreatedAt, DateTime.Today)
                .WithValue(EntityMeta.Description, "description");

            IPropertyValue<string>? propertyValue = propertyContainer.GetPropertyValue(EntityMeta.Description);
            propertyValue.Should().NotBeNull();
            propertyValue.Property.Should().BeSameAs(EntityMeta.Description);
            propertyValue.Value.Should().Be("description");
            propertyValue.Source.Should().Be(ValueSource.Defined);
        }
    }
}

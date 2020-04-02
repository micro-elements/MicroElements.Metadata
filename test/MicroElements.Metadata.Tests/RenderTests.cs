using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class RenderTests
    {
        [Fact]
        public void RenderDouble()
        {
            var propertyDouble = new Property<double>("propertyDouble");
            var propertyNullableDouble = new Property<double?>("propertyNullableDouble");

            var propertyRendererDouble = new PropertyRenderer<double>(propertyDouble);
            var propertyRendererNullableDouble = new PropertyRenderer<double?>(propertyNullableDouble);

            var propertyContainer = new MutablePropertyContainer()
                .WithValue(propertyDouble, 10.1)
                .WithValue(propertyNullableDouble, 10.1);

            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            string render1 = propertyRendererDouble.Render(propertyContainer);
            string render2 = propertyRendererNullableDouble.Render(propertyContainer);
            render1.Should().Be(render2);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-Ru");
            string render3 = propertyRendererDouble.Render(propertyContainer);
            string render4 = propertyRendererNullableDouble.Render(propertyContainer);
            render3.Should().Be(render4);

            render1.Should().Be(render3);

            propertyContainer = new MutablePropertyContainer()
                .WithValue(propertyNullableDouble, null);
            propertyRendererNullableDouble.Render(propertyContainer).Should().Be(null);
        }
    }
}

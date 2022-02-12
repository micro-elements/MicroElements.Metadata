﻿using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyValueTests
    {
        [Fact]
        public void not_defined_property_value_should_return_null()
        {
            new PropertyValue<int>(new Property<int>("IntDefined"), 1).ValueUntyped.Should().Be(1);
            new PropertyValue<int>(new Property<int>("IntDefined"), 0).ValueUntyped.Should().Be(0);
            new PropertyValue<int>(new Property<int>("IntNotDefined"), 0, ValueSource.NotDefined).ValueUntyped.Should().Be(null);
        }
    }
}

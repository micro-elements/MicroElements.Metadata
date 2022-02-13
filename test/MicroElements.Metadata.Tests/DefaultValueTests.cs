using System;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class DefaultValueTests
    {
        [Fact]
        public void default_values_should_be_correct()
        {
            DefaultValue<object>.Default.Value.Should().Be(null);
            DefaultValue<string>.Default.Value.Should().Be(null);
            DefaultValue<int>.Default.Value.Should().Be(0);
            DefaultValue<int?>.Default.Value.Should().Be(null);
            DefaultValue<DateTime>.Default.Value.Should().Be(default(DateTime));
        }

        [Fact]
        public void untyped_default_values_should_be_the_same_as_typed()
        {
            DefaultValue<object>.Default.Should().BeSameAs(DefaultValue.GetDefaultForType(typeof(object)));
            DefaultValue<string>.Default.Should().BeSameAs(DefaultValue.GetDefaultForType(typeof(string)));
            DefaultValue<int>.Default.Should().BeSameAs(DefaultValue.GetDefaultForType(typeof(int)));
            DefaultValue<int?>.Default.Should().BeSameAs(DefaultValue.GetDefaultForType(typeof(int?)));
            DefaultValue<DateTime>.Default.Should().BeSameAs(DefaultValue.GetDefaultForType(typeof(DateTime)));
        }

        [Fact]
        public void untyped_default_values_should_be_the_same_as_typed2()
        {
            DefaultValue<object>.Default.Should().BeSameAs(DefaultValue.GetOrCreateDefaultValue(typeof(object), null));
            DefaultValue<string>.Default.Should().BeSameAs(DefaultValue.GetOrCreateDefaultValue(typeof(string), null));
            DefaultValue<int>.Default.Should().BeSameAs(DefaultValue.GetOrCreateDefaultValue(typeof(int), 0));
            DefaultValue<int?>.Default.Should().BeSameAs(DefaultValue.GetOrCreateDefaultValue(typeof(int?), null));
            DefaultValue<DateTime>.Default.Should().BeSameAs(DefaultValue.GetOrCreateDefaultValue(typeof(DateTime), default(DateTime)));
        }

        [Fact]
        public void provided_default_values_should_be_correct()
        {
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(object), 1))).Should().NotThrow();
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(object), "a"))).Should().NotThrow();

            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(object), "a"))).Should().NotThrow();
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(object), null))).Should().NotThrow();
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(string), 1))).Should().Throw<ArgumentException>()
                .WithMessage("Value '1' can not be set as default value for type 'System.String'");

            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(int), 1))).Should().NotThrow();
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(int), null))).Should().Throw<ArgumentException>()
                .WithMessage("Value 'null' can not be set as default value for type 'System.Int32'");

            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(int?), 1))).Should().NotThrow();
            ((Action)(() => DefaultValue.GetOrCreateDefaultValue(typeof(int?), null))).Should().NotThrow();
        }

        [Fact]
        public void default_values_variance()
        {
            IDefaultValue<string> defaultValue = new DefaultValue<string>("42");

            // not variance
            typeof(DefaultValue<string>).Should().NotBeAssignableTo(typeof(IDefaultValue<object>));

            // can assign to base interface
            IDefaultValue defaultValueUntyped = defaultValue;
            defaultValueUntyped.Value.Should().Be("42");
        }
    }
}

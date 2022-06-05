using System;
using FluentAssertions;
using MicroElements.Metadata.ComponentModel;
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
        public void default_values_covariance()
        {
            IDefaultValue<string> defaultValue = new DefaultValue<string>("42");

            IDefaultValue<object> covariantDefaultValue = defaultValue;
            covariantDefaultValue.Value.Should().Be("42");

            IDefaultValue defaultValueUntyped = covariantDefaultValue;
            defaultValueUntyped.Value.Should().Be("42");

            defaultValueUntyped.Should().BeSameAs(defaultValue);
        }

        [Fact]
        public void DefaultValueCovariance2()
        {
            Property<int> property = new Property<int>("Int42")
                .WithDefaultValue(42);

            IDefaultValue<int> defValueInt = property.GetComponent<IDefaultValue<int>>();
            IDefaultValue defValueBase = property.GetComponent<IDefaultValue>();
            defValueBase.Should().BeSameAs(defValueInt);

            IDefaultValue defaultValue = property.GetDefaultValueMetadata()!;
            defaultValue.Value.Should().Be(42);

            IProperty baseProp = property;
            defaultValue = baseProp.GetDefaultValueMetadata()!;
            defaultValue.Value.Should().Be(42);
        }

        [Fact]
        public void DefaultValueAsExternalMetadata()
        {
            Property<int> property = new Property<int>("Int42")
                .WithDefaultValue(42);

            property.DefaultValue.Should().NotBeNull();
            property.Component<IDefaultValue>().Should().NotBeNull();
            property.Component<IDefaultValue>().Value.Should().Be(42);
            property.GetComponent<IDefaultValue>()!.Value.Should().Be(42);

            Property<int> property2 = new Property<int>("Int42");
            property2.SetDefaultValueMetadata(new DefaultValue<int>(42));

            property2.DefaultValue.Should().BeNull();
            property2.Component<IDefaultValue>().Should().BeNull();
            property2.GetComponent<IDefaultValue>()!.Value.Should().Be(42);
        }
    }
}

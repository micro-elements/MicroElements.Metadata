using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyTests
    {
        [Fact]
        public void empty_property_should_be_empty()
        {
            Property<string>.Empty.ShouldBeEmpty();
            Property<int>.Empty.ShouldBeEmpty();
        }

        [Fact]
        public void property_creation()
        {
            IProperty<int> property = new Property<int>("test");

            property.Name.Should().Be("test");
            property.Type.Should().Be(typeof(int));

            property.ShouldBeEmpty();
        }

        [Fact]
        public void property_with_name_should_change_name()
        {
            var test1 = new Property<int>("test1");
            test1.Name.Should().Be("test1");
            test1.ShouldBeEmpty();

            var test2 = test1.WithName("test2");
            test2.Name.Should().Be("test2");
            test2.ShouldBeEmpty();

            var test3 = test1.WithNameUntyped("test3");
            test3.Name.Should().Be("test3");
            test3.Type.Should().Be(typeof(int));
            ((IProperty<int>)test3).ShouldBeEmpty();

            var test4 = CreateFilledProperty().WithNameUntyped("test4");
            ((IProperty<int>)test4).ShouldBeFilled(name: "test4");

            var test5 = CreateFilledProperty().WithAliasUntyped("alias5");
            ((IProperty<int>)test5).ShouldBeFilled(alias: "alias5");
        }

        [Fact]
        public void property_with_chaining()
        {
            Property<int> property = CreateFilledProperty();
            property.ShouldBeFilled();
        }

        [Fact]
        public void property_factory()
        {
            PropertyFactory propertyFactory = new PropertyFactory();
            IProperty property = propertyFactory.Create<int>("Test");
            property.Type.Should().Be(typeof(int));
            property.Name.Should().Be("Test");

            property = propertyFactory.Create(typeof(int), "Test");
            property.Type.Should().Be(typeof(int));
            property.Name.Should().Be("Test");
        }

        [Fact]
        public void property_factory_cached()
        {
            IPropertyFactory notCached = new PropertyFactory();
            notCached.Create<string>("Test").Should().NotBeSameAs(notCached.Create<string>("Test"));

            IPropertyFactory cached = new PropertyFactory().Cached();
            cached.Create<string>("Test").Should().BeSameAs(cached.Create<string>("Test"));
        }

        [Fact]
        public void PropertyValueFactory_CreateUntyped()
        {
            var factory = new PropertyValueFactory();

            IPropertyValue propertyValue = factory.CreateUntyped(new Property<int>("Age"), 10, ValueSource.Defined);
            propertyValue.Should().NotBeNull();
            propertyValue.PropertyUntyped.Type.Should().Be(typeof(int));
            propertyValue.PropertyUntyped.Name.Should().Be("Age");
            propertyValue.ValueUntyped.Should().Be(10);
        }

        private static Property<int> CreateFilledProperty() => PropertyTestsExtensions.CreateFilledProperty();


        [Fact]
        public void property_1()
        {
            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValueMetadata().Should().NotBeNull();

            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValueMetadata()!.Value.Should().Be(42);

            new Property<int>("int_default_42")
                .With(new DefaultValue<int>(42))
                .GetDefaultValue().Should().Be(42);

            new Property<int>("int_no_default")
                .GetDefaultValueMetadata().Should().BeNull();

            new Property<int>("int_no_default")
                .GetDefaultValue().Should().Be(0);

            new Property<int>("int_no_default")
                .GetDefaultValue(defaultValue: -1).Should().Be(-1);
        }
    }

    internal static class PropertyTestsExtensions
    {
        public static void ShouldBeEmpty<T>(this IProperty<T> property)
        {
            property.Description.Should().BeNull();
            property.Alias.Should().BeNull();
            property.Examples.Should().NotBeNull().And.BeEmpty();
            property.DefaultValue.Should().BeNull();
            property.Calculator.Should().BeNull();
        }

        public static Property<int> CreateFilledProperty()
        {
            var property = Property
                .Empty<int>()
                .With(name: "test")
                .With(alias: "alias")
                .With(description: "description")
                .With(defaultValue: new DefaultValueLazy<int>(() => 1))
                .With(examples: new[] { 0, 1 })
                .With(calculator: new PropertyCalculator<int>(container => 2));
            return property;
        }

        public static void ShouldBeFilled(
            this IProperty<int> property, 
            string? name = "test",
            string? alias = "alias")
        {
            property.Name.Should().Be(name);
            property.Alias.Should().Be(alias);
            property.Description.Should().Be("description");
            property.DefaultValue.Value.Should().Be(1);
            property.Examples.Should().BeEquivalentTo(new[] {0, 1});
            property.Calculator.Should().NotBeNull();
        }
    }
}

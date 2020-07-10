using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyMapTests
    {
        public static IProperty<string> Name = new Property<string>("Name");

        [Fact]
        public void map_filled_property()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex");
            container.GetValue(Name.Map(name => $"Name {name}")).Should().Be("Name Alex");
        }

        [Fact]
        public void map_absent_property()
        {
            var container = new MutablePropertyContainer();
            container.GetValue(Name.Map(name => $"Name {name}")).Should().Be(null);
        }

        [Fact]
        public void map_property_with_null_value()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, null);
            container.GetValue(Name.Map(name => $"Name {name}")).Should().Be(null);
        }

        [Fact]
        public void map_property_with_null_value_allowed()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, null);
            container.GetValue(Name.Map(name => $"Name {name ?? "undefined"}", allowMapNull: true)).Should().Be("Name undefined");
        }

        [Fact]
        public void map_filled_property_with_chaining()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex");
            container
                .GetValue(Name
                    .Map(name => $"Name {name}")
                    .Map(text => text.Length))
                .Should().Be(9);
        }

        [Fact]
        public void map_absent_property_with_chaining_and_search_options()
        {
            var container = new MutablePropertyContainer();
            SearchOptions searchOptions = SearchOptions.ExistingOnly.CalculateValue();
            container
                .GetPropertyValue(Name
                    .Map(name => $"Name {name}", searchOptions: searchOptions)
                    .Map(text => text.Length), searchOptions)
                .Should().Be(null);
        }

        [Fact]
        public void map_absent_property_with_chaining()
        {
            var container = new MutablePropertyContainer();
            var propertyValue = container
                .GetPropertyValue(Name
                    .Map(name => $"Name {name}")
                    .Map(text => text.Length));
            propertyValue.Should().NotBeNull();
            propertyValue.Property.Name.Should().Be(Name.Name);
            propertyValue.Value.Should().Be(default(int));
            propertyValue.Source.Should().Be(ValueSource.NotDefined);
        }

        [Fact]
        public void map_property_nullable_struct()
        {
            var nullableInt = new Property<int?>("nullableInt");
            var notNullableInt = new Property<int>("notNullableInt");

            test(new MutablePropertyContainer()
                .WithValue(nullableInt, null));

            test(new MutablePropertyContainer());

            void test(IPropertyContainer propertyContainer)
            {
                propertyContainer.GetValue(nullableInt.Map(value => value.Value, allowMapNull: false)).Should().Be(0);

                propertyContainer.GetValue(nullableInt.DeNullify()).Should().Be(0);

                propertyContainer.GetValue(notNullableInt).Should().Be(0);
                propertyContainer.GetValue(notNullableInt.Nullify()).Should().Be(null);

                propertyContainer.GetValue(nullableInt.Map(value => value ?? 42, allowMapNull: true)).Should().Be(42);

                var propertyValue = propertyContainer.GetPropertyValue(nullableInt.Map(pv =>
                {
                    return pv.Value.HasValue
                        ? (pv.Value.Value, ValueSource.Calculated)
                        : (0, ValueSource.NotDefined);
                }, allowMapNull: true));

                propertyValue.Source.Should().Be(ValueSource.NotDefined);
            }
        }
    }
}

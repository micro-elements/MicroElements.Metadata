using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class PropertyMapTests
    {
        public static IProperty<string> Name = new Property<string>("Name");

        [Fact]
        public void do_not_calculate_if_value_was_provided()
        {
            IProperty<string> mappedName = Name.Map(name => $"Calculated {name}");

            // Set value. It will not be calculated
            var container = new MutablePropertyContainer()
                .WithValue(mappedName, "Alex");

            container.GetValue(mappedName).Should().Be("Alex");
        }

        [Fact]
        public void calculate_value_if_base_property_value_provided()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex");

            container.GetValue(Name.Map(name => $"Calculated {name}")).Should().Be("Calculated Alex");
        }

        [Fact]
        public void calculated_value_should_be_null_if_base_property_was_not_provided()
        {
            var container = new MutablePropertyContainer();

            container.GetValue(Name.Map(name => $"Calculated {name}")).Should().Be(null);
        }

        [Fact]
        public void calculated_value_should_be_null_if_base_property_was_set_to_null()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, null);

            container.GetValue(Name.Map(name => $"Calculated {name}")).Should().Be(null);
        }

        [Fact]
        public void map_property_with_null_value_allowed_if_base_property_is_undefined()
        {
            var container = new MutablePropertyContainer();

            container.GetValue(Name.Map(name => $"Name {name ?? "undefined"}", allowMapNull: true, allowMapUndefined: true)).Should().Be("Name undefined");
        }

        [Fact]
        public void map_property_with_null_value_allowed_if_base_property_was_set_to_null()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, null);

            container.GetValue(Name.Map(name => $"Name {name ?? "undefined"}", allowMapNull: true)).Should().Be("Name undefined");
        }

        [Fact]
        public void map_filled_property_with_chaining()
        {
            new MutablePropertyContainer()
                .WithValue(Name, "Alex")
                .GetValue(Name
                    .Map(name => $"Name {name}")
                    .Map(text => text.Length))
                .Should().Be(9);
        }

        [Fact]
        public void map_absent_property_with_chaining_and_search_options()
        {
            SearchOptions searchOptions = SearchOptions.ExistingOnly.CalculateValue();
            new MutablePropertyContainer()
                .GetPropertyValue(Name
                    .Map(name => $"Name {name}")
                    .Map(text => text.Length), searchOptions)
                .Should().Be(null);
        }

        [Fact]
        public void map_absent_property_with_chaining()
        {
            var propertyValue = new MutablePropertyContainer()
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

                propertyContainer.GetValue(nullableInt.Map(value => value ?? 42, allowMapNull: true, allowMapUndefined: true)).Should().Be(42);

                var propertyValue = propertyContainer.GetPropertyValue(nullableInt.Map(pv =>
                {
                    return pv.Value.HasValue
                        ? (pv.Value.Value, ValueSource.Calculated)
                        : (0, ValueSource.NotDefined);
                }));

                propertyValue.Source.Should().Be(ValueSource.NotDefined);
            }
        }

        [Fact]
        public void TestComplexProperty()
        {
            IProperty<double?> nullableDouble = new Property<double?>("nullableDouble");
            IProperty<double> deNullify = nullableDouble.DeNullify();
            var property = deNullify.UseDefaultForUndefined();
            
            var propertyContainer = new PropertyContainer();

            var propertyValue = propertyContainer.GetPropertyValue(property);
            propertyValue.HasValue().Should().BeTrue();
            propertyValue.Value.Should().Be(0);
        }
    }
}

using FluentAssertions;
using MicroElements.Metadata.Schema;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class SearchTests
    {
        [Fact]
        public void search_options_default()
        {
            static void EnsureDefaultValues(SearchOptions searchOptions)
            {
                searchOptions.SearchProperty.Should().BeNull();
                searchOptions.PropertyComparer.Should().NotBeNull();
                searchOptions.SearchInParent.Should().BeTrue();
                searchOptions.CalculateValue.Should().BeTrue();
                searchOptions.UseDefaultValue.Should().BeTrue();
                searchOptions.ReturnNotDefined.Should().BeTrue();
            }

            EnsureDefaultValues(default(SearchOptions));
            EnsureDefaultValues(SearchOptions.Default);
        }

        [Fact]
        public void hierarchy()
        {
            IPropertyContainer container2 = new MutablePropertyContainer()
                .WithValue("Name", "Container2");

            IPropertyContainer container1 = new MutablePropertyContainer(parentPropertySource: container2)
                .WithValue("Name", "Container1");

            IPropertyContainer container3 = new MutablePropertyContainer()
                .WithValue("Name", "Container3");

            var hierarchicalContainer = PropertyContainer.CreateHierarchicalContainer(container1, container3);
            hierarchicalContainer.ToString().Should().Be("[Name: Container1] -> [Name: Container2] -> [Name: Container3]");
        }

        [Fact]
        public void match()
        {
            IProperty<string> name = new Property<string>("Name");
            IProperty<int> age = new Property<int>("Age");
            IProperty<string> name2 = new Property<string>("Name2");
            IProperty<int> age2 = new Property<int>("Age2");

            var existingOnly = SearchOptions.ExistingOnly;
            IPropertyContainer container = new MutablePropertyContainer(searchOptions: existingOnly)
                .WithValue(name, "Alex")
                .WithValue(age, 42);

            container.MatchValue(name, s => s.ToUpper()).Should().Be("ALEX");
            container.MatchValue(name2, s => s.ToUpper(), () => "NONE").Should().Be("NONE");

        }
    }
}

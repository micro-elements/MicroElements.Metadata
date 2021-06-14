using FluentAssertions;
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

            var hierarchicalContainer = new HierarchicalContainer(container1, container3);
            hierarchicalContainer.ToString().Should().Be("[Name: Container1] -> [Name: Container2] -> [Name: Container3]");
        }
    }
}

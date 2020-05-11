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
    }
}

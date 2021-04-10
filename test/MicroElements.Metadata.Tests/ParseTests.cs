using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("text", true)]
        [InlineData("", true)]
        [InlineData(null, false)]
        public void ParseAndMap(string? value, bool shouldBeSuccess)
        {
            IStringProvider stringProvider = new InterningStringProvider();

            value
                .ParseNotNull()
                .Map(value => stringProvider.GetString(value))
                .Should(beSuccess: shouldBeSuccess);

            value
                .Parse()
                .MapNotNull()
                .Map(value => stringProvider.GetString(value))
                .Should(beSuccess: shouldBeSuccess);

            value
                .Parse()
                .MapNotNull(value => stringProvider.GetString(value))
                .Should(beSuccess: shouldBeSuccess);
        }
    }

    public static class ParseResultAssertions
    {
        public static ParseResult<T> Should<T>(this ParseResult<T> parseResult, bool beSuccess = true)
        {
            parseResult.IsSuccess.Should().Be(beSuccess);

            return parseResult;
        }
    }
}

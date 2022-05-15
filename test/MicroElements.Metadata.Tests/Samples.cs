using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public static class Meta
    {
        public static IProperty<string> StringSample = 
            Property
                .Create<string>("StringSample")
                .WithAlias("SampleAlias");

        public static IProperty<string> ExchangeCode =
            new Property<string>("ExchangeCode")
                .WithDescription("Short exchange code")
                .WithExamples("MOEX", "SPBEX", "LSE", "NASDAQ", "NYSE");
    }

    public class MetadataUsage
    { 
        [Fact]
        public void TypedPropertyCreation()
        {
            IProperty<string> property = Property
                .Create<string>("StringSample")
                .WithAlias("SampleAlias");

            property.Type.Should().Be(typeof(string));
            property.Name.Should().Be("StringSample");
            property.Alias.Should().Be("SampleAlias");
        }

        [Fact]
        public void UntypedPropertyCreation()
        {
            IProperty property = Property
                .Create(typeof(string), "StringSample")
                .WithAliasUntyped("SampleAlias");

            property.Type.Should().Be(typeof(string));
            property.Name.Should().Be("StringSample");
            property.Alias.Should().Be("SampleAlias");
        }
    }
}

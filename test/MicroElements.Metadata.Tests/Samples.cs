using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public static class Meta
    {
        public static IProperty<string> StringSample = 
            Property
                .Create<string>("StringSample")
                .SetAlias("SampleAlias");

        public static IProperty<string> ExchangeCode =
            new Property<string>("ExchangeCode")
                .SetDescription("Short exchange code", Language.English)
                .SetDescription("Краткий код биржи", Language.Russian)
                .SetExamples("MOEX", "SPBEX", "LSE", "NASDAQ", "NYSE");
    }

    public class MetadataUsage
    { 
        [Fact]
        public void TypedPropertyCreation()
        {
            IProperty<string> property = Property.Create<string>("StringSample").SetAlias("SampleAlias");
            property.Type.Should().Be(typeof(string));
            property.Name.Should().Be("StringSample");
            property.Alias.Should().Be("SampleAlias");
        }

        [Fact]
        public void UntypedPropertyCreation()
        {
            IProperty property = Property.Create(typeof(string), "StringSample").SetAliasUntyped("SampleAlias");
            property.Type.Should().Be(typeof(string));
            property.Name.Should().Be("StringSample");
            property.Alias.Should().Be("SampleAlias");
        }
    }

}

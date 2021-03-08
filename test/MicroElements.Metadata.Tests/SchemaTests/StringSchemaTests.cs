using System.Linq;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests.SchemaTests
{
    public class StringSchemaTests
    {
        [Fact]
        public void StringMinLength()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string))
                .SetStringMinLength(3);

            schema.GetStringMinLength()!.MinLength.Should().Be(3);

            Property<string> currency = new Property<string>("Currency")
                .SetSchema(schema);

            var validationRules = ValidationProvider.Instance.GetValidationRules(currency).ToList();
            validationRules.Should().HaveCount(1);

            var messages = new MutablePropertyContainer()
                .WithValue(currency, "USD")
                .Validate(validationRules)
                .ToList();
            messages.Should().BeEmpty();

            messages = new MutablePropertyContainer()
                .WithValue(currency, "a")
                .Validate(validationRules)
                .ToList();

            messages[0].FormattedMessage.Should().Be("Value 'a' is too short (length: 1, minLength: 3)");
        }

        [Fact]
        public void StringMaxLength()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string))
                .SetStringMaxLength(3);

            schema.GetStringMaxLength()!.MaxLength.Should().Be(3);

            Property<string> currency = new Property<string>("Currency")
                .SetSchema(schema);

            var validationRules = ValidationProvider.Instance.GetValidationRules(currency).ToList();
            validationRules.Should().HaveCount(1);

            var messages = new MutablePropertyContainer()
                .WithValue(currency, "USD")
                .Validate(validationRules)
                .ToList();
            messages.Should().BeEmpty();

            messages = new MutablePropertyContainer()
                .WithValue(currency, "abcd")
                .Validate(validationRules)
                .ToList();

            messages[0].FormattedMessage.Should().Be("Value 'abcd' is too long (length: 4, maxLength: 3)");
        }

        [Fact]
        public void StringMinMaxLength()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string))
                .SetStringMinLength(3)

                .SetStringMaxLength(2)
                .SetStringMaxLength(3);

            Property<string> currency = new Property<string>("Currency")
                .SetSchema(schema);

            var validationRules = ValidationProvider.Instance.GetValidationRules(currency).ToList();
            validationRules.Should().HaveCount(2);
        }
    }
}

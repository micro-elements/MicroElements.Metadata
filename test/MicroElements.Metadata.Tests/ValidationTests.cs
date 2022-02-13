
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class ValidationSexTypeTests
    {
        public static IProperty<string> Name = new Property<string>("Name");
        public static IProperty<int> Age = new Property<int>("Age");
        public static IProperty<int?> NullableInt = new Property<int?>("NullableInt");

        public static IProperty<string> Sex = new Property<string>("Sex")
            .SetAllowedValues("Male", "Female");

        [Fact]
        public void validate()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex Jr")
                .WithValue(Age, 9)
                .WithValue(Sex, "Undefined");

            IEnumerable<IValidationRule> Rules()
            {
                yield return Name.NotNull();
                yield return Age.NotDefault().And().ShouldBe(a => a > 18).WithMessage("Age should be over 18! but was {value}");
                yield return Sex.OnlyAllowedValues().And().ShouldMatchNullability();
            }

            var messages = container.Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(2);
            messages[0].FormattedMessage.Should().Be("Age should be over 18! but was 9");
            messages[1].FormattedMessage.Should().Be("Sex can not be 'Undefined' because it is not in allowed values list. Allowed values: (Male, Female).");
        }

        [Fact]
        public void validate_not_exist()
        {
            var container = new MutablePropertyContainer()
                .WithValue(Name, "Alex");

            IEnumerable<IValidationRule> Rules()
            {
                yield return Name.Exists();
                yield return Age.Exists();
            }

            IValidator validator = Rules().Cached();
            var messages = container.Validate(validator).ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("Age is not exists.");
        }

        [Fact]
        public void validate_nullable_not_default()
        {
            IValidator notDefault = new Validator(NullableInt.NotDefault());

            var messages = new MutablePropertyContainer()
                .WithValue(NullableInt, null)
                .Validate(notDefault)
                .ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("NullableInt should not have default value null.");

            messages = new MutablePropertyContainer()
                .Validate(notDefault)
                .ToList();
            messages.Should().BeEmpty(because: "Property is absent and can not be default");

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, 0)
                .Validate(notDefault)
                .ToList();
            messages.Should().BeEmpty(because: "Property is nullable and 0 is not default value for Nullable<int>");

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, 42)
                .Validate(notDefault)
                .ToList();
            messages.Should().BeEmpty();

            IValidator existsAndNotDefault =
                new Validator(NullableInt.Exists().And(breakOnFirstError: false).NotDefault());

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, null)
                .Validate(existsAndNotDefault)
                .ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("NullableInt should not have default value null.");

            messages = new MutablePropertyContainer()
                .Validate(existsAndNotDefault)
                .ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("NullableInt is not exists.");

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, 0)
                .Validate(existsAndNotDefault)
                .ToList();
            messages.Should().BeEmpty(because: "Property is nullable and 0 is not default value for Nullable<int>");

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, 42)
                .Validate(existsAndNotDefault)
                .ToList();
            messages.Should().BeEmpty();
        }

        [Fact]
        public void validate_nullable_exists()
        {
            IEnumerable<IValidationRule> Rules()
            {
                yield return NullableInt.Exists();
            }

            var messages = new MutablePropertyContainer()
                .WithValue(Name, "Alex")
                .WithValue(NullableInt, null)
                .Validate(Rules().Cached()).ToList();
            messages.Should().BeEmpty();

            messages = new MutablePropertyContainer().Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("NullableInt is not exists.");

            messages = new MutablePropertyContainer()
                .WithValue(NullableInt, 42)
                .Validate(Rules().Cached()).ToList();
            messages.Should().BeEmpty();
        }

        [Fact]
        public void validate_and()
        {
            // Using And
            IEnumerable<IValidationRule> Rules1()
            {
                yield return Age.NotDefault().And().ShouldBe(a => a > 18).WithMessage("Age should be over 18! but was {value}");
            }

            // Not using And
            IEnumerable<IValidationRule> Rules2()
            {
                yield return Age.NotDefault();
                yield return Age.ShouldBe(a => a > 18).WithMessage("Age should be over 18! but was {value}");
            }

            Validate(Rules1());
            Validate(Rules2());

            void Validate(IEnumerable<IValidationRule> rules)
            {
                IValidator validator = rules.Cached();

                var messages = new MutablePropertyContainer()
                    .WithValue(Age, 0)
                    .Validate(validator)
                    .ToList();

                messages.Should().HaveCount(2);

                messages[0].FormattedMessage.Should().Be("Age should not have default value 0.");
                messages[1].FormattedMessage.Should().Be("Age should be over 18! but was 0");
            }
        }

        [Fact]
        public void validate_or()
        {
            IEnumerable<IValidationRule> Rules()
            {
                yield return Sex.ShouldBe(value => value == "Male").Or().ShouldBe(value => value == "Female");
            }

            var messages = new MutablePropertyContainer()
                .WithValue(Sex, "Other")
                .Validate(Rules().Cached())
                .ToList();

            messages.Should().HaveCount(1);

            messages[0].FormattedMessage.Should().Be("[Sex should match expression: (value == \"Male\") but value is 'Other'.] or [Sex should match expression: (value == \"Female\") but value is 'Other'.]");
        }


        [Fact]
        public void validate_or_for_different_properties()
        {
            IEnumerable<IValidationRule> Rules()
            {
                yield return Name.NotNull().Or(Age.NotDefault());
            }

            var messages = new MutablePropertyContainer()
                .WithValue(Name, null)
                .WithValue(Age, 0)
                .Validate(Rules().Cached())
                .ToList();

            messages.Should().HaveCount(1);

            messages[0].FormattedMessage.Should().Be("[Name should not be null.] or [Age should not have default value 0.]");
        }

        [Fact]
        public void validate_and_break_on_first_error()
        {
            IEnumerable<IValidationRule> Rules()
            {
                yield return Age.NotDefault().And(breakOnFirstError: true).ShouldBe(a => a > 18).WithMessage("Age should be over 18! but was {value}");
            }

            var messages = new MutablePropertyContainer()
                .WithValue(Age, 0).Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(1);

            messages[0].FormattedMessage.Should().Be("Age should not have default value 0.");
        }

        [Fact]
        public void validate_string_length()
        {
            IProperty<string> Name = new Property<string>("Name");

            IEnumerable<IValidationRule> Rules()
            {
                yield return new StringMinLengthValidationRule(Name, new StringMinLength(3));
                yield return new StringMaxLengthValidationRule(Name, new StringMaxLength(4));
            }

            var container = new MutablePropertyContainer()
                .WithValue(Name, "12345678");

            var messages = container.Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("Value '12345678' is too long (length: 8, maxLength: 4)");

            container = new MutablePropertyContainer()
                .WithValue(Name, "12");

            messages = container.Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(1);
            messages[0].FormattedMessage.Should().Be("Value '12' is too short (length: 2, minLength: 3)");

            container = new MutablePropertyContainer()
                .WithValue(Name, "1234");

            messages = container.Validate(Rules().Cached()).ToList();
            messages.Should().HaveCount(0);
        }
    }
}

using System;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests.SchemaTests
{
    public class SimpleSchemaTests
    {
        [Fact]
        public void SimpleSchema()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string))
                .WithDescription("ISO 4217 3-Letter Currency Code")
                .SetStringMinLength(3);

            schema.Name.Should().Be("Currency");
            schema.Description.Should().Be("ISO 4217 3-Letter Currency Code");
            schema.Type.Should().Be(typeof(string));

            schema.AsMetadataProvider().GetMetadataContainer().Count.Should().Be(1);
        }

        [Fact]
        public void SetDefaultValueUntyped()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string));

            Action setDefaultValue = () => schema.SetDefaultValueUntyped(null);
            setDefaultValue.Should().NotThrow();
            schema.GetDefaultValueMetadata().Should().NotBeNull();
            schema.GetDefaultValueMetadata().Value.Should().BeNull();

            schema = new SimpleTypeSchema("Currency", typeof(string));
            setDefaultValue = () => schema.SetDefaultValueUntyped(1);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("Value '1' can not be set as default value for type 'System.String'");
            schema.GetDefaultValueMetadata().Should().BeNull();

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(0.0);
            setDefaultValue.Should().NotThrow();
            schema.GetDefaultValueMetadata().Should().NotBeNull();
            schema.GetDefaultValueMetadata().Value.Should().Be(0.0);

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(100);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("Value '100' can not be set as default value for type 'System.Double'");
            schema.GetDefaultValueMetadata().Should().BeNull();

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(null);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("Value 'null' can not be set as default value for type 'System.Double'");
            schema.GetDefaultValueMetadata().Should().BeNull();
        }

        [Fact]
        public void SetDefaultValueTyped()
        {
            {
                var schema = new SimpleTypeSchema<string>("Currency");
                Action setDefaultValue = () => schema.SetDefaultValue(null);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValueMetadata().Should().NotBeNull();
                schema.GetDefaultValueMetadata().Value.Should().Be(null);
            }

            {
                var schema = new SimpleTypeSchema<string>("Currency");
                Action setDefaultValue = () => schema.SetDefaultValue("empty");
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValueMetadata().Should().NotBeNull();
                schema.GetDefaultValueMetadata().Value.Should().Be("empty");
            }

            {
                var schema = new SimpleTypeSchema<double>("Money");
                Action setDefaultValue = () => schema.SetDefaultValue(0.0);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValueMetadata().Should().NotBeNull();
                schema.GetDefaultValueMetadata().Value.Should().Be(0.0);
            }

            {
                var schema = new SimpleTypeSchema<double>("Money");
                Action setDefaultValue = () => schema.SetDefaultValue(100);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValueMetadata().Should().NotBeNull();
                schema.GetDefaultValueMetadata().Value.Should().Be(100.0);
            }

            {
                var schema = new Property<double>("Money").SetDefaultValue(100);
                schema.GetDefaultValueMetadata().Should().NotBeNull();
                schema.GetDefaultValue().Should().Be(100.0);
            }
        }

        [Fact]
        public void MinMax()
        {
            {
                ISchema<double> schema = new SimpleTypeSchema<double>("Rate")
                    .SetMinimum(0)
                    .SetMaximum(1);
                schema.GetNumericInterval().Should().NotBeNull().And.BeEquivalentTo(new NumericInterval(0, false, 1, false));

                IProperty<double> property = new Property<double>("Rate")
                    .SetMinimum(0)
                    .SetMaximum(1);
                property.GetNumericInterval().Should().NotBeNull().And.BeEquivalentTo(new NumericInterval(0, false, 1, false));
            }

        }
    }
}

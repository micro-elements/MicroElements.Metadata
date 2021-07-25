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
        public void SchemaBuilder()
        {
            SimpleTypeSchema simpleTypeSchema = new SimpleTypeSchema("Currency", typeof(string));

            ISchemaBuilder<SimpleTypeSchema, ISchemaDescription> schemaBuilder = simpleTypeSchema;
            ISchemaBuilder<IMetadataProvider, ISchemaDescription> schemaBuilderCovariant = simpleTypeSchema;
            ISchemaBuilder<SimpleTypeSchema, SchemaDescription> schemaBuilderComponentContravariant = simpleTypeSchema;

            string description = "ISO 4217 3-Letter Currency Code";

            var instance1 = schemaBuilder.With(new SchemaDescription(description));
            instance1.Description.Should().Be(description);

            var instance2 = schemaBuilderCovariant.With(new SchemaDescription(description));
            instance2.GetDescription().Should().Be(description);

            var instance3 = schemaBuilderComponentContravariant.With(new SchemaDescription(description));
            instance3.Description.Should().Be(description);
        }

        [Fact]
        public void SetDefaultValueUntyped()
        {
            var schema = new SimpleTypeSchema("Currency", typeof(string));

            Action setDefaultValue = () => schema.SetDefaultValueUntyped(null);
            setDefaultValue.Should().NotThrow();
            schema.GetDefaultValue().Should().NotBeNull();
            schema.GetDefaultValue().GetDefaultValue().Should().BeNull();

            schema = new SimpleTypeSchema("Currency", typeof(string));
            setDefaultValue = () => schema.SetDefaultValueUntyped(1);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("Value 1 can not be set as default value for type System.String");
            schema.GetDefaultValue().Should().BeNull();

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(0.0);
            setDefaultValue.Should().NotThrow();
            schema.GetDefaultValue().Should().NotBeNull();
            schema.GetDefaultValue().GetDefaultValue().Should().Be(0.0);

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(100);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("Value 100 can not be set as default value for type System.Double");
            schema.GetDefaultValue().Should().BeNull();

            schema = new SimpleTypeSchema("Money", typeof(double));
            setDefaultValue = () => schema.SetDefaultValueUntyped(null);
            setDefaultValue.Should().Throw<Exception>().And.Message.Should().Be("null value can not be set as default value for type System.Double");
            schema.GetDefaultValue().Should().BeNull();
        }

        [Fact]
        public void SetDefaultValueTyped()
        {
            {
                var schema = new SimpleTypeSchema<string>("Currency");
                Action setDefaultValue = () => schema.SetDefaultValue(null);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValue().Should().NotBeNull();
                schema.GetDefaultValue().GetDefaultValue().Should().Be(null);
            }

            {
                var schema = new SimpleTypeSchema<string>("Currency");
                Action setDefaultValue = () => schema.SetDefaultValue("empty");
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValue().Should().NotBeNull();
                schema.GetDefaultValue().GetDefaultValue().Should().Be("empty");
            }

            {
                var schema = new SimpleTypeSchema<double>("Money");
                Action setDefaultValue = () => schema.SetDefaultValue(0.0);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValue().Should().NotBeNull();
                schema.GetDefaultValue().GetDefaultValue().Should().Be(0.0);
            }

            {
                var schema = new SimpleTypeSchema<double>("Money");
                Action setDefaultValue = () => schema.SetDefaultValue(100);
                setDefaultValue.Should().NotThrow();
                schema.GetDefaultValue().Should().NotBeNull();
                schema.GetDefaultValue().GetDefaultValue().Should().Be(100.0);
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

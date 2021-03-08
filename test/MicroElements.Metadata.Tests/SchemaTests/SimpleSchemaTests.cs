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
                .SetStringMinLength(3);

            schema.Name.Should().Be("Currency");
            schema.Type.Should().Be(typeof(string));

            schema.AsMetadataProvider().GetMetadataContainer().Count.Should().Be(1);
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
    }
}

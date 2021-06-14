using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using MicroElements.Diagnostics;
using MicroElements.Functional;
using MicroElements.Metadata.Parsing;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Xml;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using NodaTime.Text;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class XmlReaderTests
    {
        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
        }

        public enum Sex
        {
            Unknown,
            Male,
            Female
        }

        public class PersonSchema : StaticSchema
        {
            public static readonly IProperty<string> FirstName = new Property<string>("FirstName")
                .AddValidation(property => property
                    .ShouldBe(s => s.Length > 1)
                    .ConfigureMessage((message, value, container) => message.WithProperty("length", value.ValueUntyped?.ToString()?.Length ?? 0))
                    .WithMessage("{propertyName} length should be greater then 1 but was {length}"));

            public static readonly IProperty<string> LastName = new Property<string>("LastName")
                .AddValidation(property => property.Required());

            public static readonly IProperty<Sex> Sex = new Property<Sex>("Sex")
                .SetSchema(new SimpleTypeSchema());

            public static readonly IProperty<IPropertyContainer> Address = new Property<IPropertyContainer>("Address")
                .SetSchema<AddressSchema>()
                .WithDescription("Address");

            public static readonly IProperty<IPropertyContainer> Addresses = new Property<IPropertyContainer>("Addresses")
                //.SetIsListOf(new AddressSchema())
                .SetSchema(new AddressSchema())
                .WithDescription("Addresses list ");
        }

        public class AddressSchema : StaticSchema
        {
            public static IProperty<string> City { get; } = new Property<string>("City");

            public static IProperty<int> Zip { get; } = new Property<int>("Zip");
        }

        public class SimpleTypeSchema : ISchema
        {
            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public Type Type { get; }

            /// <inheritdoc />
            public string? Description { get; }
        }

        [Fact]
        public void ReflectionSchema()
        {
            PersonSchema personSchema = new PersonSchema();
            var properties = personSchema.GetObjectSchema().GetProperties().ToArray();
            properties.Should().HaveCount(5);

            AddressSchema addressSchema = new AddressSchema();
            var city = addressSchema.GetObjectSchema().GetProperties().First();
            city.Should().NotBeNull();
            city.Name.Should().Be("City");
            city.Type.Should().Be(typeof(string));
        }

        [Fact]
        public void ReadXml()
        {
            string testXml = @"
<Person>
  <FirstName>Alex</FirstName>
  <Address>
    <City>NY</City>
  </Address>
  <LastName>Smith</LastName>
</Person>";

            IObjectSchema personSchema = new PersonSchema().GetObjectSchema();
            var container = XDocument.Parse(testXml).ParseXmlToContainer(personSchema);
            container.GetSchema().Should().NotBeNull();
        }

        [Fact]
        public void ReadInvalidXml()
        {
            string testXml = @"
<Person>
  <FirstName>z</FirstName>
  <Address>
    <City>NY</City>
    <Unknown>1</Unknown>
  </Address>
</Person>";

            IObjectSchema personSchema = new PersonSchema().GetObjectSchema();
            var container = XDocument.Parse(testXml).ParseXmlToContainer(personSchema);
            IXmlParserContext xmlParserContext = container.GetMetadata<IXmlParserContext>();

            var validationRules = personSchema.Properties.GetValidationRules().ToArray();
            var messages = container.Validate(validationRules).ToArray();
            messages.Should().HaveCount(2);
            messages[0].FormattedMessage.Should().Be("FirstName length should be greater then 1 but was 1");
            messages[1].FormattedMessage.Should().Be("LastName is marked as required but is not exists.");
            
            IObjectSchema addressSchemaStatic = personSchema.GetProperty("Address")!.GetSchema()!.ToObjectSchema();
            addressSchemaStatic.Properties.Should().HaveCount(2);

            IObjectSchema addressSchemaReal = xmlParserContext.GetSchema(personSchema.GetProperty("Address")).ToObjectSchema();
            addressSchemaReal.Properties.Should().HaveCount(3);

            IProperty[] notFromSchema = addressSchemaReal.GetPropertiesNotFromSchema().ToArray();
            notFromSchema.Should().HaveCount(1);
            notFromSchema[0].Name.Should().Be("Unknown");
            notFromSchema[0].Type.Should().Be(typeof(string));
        }

        [Fact]
        public void ReadObjectWithListWithoutSchema()
        {
            string testXml = @"
<Person>
  <FirstName>Alex</FirstName>
  <LastName>Smith</LastName>
  <Addresses>
    <Address>
      <City>NY</City>
      <Zip>111</Zip>
    </Address>
    <Address>
      <City>Moscow</City>
      <Zip>222</Zip>
    </Address>
  </Addresses>
  <Address>
    <City>NY</City>
    <Zip>333</Zip>
  </Address>
</Person>";

            var container = XDocument.Parse(testXml).ParseXmlToContainer();
            container.Should().NotBeNull();

            IPropertyValue[] values = container.Properties.ToArray();
            values[0].PropertyUntyped.Name.Should().Be("FirstName");
            values[0].PropertyUntyped.Type.Should().Be(typeof(string));

            values[2].PropertyUntyped.Name.Should().Be("Addresses");
            values[2].PropertyUntyped.Type.Should().Be(typeof(IPropertyContainer));

            values[3].PropertyUntyped.Name.Should().Be("Address");
            values[3].PropertyUntyped.Type.Should().Be(typeof(IPropertyContainer));
        }

        [Fact]
        public void ReadObjectWithListWithSchema()
        {
            string testXml = @"
<Person>
  <FirstName>Alex</FirstName>
  <LastName>Smith</LastName>
  <Sex>Male</Sex>
  <Addresses>
    <Address>
      <City>NY</City>
      <Zip>111</Zip>
    </Address>
    <Address>
      <City>Moscow</City>
      <Zip>222</Zip>
    </Address>
  </Addresses>
  <Address>
    <City>NY</City>
    <Zip>333</Zip>
  </Address>
</Person>";

            IObjectSchema personSchema = new PersonSchema().GetObjectSchema();

            IPropertyContainer? container = XDocument
                .Parse(testXml, LoadOptions.SetLineInfo)
                .ParseXmlToContainer(personSchema, new XmlParserSettings(validateOnParse: true));
            container.Should().NotBeNull();

            IPropertyValue[] values = container.Properties.ToArray();
            values[0].PropertyUntyped.Name.Should().Be("FirstName");
            values[0].PropertyUntyped.Type.Should().Be(typeof(string));

            values[1].PropertyUntyped.Name.Should().Be("LastName");
            values[1].PropertyUntyped.Type.Should().Be(typeof(string));

            values[2].PropertyUntyped.Name.Should().Be("Sex");
            values[2].PropertyUntyped.Type.Should().Be(typeof(Sex));

            values[3].PropertyUntyped.Name.Should().Be("Addresses");
            values[3].PropertyUntyped.Type.Should().Be(typeof(IPropertyContainer));

            values[4].PropertyUntyped.Name.Should().Be("Address");
            values[4].PropertyUntyped.Type.Should().Be(typeof(IPropertyContainer));

            var addressSchema = container.GetSchema().GetProperty("Address").GetSchema().ToObjectSchema();
            addressSchema.GetProperty("Zip").Type.Should().Be(typeof(int));

            var address = (values[4].ValueUntyped as IPropertyContainer).Properties.ToArray();
            address[0].PropertyUntyped.Name.Should().Be("City");
            address[0].PropertyUntyped.Type.Should().Be(typeof(string));

            address[1].PropertyUntyped.Name.Should().Be("Zip");
            address[1].PropertyUntyped.Type.Should().Be(typeof(int));
            address[1].ValueUntyped.Should().Be(333);
        }

        private string folder1 = "C:\\Projects\\test-data\\test-xmls";

        [Fact]
        public void ReadXml2()
        {
            if (!Directory.Exists(folder1))
                return;

            List<IPropertyContainer> list = new List<IPropertyContainer>();

            var schema = new PropertySet().ToSchema();

            foreach (string file in Directory.EnumerateFiles(folder1))
            {
                var xmlReader = System.Xml.XmlReader.Create(File.OpenRead(file));
                var propertyContainer = XmlParser.ReadXmlElement(xmlReader, schema) as IPropertyContainer;
                list.Add(propertyContainer);
            }
        }

        [Fact]
        public void ReadXml3()
        {
            if (!Directory.Exists(folder1))
                return;

            List<IPropertyContainer> list = new List<IPropertyContainer>();

            IXmlParserSettings parserSettings = new XmlParserSettings(
                propertyValueFactory: new PropertyValueFactory().Cached(),
                getElementName: element => element.GetFullName());

            var schema = new PropertySet().ToSchema();

            IXmlParserContext parserContext = new XmlParserContext(parserSettings, schema);

            foreach (string file in Directory.EnumerateFiles(folder1))
            {
                var propertyContainer = File.OpenRead(file).ParseXmlToContainer(schema, parserSettings, parserContext);
                list.Add(propertyContainer);
            }
        }

        [Fact]
        public void EnumParsingRules()
        {
            IXmlParserSettings parserSettings = new XmlParserSettings();
            XmlParserContext xmlParserContext = new XmlParserContext(parserSettings, null);

            Property<Sex> enumProp = new Property<Sex>("Sex");
            var enumParser = xmlParserContext.ParserRuleProvider.GetParser(enumProp);
            enumParser.Should().NotBeNull();

            CheckValue(enumProp, enumParser, "Male", true, Sex.Male);
            CheckValue(enumProp, enumParser, "???", false, null);
            CheckValue(enumProp, enumParser, null, false, null);

            Property<Sex?> nullableEnumProp = new Property<Sex?>("OptionalSex");
            var nullableEnumParser = xmlParserContext.ParserRuleProvider.GetParser(nullableEnumProp);
            nullableEnumParser.Should().NotBeNull();

            CheckValue(nullableEnumProp, nullableEnumParser, "Male", true, Sex.Male);
            CheckValue(nullableEnumProp, nullableEnumParser, "???", false, null);
            CheckValue(nullableEnumProp, nullableEnumParser, null, true, null);

            void CheckValue(IProperty property, IValueParser valueParser, string value, bool shouldBeSuccess, object shouldBeValue)
            {
                var parseResult = valueParser.ParseUntyped(value);
                parseResult.IsSuccess.Should().Be(shouldBeSuccess);
                if (shouldBeSuccess)
                {
                    IPropertyValue propertyValue = PropertyValue.Create(property, parseResult.ValueUntyped);
                    propertyValue.ValueUntyped.Should().Be(shouldBeValue);
                }
            }
        }

        public void ParseLocalDate()
        {
            string textDate = "2010-12-05";
            var parseResult = LocalDatePattern.Iso.Parse(textDate);
        }
    }
}

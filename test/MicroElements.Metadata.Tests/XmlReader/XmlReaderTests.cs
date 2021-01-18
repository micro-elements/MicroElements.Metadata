using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using MicroElements.Functional;
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

        public class PersonSchema : StaticPropertySet
        {
            public static readonly IProperty<string> FirstName = new Property<string>("FirstName")
                .AddValidation(property => property
                    .ShouldBe(s => s.Length > 1)
                    .ConfigureMessage((message, value, container) => message.WithProperty("length", value.ValueUntyped?.ToString()?.Length ?? 0))
                    .WithMessage("{propertyName} length should be greater then 1 but was {length}"));

            public static readonly IProperty<string> LastName = new Property<string>("LastName")
                .AddValidation(property => property.Required());

            public static readonly IProperty<Sex> Sex = new Property<Sex>("Sex");

            public static readonly IProperty<IPropertyContainer> Address = new Property<IPropertyContainer>("Address")
                .SetSchema(new AddressSchema())
                .WithDescription("Address");

            public static readonly IProperty<IPropertyContainer> Addresses = new Property<IPropertyContainer>("Addresses")
                //.SetIsListOf(new AddressSchema())
                .SetSchema(new AddressSchema())
                .WithDescription("Addresses list ");
        }

        public class AddressSchema : StaticPropertySet
        {
            public static IProperty<string> City { get; } = new Property<string>("City");

            public static IProperty<int> Zip { get; } = new Property<int>("Zip");
        }

        [Fact]
        public void ReflectionSchema()
        {
            PersonSchema personSchema = new PersonSchema();
            var properties = personSchema.ToArray();
            properties.Should().HaveCount(5);

            AddressSchema addressSchema = new AddressSchema();
            var city = addressSchema.First();
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

            ISchema personSchema = new PersonSchema().ToSchema();
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

            ISchema personSchema = new PersonSchema().ToSchema();
            var container = XDocument.Parse(testXml).ParseXmlToContainer(personSchema);

            var validationRules = personSchema.GetValidationRules().ToArray();
            var messages = container.Validate(validationRules).ToArray();
            messages.Should().HaveCount(2);
            messages[0].FormattedMessage.Should().Be("FirstName length should be greater then 1 but was 1");
            messages[1].FormattedMessage.Should().Be("LastName is marked as required but is not exists.");

            ISchema addressSchema = personSchema.GetProperty("Address")!.GetSchema()!;

            IProperty[] notFromSchema = addressSchema.GetPropertiesNotFromSchema().ToArray();
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

            IPropertyContainer? container = XDocument
                .Parse(testXml, LoadOptions.SetLineInfo)
                .ParseXmlToContainer(new PersonSchema().ToSchema());
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

            ISchema? addressSchema = container.GetSchema().GetProperty("Address").GetSchema();
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

            foreach (string file in Directory
                .EnumerateFiles(folder1))
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

            var schema = new PropertySet().ToSchema();

            IXmlParserSettings parserSettings = new XmlParserSettings();
            IXmlParserContext parserContext = new XmlParserContext(parserSettings);

            foreach (string file in Directory
                .EnumerateFiles(folder1))
            {
                var propertyContainer = File.OpenRead(file).ParseXmlToContainer(schema, parserSettings, parserContext);
                list.Add(propertyContainer);
            }
        }

        public void ParseLocalDate()
        {
            string textDate = "2010-12-05";
            var parseResult = LocalDatePattern.Iso.Parse(textDate);
        }
    }
}

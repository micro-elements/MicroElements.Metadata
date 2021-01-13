using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class XmlReaderTests
    {
        private string testXml = @"
<Person>
  <FirstName>Alex</FirstName>
  <Address>
    <City>NY</City>
    <Unknown>1</Unknown>
  </Address>
  <LastName>Smith</LastName>
</Person>";

        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
        }

        public class PersonSchema : StaticPropertySet
        {
            public static readonly IProperty<string> FirstName = new Property<string>("FirstName");
            public static readonly IProperty<string> LastName = new Property<string>("LastName");
            public static readonly IProperty<IPropertyContainer> Address = new Property<IPropertyContainer>("Address")
                .SetSchema(new AddressSchema().ToSchema());
        }

        public class AddressSchema : StaticPropertySet
        {
            public static IProperty<string> City { get; private set; }
        }


        public IPropertyContainer ParseXml(string xml, ISchema schema)
        {
            XmlReader xmlReader = XmlReader.Create(new StringReader(xml));

            return XmlParser.ReadXmlElement(xmlReader, schema) as IPropertyContainer;
        }

        [Fact]
        public void ReflectionSchema()
        {
            PersonSchema personSchema = new PersonSchema();
            var properties = personSchema.ToArray();
            properties.Should().HaveCount(3);

            AddressSchema addressSchema = new AddressSchema();
            var city = addressSchema.First();
            city.Should().NotBeNull();
            city.Name.Should().Be("City");
            city.Type.Should().Be(typeof(string));
        }

        [Fact]
        public void ReadXml()
        {
            PersonSchema personSchema = new PersonSchema();
            XmlReader xmlReader = XmlReader.Create(new StringReader(testXml));

            var container = XmlParser.ReadXmlElement(xmlReader, personSchema.ToSchema());
        }

        private string folder1 = "C:\\Projects\\sber\\TestData\\loans\\period-close-xmls-132519995974813732";
        private string folder2 = "C:\\Projects\\sber\\TestData\\loans\\many-loans";

        //[Fact]
        public void ReadXml2()
        {
            List<IPropertyContainer> list = new List<IPropertyContainer>();

            var schema = new PropertySet().ToSchema();

            foreach (string file in Directory
                .EnumerateFiles(folder1))
            {
                string xml = File.ReadAllText(file);
                XDocument xDocument = XDocument.Parse(xml);

                var propertyContainer = ParseXml(xml, schema);
                list.Add(propertyContainer);
            }
        }

        //[Fact]
        public void ReadXml3()
        {
            List<IPropertyContainer> list = new List<IPropertyContainer>();

            var schema = new PropertySet().ToSchema();

            foreach (string file in Directory
                .EnumerateFiles(folder1))
            {
                string xml = File.ReadAllText(file);
                XDocument xDocument = XDocument.Parse(xml);

                var propertyContainer = XmlParser.ParseXmlDocument(xDocument, schema);
                list.Add(propertyContainer);
            }
        }
    }
}

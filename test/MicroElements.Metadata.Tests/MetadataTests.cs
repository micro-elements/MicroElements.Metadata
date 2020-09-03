using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NodaTime;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class MetadataTests
    {
        [Fact]
        public void MetadataTest()
        {
            Client client =
                new Client("Bill", new LocalDate(2000, 04, 25))
                    .SetMetadata(new EntityMetadata("Database"))
                    .SetMetadata("AttachedProperty", 42);

            var entityMetadata = client.GetMetadata<EntityMetadata>();
            entityMetadata.Should().NotBeNull();
            entityMetadata.Source.Should().Be("Database");

            client.GetMetadata<int>("AttachedProperty").Should().Be(42);

            // Get metadata for client
            IPropertyContainer metadata = ((IMetadataProvider) client).Metadata;
            metadata.Count.Should().Be(2);

            // Instance properties does not include Metadata from IMetadataProvider
            var clientProperties = client.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            clientProperties.Should().HaveCount(2);

            client
                .ConfigureMetadata<Client, SomeMutableMetadata>(m => m.Source = "FromConfigure")
                .ConfigureMetadata<Client, SomeMutableMetadata>(m => m.Timestamp = DateTime.Today);

            var mutableMetadata = client.GetMetadata<SomeMutableMetadata>();
            mutableMetadata.Source.Should().Be("FromConfigure");
            mutableMetadata.Timestamp.Should().Be(DateTime.Today);

            client.ConfigureMetadata<SomeMutableMetadata>(m => m.Source = "Overriden");
            client.GetMetadata<SomeMutableMetadata>().Source.Should().Be("Overriden");
        }

        [Fact]
        public void PropertyContainer()
        {
            var propertyContainer = new MutablePropertyContainer();
            var propertyValueA = propertyContainer.SetValue("PropertyA", "ValueA");
            var propertyValueB = propertyContainer.SetValue(new Property<int>("PropertyB"), 42);

            propertyValueA.Should().NotBeNull();
            propertyValueB.Should().NotBeNull();

            propertyContainer.GetValue(propertyValueA.Property).Should().Be("ValueA");
            propertyContainer.GetValueUntyped("PropertyA").Should().Be("ValueA");

            dynamic dynamicContainer = propertyContainer.AsDynamic();
            object valueA = dynamicContainer.PropertyA;
            valueA.Should().Be("ValueA");

            object valueB = dynamicContainer.PropertyB;
            valueB.Should().Be(42);

            object notFoundProperty = dynamicContainer.NotFoundProperty;
            notFoundProperty.Should().BeNull();

            //TODO: override, parent
        }

        [Fact]
        public void PropertySetTests()
        {
            PropertySet propertySet = new PropertySet(new[] {new Property<string>("prop1"), new Property<string>("prop2"),});
            propertySet.Count().Should().Be(2);
            propertySet.Last().Name.Should().Be("prop2");
        }

    }

    public class EntityMetadata
    {
        public string Source { get; }

        public EntityMetadata(string source)
        {
            Source = source;
        }
    }

    public class SomeMutableMetadata
    {
        public string Source { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Client : IMetadataProvider
    {
        public string Name { get; }

        public LocalDate BirthDate { get; }

        public Client(string name, LocalDate birthDate)
        {
            Name = name;
            BirthDate = birthDate;
        }
    }
}

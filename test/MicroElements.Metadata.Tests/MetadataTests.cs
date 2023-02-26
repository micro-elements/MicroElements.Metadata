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
            IPropertyContainer metadata = ((IMetadataProvider) client).GetMetadataContainer();
            metadata.Properties.Count.Should().Be(2);

            // Default properties does not include Metadata from IMetadataProvider
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
            propertyContainer.GetValueUntypedByName("PropertyA").Should().Be("ValueA");

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
        public void DynamicContainer()
        {
            var propertyContainer = new MutablePropertyContainer();
            propertyContainer.SetValue("PropertyA", "ValueA");
            propertyContainer.SetValue(new Property<int>("PropertyB"), 42);

            dynamic dynamicContainer = propertyContainer.AsDynamic();
            object valueA = dynamicContainer.PropertyA;
            valueA.Should().Be("ValueA");

            object valueB = dynamicContainer.PropertyB;
            valueB.Should().Be(42);

            object notFoundProperty = dynamicContainer.NotFoundProperty;
            notFoundProperty.Should().BeNull();
        }

        [Fact]
        public void PropertySetTests()
        {
            PropertySet propertySet = new(new Property<string>("prop1"), new Property<string>("prop2"));
            propertySet.Properties.Count.Should().Be(2);
            propertySet.Properties.Last().Name.Should().Be("prop2");
        }

        [Fact]
        public void MetadataWithUserImplementation()
        {
            Client2 client =
                new Client2("Bill", new LocalDate(2000, 04, 25))
                    .SetMetadata(new EntityMetadata("Database"))
                    .SetMetadata("AttachedProperty", 42);

            var entityMetadata = client.GetMetadata<EntityMetadata>();
            entityMetadata.Should().NotBeNull();
            entityMetadata.Source.Should().Be("Database");

            client.GetMetadata<int>("AttachedProperty").Should().Be(42);

            // Get metadata for client
            IPropertyContainer metadata = client.Metadata;
            metadata.Properties.Count.Should().Be(2);

            IPropertyContainer propertyContainer = client.FreezeMetadata();
            propertyContainer.Should().BeOfType<PropertyContainer>();
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

    public class Client2 : IManualMetadataProvider
    {
        public IPropertyContainer Metadata { get; private set; } = new MutablePropertyContainer();

        public string Name { get; }

        public LocalDate BirthDate { get; }

        public Client2(string name, LocalDate birthDate)
        {
            Name = name;
            BirthDate = birthDate;
        }
    }
}

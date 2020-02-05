using System;
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
        }

        [Fact]
        public void PropertyContainer()
        {
            var propertyContainer = new MutablePropertyContainer();
            //TODO propertyContainer.SetValue()
        }

        [Fact]
        public void ParseTest()
        {
            //TODO
        }

        [Fact]
        public void ReportTest()
        {
            //TODO
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

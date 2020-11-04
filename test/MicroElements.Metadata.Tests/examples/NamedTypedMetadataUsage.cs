using FluentAssertions;
using Xunit;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace MicroElements.Metadata.Tests.examples
{
    public class NamedTypedMetadataUsage
    {
        /// <summary>
        /// Sample metadata.
        /// </summary>
        public class SampleMetadata
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Sample entity that extend <see cref="IMetadataProvider"/>.
        /// </summary>
        public class MyEntity : IMetadataProvider
        {
        }

        [Fact]
        public void set_and_get_typed_metadata()
        {
            // Set typed metadata to object.
            MyEntity instance1 = new MyEntity().SetMetadata(new SampleMetadata
            {
                Name = "Instance1", Description = "Instance1 description"
            });

            // Get typed metadata from object.
            var sampleMetadata = instance1.GetMetadata<SampleMetadata>();
            sampleMetadata.Should().NotBeNull();
            sampleMetadata.Name.Should().Be("Instance1");
        }

        [Fact]
        public void set_and_get_named_typed_metadata()
        {
            // Set named typed metadata to object.
            MyEntity entity = new MyEntity();
            entity.SetMetadata("Meta1", new SampleMetadata {Name = "Meta1"});
            entity.SetMetadata("Meta2", new SampleMetadata {Name = "Meta2"});

            entity.GetMetadata<SampleMetadata>().Should().BeNull();

            // Get named and typed metadata.
            entity.GetMetadata<SampleMetadata>("Meta1").Should().NotBeNull();
            entity.GetMetadata<SampleMetadata>("Meta1").Name.Should().Be("Meta1");
            entity.GetMetadata<SampleMetadata>("Meta2").Name.Should().Be("Meta2");
        }

        [Fact]
        public void configure_typed_metadata()
        {
            MyEntity entity = new MyEntity();

            // Configure metadata several times.
            entity.ConfigureMetadata<SampleMetadata>(metadata => metadata.Name = "Instance1");
            entity.ConfigureMetadata<SampleMetadata>(metadata => metadata.Description = "Instance1 description");

            // Get merged metadata
            entity.GetMetadata<SampleMetadata>().Name.Should().Be("Instance1");
            entity.GetMetadata<SampleMetadata>().Description.Should().Be("Instance1 description");
        }
    }
}

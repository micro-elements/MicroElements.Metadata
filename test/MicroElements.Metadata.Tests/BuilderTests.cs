using System.Linq;
using FluentAssertions;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;
using Xunit;

namespace MicroElements.Metadata.Tests
{
    public class BuilderTests
    {
        public record Name(string FirstName);

        public record Address(string City);

        public interface IPerson :
            IComposite,
            IHas<Name>,
            IHas<Address>
        {
            Name? Name { get; }

            Address? Address { get; }

            /// <inheritdoc />
            Name? IHas<Name>.Component => Name;

            /// <inheritdoc />
            Address? IHas<Address>.Component => Address;
        }

        public class ImmutablePerson:
            IPerson,
            ICompositeBuilder<ImmutablePerson, Name>,
            ICompositeBuilder<ImmutablePerson, Address>
        {
            public Name? Name { get; }

            public Address? Address { get; }

            public ImmutablePerson()
            {
            }

            public ImmutablePerson(Name? name = null, Address? address = null)
            {
                Name = name;
                Address = address;
            }

            /// <inheritdoc />
            public ImmutablePerson With(Name name) => new (name, Address);

            /// <inheritdoc />
            public ImmutablePerson With(Address address) => new (Name, address);
        }

        public class MutablePerson :
            IPerson,
            ICompositeSetter<Name>,
            ICompositeSetter<Address>
        {
            public Name? Name { get; private set; }

            public Address? Address { get; private set; }

            /// <inheritdoc />
            public void Append(Name name) => Name = name;

            /// <inheritdoc />
            public void Append(Address address) => Address = address;
        }

        public class HybridPerson :
            IMetadataProvider,
            IPerson,
            ICompositeBuilder<HybridPerson, Name>,
            ICompositeBuilder<HybridPerson, Address>
        {
            public Name? Name { get; }

            public Address? Address { get; }

            /// <inheritdoc />
            Name? IHas<Name>.Component => Name ?? this.GetMetadata<Name>();

            /// <inheritdoc />
            Address? IHas<Address>.Component => Address ?? this.GetMetadata<Address>();

            public HybridPerson(Name? name = null, Address? address = null)
            {
                Name = name;
                Address = address;
            }

            /// <inheritdoc />
            public HybridPerson With(Name name) => new HybridPerson(name, Address);

            /// <inheritdoc />
            public HybridPerson With(Address address) => new HybridPerson(Name, address);
        }

        public class VirtualPerson :
            IMetadataProvider,
            IPerson,
            ICompositeSetter<Name>,
            ICompositeSetter<Address>
        {
            public Name? Name => this.GetMetadata<Name>();

            public Address? Address => this.GetMetadata<Address>();

            /// <inheritdoc />
            public void Append(Name name) => this.SetMetadata(name);

            /// <inheritdoc />
            public void Append(Address address) => this.SetMetadata(address);
        }

        [Fact]
        public void build_immutable_with()
        {
            var person1 = new ImmutablePerson()
                .With(new Name("Alex"))
                .With(new Address("Moscow"));

            person1.Name.FirstName.Should().Be("Alex");
            person1.Address.City.Should().Be("Moscow");

            var person2 = person1.With(new Address("NY"));
            person2.Should().NotBeSameAs(person1);

            person2.Name.FirstName.Should().Be("Alex");
            person2.Address.City.Should().Be("NY");
        }

        [Fact]
        public void build_immutable_with_component()
        {
            var person1 = new ImmutablePerson()
                .WithComponent(new Name("Alex"))
                .WithComponent(new Address("Moscow"));

            person1.Name.FirstName.Should().Be("Alex");
            person1.Address.City.Should().Be("Moscow");

            var person2 = person1.WithComponent(new Address("NY"));
            person2.Name.FirstName.Should().Be("Alex");
            person2.Address.City.Should().Be("NY");

            person2.Should().NotBeSameAs(person1);
            person1.Name.FirstName.Should().Be("Alex");
            person1.Address.City.Should().Be("Moscow");
        }

        [Fact]
        public void build_mutable_with()
        {
            var person1 = new MutablePerson()
                .WithComponent(new Name("Alex"))
                .WithComponent(new Address("Moscow"));

            person1.Name.FirstName.Should().Be("Alex");
            person1.Address.City.Should().Be("Moscow");

            var person2 = person1.WithComponent(new Address("NY"));
            person2.Name.FirstName.Should().Be("Alex");
            person2.Address.City.Should().Be("NY");

            person2.Should().BeSameAs(person1);
            person1.Address.City.Should().Be("NY");
        }

        [Fact]
        public void build_hybrid_person_with_attached_metadata()
        {
            var person1 = new HybridPerson()
                .WithComponent(new Name("Alex"))
                .SetMetadata(new Address("Moscow"));

            person1.Name.FirstName.Should().Be("Alex");

            // Address component is absent it's in external stored metadata
            person1.Address.Should().BeNull(because: "Address component is absent it's in external stored metadata");
            
            // But it can be accessed with GetComponent
            person1.GetComponent<Address>().City.Should().Be("Moscow");
        }

        [Fact]
        public void build_virtual_person()
        {
            var person1 = new VirtualPerson()
                .WithComponent(new Name("Alex"))
                .SetMetadata(new Address("Moscow"));

            person1.Name.Should().NotBeNull();
            person1.Address.Should().NotBeNull();

            person1.Name.FirstName.Should().Be("Alex");
            person1.Address.City.Should().Be("Moscow");

            person1.GetComponent<Name>().FirstName.Should().Be("Alex");
            person1.GetComponent<Address>().City.Should().Be("Moscow");
        }

        [Fact]
        public void get_components_of_composite()
        {
            var person1 = new ImmutablePerson()
                .WithComponent(new Name("Alex"))
                .WithComponent(new Address("Moscow"));

            object[] components = ((IComposite)person1).Components().ToArray();
            components[0].Should().Be(new Name("Alex"));
            components[1].Should().Be(new Address("Moscow"));
        }

        [Fact]
        public void rebuild_composite_into_other_type()
        {
            var mutablePerson = new MutablePerson()
                .WithComponent(new Name("Alex"))
                .WithComponent(new Address("Moscow"));

            ImmutablePerson immutablePerson = mutablePerson.BuildAs<ImmutablePerson>();

            immutablePerson.Name.FirstName.Should().Be("Alex");
            immutablePerson.Address.City.Should().Be("Moscow");
        }
    }
}

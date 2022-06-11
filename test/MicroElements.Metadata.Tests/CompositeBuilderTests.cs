using System.Collections.Generic;
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

        /// <summary> A person base interface. </summary>
        public interface IPerson :
            IComposite,
            IHas<Name>,
            IHas<Address>
        {
            /// <summary> Gets the person name. </summary>
            Name? Name { get; }

            /// <summary> Gets the person address. </summary>
            Address? Address { get; }

            /// <inheritdoc />
            Name? IHas<Name>.Component => Name;

            /// <inheritdoc />
            Address? IHas<Address>.Component => Address;
        }

        public class ImmutablePerson :
            IPerson,
            ICompositeBuilder<ImmutablePerson, Name>,
            ICompositeBuilder<ImmutablePerson, Address>
        {
            public Name? Name { get; }

            public Address? Address { get; }
            
            public ImmutablePerson(Name? name = null, Address? address = null)
            {
                Name = name;
                Address = address;
            }
            
            /// <inheritdoc />
            public ImmutablePerson With(Name name) => new(name, Address);

            /// <inheritdoc />
            public ImmutablePerson With(Address address) => new(Name, address);
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

        public class PersonData : ICloneable<PersonData>
        {
            public Name? Name { get; set; }

            public Address? Address { get; set; }
        }

        public class ConfigurablePerson :
            IPerson,
            IConfigurableBuilder<ConfigurablePerson, PersonData>
        {
            private readonly PersonData _data;

            public Name? Name => _data.Name;

            public Address? Address => _data.Address;

            public ConfigurablePerson(PersonData? data = null) => _data = data?.Clone() ?? new PersonData();

            /// <inheritdoc />
            public PersonData GetState() => _data.Clone();

            /// <inheritdoc />
            public ConfigurablePerson With(PersonData data) => new ConfigurablePerson(data);
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

            object[] components = person1.GetComponents().ToArray();
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

        [Fact]
        public void build_configurable_person()
        {
            var person = new ConfigurablePerson()
                .Configure(data => data.Name = new Name("Alex"))
                .Configure(data => data.Address = new Address("Moscow"));

            person.Name.FirstName.Should().Be("Alex");
            person.Address.City.Should().Be("Moscow");
        }

        [Fact]
        public void get_components_and_metadata()
        {
            object[] components = new Property<int>("int_default_42")
                .WithDefaultValue(42)
                .SetRequired()
                .GetComponentsAndMetadata()
                .ToArray();
            components[0].Should().BeOfType<DefaultValue<int>>();
            components[1].Should().BeOfType<Required>();
        }

        public class Sample : IStaticComponentProvider<Sample.Metadata>
        {
            public class Metadata : IComposite
            {
                /// <inheritdoc />
                public IEnumerable<object> GetComponents()
                {
                    yield return Required.Instance;
                }
            }
        }

        [Fact]
        public void get_static_components()
        {
            object[] objects = new Sample().GetStaticComponents().ToArray();
        }
    }
}

namespace MicroElements.Builder1
{
    public record Name(string FirstName);

    public record Address(string City);

    /// <summary> A person base interface. </summary>
    public interface IPerson
    {
        /// <summary> Gets the person name. </summary>
        Name? Name { get; }

        /// <summary> Gets the person address. </summary>
        Address? Address { get; }
    }
    
    public class Person : IPerson
    {
        public Name? Name { get; set; }

        public Address? Address { get; set; }
    }
    
    public record PersonRecord : IPerson
    {
        public Name? Name { get; init; }

        public Address? Address { get; init; }
    }
    
    public class Test
    {
        [Fact]
        public void create_mutable_person()
        {
            Person person = new Person()
            {
                Name = new Name("Alex"),
                Address = new Address("Moscow")
            };
        }
    }
}

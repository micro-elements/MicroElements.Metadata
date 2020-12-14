using System.Collections.Generic;
using MicroElements.Metadata.Schema;
using Microsoft.AspNetCore.Mvc;

namespace MicroElements.Metadata.SampleApp.Controllers
{
    [ApiController]
    public class MetadataController : Controller
    {
        [HttpGet("[action]")]
        public Person GetPerson()
        {
            Person person = new Person()
            {
                Name = "Alex",
                Sex = "Male",
                Age = 42
            };

            return person;
        }

        [HttpGet("[action]")]
        public PersonView GetPersonView()
        {
            Person person = new Person()
            {
                Name = "Alex",
                Sex = "Male",
                Age = 42
            };

            IPropertyContainer propertyContainer = new PersonMetadata().ToContainer(person);

            return new PersonView
            {
                Person = new PropertyContainerContract<PersonMetadata>(propertyContainer)
            };
        }
    }

    public class Person
    {
        public string? Name { get; set; }
        public string? Sex { get; set; }
        public int Age { get; set; }
    }

    public class PersonView
    {
        [PropertySet(Type = typeof(DatabaseMeta))]
        public IPropertyContainer Database { get; set; }

        public PropertyContainerContract<PersonMetadata> Person { get; set; }
    }

    public class PropertyContainerContract<TPropertySet> : PropertyContainer, IKnownPropertySet<TPropertySet>
        where TPropertySet : IPropertySet, new()
    {
        /// <inheritdoc />
        public PropertyContainerContract(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
            : base(sourceValues, parentPropertySource, searchOptions)
        {
        }
    }

    public class DatabaseMeta : IPropertySet
    {
        public static IProperty<string> StorageId = new Property<string>("StorageId")
            .WithDescription("Unique StorageId.")
            .SetNotNull();

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties()
        {
            yield return StorageId;
        }
    }

    public class PersonMetadata : IPropertySet, IModelMapper<Person>
    {
        public static IProperty<string> Name = new Property<string>("Name")
            .WithDescription("Person name.")
            .SetNotNull();

        public static IProperty<string> Sex = new Property<string>("Sex")
            .WithDescription("Person sex")
            .WithAllowedValues("Male", "Female");

        public static IProperty<int> Age = new Property<int>("Age")
            .WithDescription("Person age in years.");

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties()
        {
            yield return Name;
            yield return Sex;
            yield return Age;
        }

        /// <inheritdoc />
        public IPropertyContainer ToContainer(Person model)
        {
            return new MutablePropertyContainer()
                .WithValue(Name, model.Name)
                .WithValue(Sex, model.Sex)
                .WithValue(Age, model.Age);
        }

        /// <inheritdoc />
        public Person ToModel(IPropertyContainer container)
        {
            throw new System.NotImplementedException();
        }
    }
}

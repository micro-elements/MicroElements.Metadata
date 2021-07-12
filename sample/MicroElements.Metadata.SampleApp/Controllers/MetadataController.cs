using System;
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
                Person = new PropertyContainer<PersonMetadata>(propertyContainer)
            };
        }
    }

    public class Person
    {
        public string? Name { get; set; }
        public string? Sex { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class PersonView
    {
        [PropertySet(Type = typeof(DatabaseMeta))]
        public IPropertyContainer Database { get; set; }

        public PropertyContainer<PersonMetadata> Person { get; set; }
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

    public enum SexTypeEnum
    {
        Male,
        Female
    }

    public class PersonMetadata : IPropertySet, IModelMapper<Person>
    {
        public static ISchema<string> SexType = new Property<string>("SexType")
            .WithDescription("Person sex")
            .SetAllowedValues("Male", "Female");


        public static IProperty<string> Name = new Property<string>("Name")
            .WithDescription("Person name.")
            .SetNotNull();

        public static IProperty<string> Sex = new Property<string>("Sex")
            .WithDescription("Person sex")
            .SetAllowedValues("Male", "Female");

        public static IProperty<string> Sex2 = new Property<string>("Sex2")
            .SetSchema(SexType);

        public static IProperty<SexTypeEnum> Sex3 = new Property<SexTypeEnum>("Sex3")
            .WithDescription("Person sex")
            .SetAllowedValuesFromEnum<SexTypeEnum>();

        public static IProperty<int> Age = new Property<int>("Age")
            .WithDescription("Person age in years.");

        public static IProperty<DateTime> BirthDate = new Property<DateTime>("BirthDate")
            .WithDescription("BirthDate.");

        /// <inheritdoc />
        public IEnumerable<IProperty> GetProperties()
        {
            yield return Name;
            yield return Sex;
            yield return Sex2;
            yield return Sex3;
            yield return Age;
            yield return BirthDate;
        }

        /// <inheritdoc />
        public IPropertyContainer ToContainer(Person model)
        {
            return new MutablePropertyContainer()
                .WithValue(Name, model.Name)
                .WithValue(Sex, model.Sex)
                .WithValue(Sex2, model.Sex)
                .WithValue(Age, model.Age);
        }

        /// <inheritdoc />
        public Person ToModel(IPropertyContainer container)
        {
            throw new System.NotImplementedException();
        }
    }
}

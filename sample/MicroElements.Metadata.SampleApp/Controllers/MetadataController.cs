using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata.Schema;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using Microsoft.AspNetCore.Mvc;

namespace MicroElements.Metadata.SampleApp.Controllers
{
    [ApiController]
    internal class MetadataController : Controller
    {
        [HttpGet("[action]")]
        public Person GetPerson()
        {
            Person person = new Person()
            {
                Name = "Alex",
                Sex = SexTypeEnum.Male,
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
                Sex = SexTypeEnum.Male,
                Age = 42
            };

            IPropertyContainer propertyContainer = new PersonMetadata().ToContainer(person);

            return new PersonView
            {
                Person = new PropertyContainer<PersonMetadata>(propertyContainer)
            };
        }

        [HttpGet("[action]")]
        public PersonView GetPersonView2()
        {
            PropertyContainer<PersonMetadata> propertyContainer = new PropertyContainer<PersonMetadata>()
                .WithValue(PersonMetadata.Name, "Alex")
                .WithValue(PersonMetadata.Sex, "UNKNOWN")
                .WithValue(PersonMetadata.Age, 42);

            var validationRules = new PersonMetadata()
                .GetProperties()
                .SelectMany(ValidationProvider.Instance.GetValidationRules)
                .ToArray();

            var messages = propertyContainer.Validate(validationRules).ToArray();

            return new PersonView
            {
                Person = new PropertyContainer<PersonMetadata>(propertyContainer),
                Messages = messages.Select(message => message.FormattedMessage).ToArray(),
            };
        }

    }

    public class Person
    {
        public string? Name { get; set; }

        /// <summary>
        /// Person sex.
        /// </summary>
        public SexTypeEnum? Sex { get; set; }

        /// <summary>
        /// Person age.
        /// </summary>
        public int Age { get; set; }

        public DateTime BirthDate { get; set; }
    }

    public class PersonView
    {
        [PropertySet(Type = typeof(DatabaseMeta))]
        public IPropertyContainer Database { get; set; }

        public PropertyContainer<PersonMetadata> Person { get; set; }

        public string[] Messages { get; set; }
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
                .WithValue(Sex, model.Sex?.ToString())
                .WithValue(Sex2, model.Sex?.ToString())
                .WithValue(Age, model.Age);
        }

        /// <inheritdoc />
        public Person ToModel(IPropertyContainer container)
        {
            throw new System.NotImplementedException();
        }
    }
}

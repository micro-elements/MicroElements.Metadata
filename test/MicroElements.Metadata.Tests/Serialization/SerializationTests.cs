using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using MicroElements.Reflection;
using MicroElements.Metadata.Diff;
using MicroElements.Metadata.JsonSchema;
using MicroElements.Metadata.Mapping;
using MicroElements.Metadata.Formatting;
using MicroElements.Metadata.NewtonsoftJson;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using MicroElements.Metadata.SystemTextJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Xunit;
using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MicroElements.Metadata.Tests.Serialization
{
    public class SerializationTests
    {
        public IPropertyContainer CreateTestContainer()
        {
            var initialContainer = new MutablePropertyContainer()
                    .WithValue(TestMeta.StringProperty, "Text")
                    .WithValue(TestMeta.IntProperty, 42)
                    .WithValue(TestMeta.DoubleProperty, 10.2)
                    .WithValue(TestMeta.NullableIntProperty, null)
                    .WithValue(TestMeta.BoolProperty, true)
                    .WithValue(TestMeta.DateProperty, new LocalDate(2020, 12, 26))
                    .WithValue(TestMeta.StringArray, new[] { "a1", "a2" })
                    .WithValue(TestMeta.IntArray, new[] { 1, 2 })
                ;

            return initialContainer;
        }

        class TestJsonSchemaGenerator : IJsonSchemaGenerator
        {
            /// <inheritdoc />
            public object GenerateSchema(ISchema schema)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void MetadataJsonSerializationOptions_should_be_copied_properly()
        {
            Fixture fixture = new Fixture();
            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(ITypeMapper),
                    typeof(DefaultTypeMapper)));
            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IJsonSchemaGenerator),
                    typeof(TestJsonSchemaGenerator)));
            var options = fixture.Create<MetadataJsonSerializationOptions>();
            var optionsCopy = options.Copy();

            optionsCopy.Should().BeEquivalentTo(options);
        }

        [Fact]
        public void SerializeDeserializePropertyContainer()
        {
            var initialContainer = CreateTestContainer();

            var propertyContainerContract = initialContainer.ToContract(new DefaultMetadataSerializer());
            propertyContainerContract.Should().NotBeNull();

            var contractJson = propertyContainerContract.ToJsonWithSystemTextJson();

            var json1 = initialContainer.ToJsonWithSystemTextJson();
            var jsonOptions = new JsonSerializerOptions().ConfigureJsonOptions();

            DeserializeAndCompare<IPropertyContainer>();
            DeserializeAndCompare<PropertyContainer>();
            DeserializeAndCompare<IMutablePropertyContainer>();
            DeserializeAndCompare<MutablePropertyContainer>();
            DeserializeAndCompare<PropertyContainer<TestMeta>>();

            var json2 = initialContainer.ToJsonWithNewtonsoftJson();
            var container2 = json2.DeserializeWithNewtonsoftJson<IPropertyContainer>();
            ObjectDiff objectDiff = MetadataComparer.GetDiff(initialContainer, container2);
            objectDiff.Diffs.Should().BeEmpty();

            void DeserializeAndCompare<TContainer>() where TContainer : IPropertyContainer
            {
                TContainer? restored = JsonSerializer.Deserialize<TContainer>(json1, jsonOptions);

                ObjectDiff objectDiff = MetadataComparer.GetDiff(restored, initialContainer);
                objectDiff.Diffs.Should().BeEmpty();
            }
        }

        [Fact]
        public void ReadWithSchemaFirst()
        {
            string json = @"{
  'StringProperty': 'Text',
  'IntProperty': 42,
  'DoubleProperty': 10.2,
  'NullableIntProperty': null,
  'BoolProperty': true,
  'DateProperty': '2020-12-26',
  'StringArray':['a1','a2'],
  'IntArray':[1,2],
  '$metadata.schema.compact': [
    'StringProperty@type=string',
    'IntProperty@type=int',
    'DoubleProperty@type=double',
    'NullableIntProperty@type=int?',
    'BoolProperty @type = bool',
    'DateProperty@type=LocalDate',
    'StringArray@type=string[]',
    'IntArray@type=int[]'
  ]
}";
            var initialContainer = CreateTestContainer();

            var restoredContainer = json.DeserializeWithNewtonsoftJson<IPropertyContainer>(configureSerialization: options => options.ReadSchemaFirst = true);
            ObjectDiff objectDiff = MetadataComparer.GetDiff(restoredContainer, initialContainer);
            objectDiff.Diffs.Should().BeEmpty();

            // container will lose types
            var restoredContainer2 = json.DeserializeWithNewtonsoftJson<IPropertyContainer>(configureSerialization: options => options.ReadSchemaFirst = false);
            ObjectDiff objectDiff2 = MetadataComparer.GetDiff(restoredContainer2, initialContainer);
            objectDiff2.Diffs.Should().NotBeNullOrEmpty();
        }

        public class ComplexObject
        {
            public PropertyContainer<TestMeta> Data1 { get; set; }
            public PropertyContainer<TestMeta> Data2 { get; set; }
        }

        public class ComplexObjectUntyped : IMetadataSchemaProvider
        {
            public IPropertyContainer Data1 { get; set; }
            public IPropertyContainer Data2 { get; set; }
        }

        [Fact]
        public void ReadWithSchemaRef()
        {
            string json = @"{
  'Data1': {
    '$ref': '#/$defs/metadata.schema.TestMeta',
    'StringProperty': 'Text',
    'IntProperty': 42,
    'DoubleProperty': 10.2,
    'NullableIntProperty': null,
    'BoolProperty': true,
    'DateProperty': '2020-12-26',
    'StringArray':['a1','a2'],
    'IntArray':[1,2]
  },
  'Data2': {
    '$ref': '#/$defs/metadata.schema.TestMeta',
    'StringProperty': 'Text',
    'IntProperty': 42,
    'DoubleProperty': 10.2,
    'NullableIntProperty': null,
    'BoolProperty': true,
    'DateProperty': '2020-12-26',
    'StringArray':['a1','a2'],
    'IntArray':[1,2]
  },
  '$defs':
  {
    'metadata.schema.TestMeta':
	{
      '$metadata.schema.compact': [
       'StringProperty@type=string',
       'IntProperty@type=int',
       'DoubleProperty@type=double',
       'NullableIntProperty@type=int?',
       'BoolProperty@type=bool',
       'DateProperty@type=LocalDate',
       'StringArray@type=string[]',
       'IntArray@type=int[]'
	   ]
	}
  }
}";
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings().ConfigureJsonForPropertyContainers(options => options.UseSchemasRoot = true);

            var complexObject = json.DeserializeWithNewtonsoftJson<ComplexObjectUntyped>(jsonSerializerSettings);
            complexObject.Data1.Properties.FirstOrDefault(value => value.PropertyUntyped.Name == "NullableIntProperty")
                .PropertyUntyped.Type.Should().Be(typeof(int?));

            var complexObject2 = json.DeserializeWithNewtonsoftJson<ComplexObjectUntyped>(jsonSerializerSettings);
            complexObject2.Data1.Properties.FirstOrDefault(value => value.PropertyUntyped.Name == "NullableIntProperty")
                .PropertyUntyped.Type.Should().Be(typeof(int?));
        }

        [Fact]
        public void WriteWithSchemaRef()
        {
            ComplexObjectUntyped complexObject = new ComplexObjectUntyped()
            {
                Data1 = CreateTestContainer(),
                Data2 = CreateTestContainer()
            };

            string jsonWithNewtonsoftJson1 = complexObject.ToJsonWithNewtonsoftJson(configureSerialization: options =>
            {
                options.UseSchemasRoot = false;
            });
            string jsonWithNewtonsoftJson2 = complexObject.ToJsonWithNewtonsoftJson(configureSerialization: options =>
            {
                options.UseSchemasRoot = true;
                options.UseJsonSchema = false;
            });
            string jsonWithNewtonsoftJson3 = complexObject.ToJsonWithNewtonsoftJson(configureSerialization: options =>
            {
                options.UseSchemasRoot = true;
                options.UseJsonSchema = true;
            });
        }

        class TestPerson
        {
            public string Name { get; set; }
            public DateTime BirthDate { get; set; }
        }

        [Fact]
        public void NewtonsoftJsonRefSample()
        {
            string json = @"
[
  {
    '$id': '1',
    'Name': 'James',
    'BirthDate': '1983-03-08T00:00Z'
  },
  {
    '$ref': '1'
  }
]";

            var persons = json.DeserializeWithNewtonsoftJson<List<TestPerson>>();
            persons[^1].Name.Should().Be("James");
        }

        private IReferenceResolver? ReferenceResolverProvider()
        {
            return new ReferenceResolver();
        }

        class ContractResolver : IContractResolver
        {
            readonly DefaultContractResolver _defaultContractResolver = new DefaultContractResolver();

            /// <inheritdoc />
            public JsonContract ResolveContract(Type type)
            {
                JsonContract jsonContract = _defaultContractResolver.ResolveContract(type);

                if (type.IsAssignableTo(typeof(IMetadataSchemaProvider)) && jsonContract is JsonObjectContract contract)
                {
                    JsonObjectContract schemaProviderContract = (JsonObjectContract)_defaultContractResolver.ResolveContract(typeof(IMetadataSchemaProvider));
                    JsonProperty jsonProperty = schemaProviderContract.Properties[0];
                    jsonProperty.PropertyName = "$defs";
                    contract.Properties.AddProperty(jsonProperty);
                }

                return jsonContract;
            }
        }

        class ReferenceResolver : IReferenceResolver
        {
            private ConcurrentDictionary<string, object> _objects = new ConcurrentDictionary<string, object>();

            /// <inheritdoc />
            public object ResolveReference(object context, string reference)
            {
                _objects.TryGetValue(reference, out var value);
                return value;
            }

            /// <inheritdoc />
            public string GetReference(object context, object value)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public bool IsReferenced(object context, object value)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public void AddReference(object context, string reference, object value)
            {
                _objects.TryAdd(reference, value);
            }
        }

        [Fact]
        public void SerializeDeserializePropertyContainerCollection()
        {
            var initialContainer = CreateTestContainer();

            IPropertyContainer[] containers = new IPropertyContainer[] { initialContainer, initialContainer };

            var json1 = containers.ToJsonWithSystemTextJson();
            var json2 = containers.ToJsonWithNewtonsoftJson();

            var containers1Restored = json1.DeserializeWithSystemTextJson<IReadOnlyCollection<IPropertyContainer>>();
            var containers2Restored = json2.DeserializeWithNewtonsoftJson<IReadOnlyCollection<IPropertyContainer>>();

            containers1Restored.Should().NotBeEmpty();
            containers2Restored.Should().NotBeEmpty();
        }

        [Fact]
        public void PropertyContainerInvalidCast()
        {
            Action invalidCast = () => CreateTestContainer().ToPropertyContainerOfType(typeof(TestMeta));
            invalidCast.Should().Throw<ArgumentException>();

            Action goodCast = () => CreateTestContainer().ToPropertyContainerOfType(typeof(PropertyContainer<TestMeta>));
            goodCast.Should().NotThrow();
        }

        [Fact]
        public void SerializeDeserializeTypedPropertyContainerCollection()
        {
            var initialContainer = CreateTestContainer()
                    .ToPropertyContainerOfType(typeof(PropertyContainer<TestMeta>));

            IPropertyContainer[] containers = new IPropertyContainer[] { initialContainer, initialContainer };

            var json1 = containers.ToJsonWithSystemTextJson(options => options.WriteSchemaOnceForKnownTypes = true);
            var json2 = containers.ToJsonWithNewtonsoftJson();

            var containers1Restored = json1.DeserializeWithSystemTextJson<IReadOnlyCollection<PropertyContainer<TestMeta>>>();
            var containers2Restored = json2.DeserializeWithNewtonsoftJson<IReadOnlyCollection<PropertyContainer<TestMeta>>>();

            containers1Restored.Should().NotBeEmpty();
            containers2Restored.Should().NotBeEmpty();
        }

        [Fact]
        public void UseSchemasRootShouldWorkOnSerializerReuse()
        {
            ComplexObjectUntyped complexObject = new ComplexObjectUntyped()
            {
                Data1 = CreateTestContainer().ToPropertyContainerOfType<PropertyContainer<TestMeta>>(),
                Data2 = CreateTestContainer().ToPropertyContainerOfType<PropertyContainer<TestMeta>>()
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
                .ConfigureJsonForPropertyContainers(options => options.UseSchemasRoot = true);

            Newtonsoft.Json.JsonSerializer jsonSerializer = Newtonsoft.Json.JsonSerializer.Create(settings);

            StringBuilder stringBuilder1 = new StringBuilder();
            StringWriter stringWriter1 = new StringWriter(stringBuilder1);

            jsonSerializer.Serialize(stringWriter1, complexObject);

            StringBuilder stringBuilder2 = new StringBuilder();
            StringWriter stringWriter2 = new StringWriter(stringBuilder2);

            jsonSerializer.Serialize(stringWriter2, complexObject);

            string json1 = stringBuilder1.ToString();
            string json2 = stringBuilder2.ToString();

            json2.Should().Be(json1);
        }

        [Fact]
        public void TestStaticSchema()
        {
            TestMeta testMeta = new TestMeta();
            IPropertySet propertySet = testMeta;
            propertySet.GetProperties().Should().HaveCount(8);

            IObjectSchemaProvider schemaProvider = testMeta;
            schemaProvider.GetObjectSchema().Properties.Should().HaveCount(8);
        }
    }

    internal static class SerializationExtensions
    {
        public static JsonSerializerOptions ConfigureJsonOptions(this JsonSerializerOptions options, Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            options.ConfigureJsonForPropertyContainers(configureSerialization);
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;

            return options;
        }

        public static string ToJsonWithSystemTextJson<T>(this T entity, Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions().ConfigureJsonOptions(configureSerialization);
            return JsonSerializer.Serialize(entity, jsonSerializerOptions);
        }

        public static string ToJsonWithNewtonsoftJson<T>(this T entity, Action<JsonSerializerSettings>? configureJsonSerializerSettings = null, Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings();
            configureJsonSerializerSettings?.Invoke(serializerSettings);
            var jsonSerializerSettings = serializerSettings.ConfigureJsonForPropertyContainers(configureSerialization);
            return JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings);
        }

        public static T DeserializeWithSystemTextJson<T>(this string json, Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions().ConfigureJsonOptions(configureSerialization);
            T? restored = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
            return restored;
        }

        public static T DeserializeWithSystemTextJson<T>(this string json, JsonSerializerOptions jsonSerializerOptions)
        {
            T? restored = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
            return restored;
        }

        public static T DeserializeWithNewtonsoftJson<T>(
            this string json,
            JsonSerializerSettings? jsonSerializerSettings = null,
            Action<JsonSerializerSettings>? configureJsonSerializerSettings = null,
            Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = new JsonSerializerSettings()
                    .ConfigureJsonForPropertyContainers(configureSerialization);
                configureJsonSerializerSettings?.Invoke(jsonSerializerSettings);
            }

            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }
    }

    public class TestMeta : IStaticSchema
    {
        public static readonly IProperty<string> StringProperty = new Property<string>("StringProperty");
        public static readonly IProperty<int> IntProperty = new Property<int>("IntProperty");
        public static readonly IProperty<int?> NullableIntProperty = new Property<int?>("NullableIntProperty");
        public static readonly IProperty<double> DoubleProperty = new Property<double>("DoubleProperty");
        public static readonly IProperty<LocalDate> DateProperty = new Property<LocalDate>("DateProperty");
        public static readonly IProperty<bool> BoolProperty = new Property<bool>("BoolProperty");

        public static readonly IProperty<string[]> StringArray = new Property<string[]>("StringArray");
        public static readonly IProperty<int[]> IntArray = new Property<int[]>("IntArray");
    }

    public class aaaaa
    {
        public static ITypeMapper Create()
        {
            return new JsonTypeMapper();
        }
    }


}

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Metadata.Diff;
using MicroElements.Metadata.Formatters;
using MicroElements.Metadata.NewtonsoftJson;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using MicroElements.Metadata.SystemTextJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MicroElements.Metadata.Tests.Serialization
{
    public class FormatTests
    {
        [Fact]
        public void FormatValueTest()
        {
            SerializeValue<string>(null).Should().Be(null);
            SerializeValue<string>("text").Should().Be("text");
            SerializeValue<int>(42).Should().Be("42");
            SerializeValue<double>(42.7).Should().Be("42.7");
            SerializeValue<LocalDate>(new LocalDate(2020, 11, 26)).Should().Be("2020-11-26");
            SerializeValue<LocalDateTime>(new LocalDateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
            SerializeValue<DateTime>(new DateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
        }

        [Fact]
        public void FormatValueTest2()
        {
            SerializeValue2<string>(null).Should().Be(null);
            SerializeValue2<string>("text").Should().Be("text");
            SerializeValue2<int>(42).Should().Be("42");
            SerializeValue2<double>(42.7).Should().Be("42.7");
            SerializeValue2<LocalDate>(new LocalDate(2020, 11, 26)).Should().Be("2020-11-26");
            SerializeValue2<LocalDateTime>(new LocalDateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
            SerializeValue2<DateTime>(new DateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
        }

        [Fact]
        public void FormatValueTest3()
        {
            SerializeValue2<string>(null).Should().Be(null);
            SerializeValue2<string>("text").Should().Be("text");
            SerializeValue2<int>(42).Should().Be("42");
            SerializeValue2<double>(42.7).Should().Be("42.7");
            SerializeValue2<LocalDate>(new LocalDate(2020, 11, 26)).Should().Be("2020-11-26");
            SerializeValue2<LocalDateTime>(new LocalDateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
            SerializeValue2<DateTime>(new DateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
        }

        public string? SerializeValue2<T>(T value) =>
            Formatter.FullToStringFormatter.Format(value, typeof(T));

        public string? SerializeValue<T>(T value) =>
            new DefaultMapperSettings().SerializeValue(typeof(T), value);

        public object? DeserializeValue<T>(string text) => 
            new DefaultMapperSettings().DeserializeValue(typeof(T), text).GetValueOrThrow(allowNullResult: true);

        [Fact]
        public void GetTypeNameTests()
        {
            var mapperSettings = new DefaultMapperSettings();

            mapperSettings.GetTypeName(typeof(string)).Should().Be("string");

            mapperSettings.GetTypeName(typeof(int)).Should().Be("int");
            mapperSettings.GetTypeName(typeof(int?)).Should().Be("int?");

            mapperSettings.GetTypeName(typeof(double)).Should().Be("double");
            mapperSettings.GetTypeName(typeof(double?)).Should().Be("double?");

            mapperSettings.GetTypeName(typeof(float)).Should().Be("float");
            mapperSettings.GetTypeName(typeof(float?)).Should().Be("float?");

            mapperSettings.GetTypeName(typeof(decimal)).Should().Be("decimal");
            mapperSettings.GetTypeName(typeof(decimal?)).Should().Be("decimal?");

            mapperSettings.GetTypeName(typeof(DateTime)).Should().Be("DateTime");
            mapperSettings.GetTypeName(typeof(DateTime?)).Should().Be("DateTime?");

            mapperSettings.GetTypeName(typeof(LocalDate)).Should().Be("LocalDate");
            mapperSettings.GetTypeName(typeof(LocalDate?)).Should().Be("LocalDate?");

            mapperSettings.GetTypeName(typeof(LocalDateTime)).Should().Be("LocalDateTime");
            mapperSettings.GetTypeName(typeof(LocalDateTime?)).Should().Be("LocalDateTime?");
        }

        [Fact]
        public void GetTypeByNameTests()
        {
            var mapperSettings = new DefaultMapperSettings();

            mapperSettings.GetTypeByName("string").Should().Be(typeof(string));

            mapperSettings.GetTypeByName("int").Should().Be(typeof(int));
            mapperSettings.GetTypeByName("int?").Should().Be(typeof(int?));

            mapperSettings.GetTypeByName("double").Should().Be(typeof(double));
            mapperSettings.GetTypeByName("double?").Should().Be(typeof(double?));

            mapperSettings.GetTypeByName("float").Should().Be(typeof(float));
            mapperSettings.GetTypeByName("float?").Should().Be(typeof(float?));

            mapperSettings.GetTypeByName("decimal").Should().Be(typeof(decimal));
            mapperSettings.GetTypeByName("decimal?").Should().Be(typeof(decimal?));

            mapperSettings.GetTypeByName("DateTime").Should().Be(typeof(DateTime));
            mapperSettings.GetTypeByName("DateTime?").Should().Be(typeof(DateTime?));

            mapperSettings.GetTypeByName("LocalDate").Should().Be(typeof(LocalDate));
            mapperSettings.GetTypeByName("LocalDate?").Should().Be(typeof(LocalDate?));

            mapperSettings.GetTypeByName("LocalDateTime").Should().Be(typeof(LocalDateTime));
            mapperSettings.GetTypeByName("LocalDateTime?").Should().Be(typeof(LocalDateTime?));
        }

        [Fact]
        public void DeserializeValueTest()
        {
            DeserializeValue<string>("null").Should().Be(null);
            DeserializeValue<string>("text").Should().Be("text");
            DeserializeValue<int>("42").Should().Be(42);
            DeserializeValue<double>("42.7").Should().Be(42.7);
            DeserializeValue<LocalDate>("2020-11-26").Should().Be(new LocalDate(2020, 11, 26));
            DeserializeValue<LocalDateTime>("2020-11-26T15:27:17").Should().Be(new LocalDateTime(2020, 11, 26, 15, 27, 17));
            DeserializeValue<DateTime>("2020-11-26T15:27:17").Should().Be(new DateTime(2020, 11, 26, 15, 27, 17));
        }

        [Theory]
        [InlineData("NullString", null, "string")]
        [InlineData("Text", "text", "string")]

        [InlineData("IntValue", "42", "System.Int32", "int")]
        [InlineData("IntValue", "42", "int")]
        [InlineData("IntValue", "42", "int?")]
        [InlineData("IntValue", null, "int?")]

        [InlineData("DoubleValue", "42.7", "double")]
        [InlineData("DoubleValue", "42.7", "double?")]
        [InlineData("DoubleValue", null, "double?")]

        [InlineData("LocalDate", "2020-11-26", "NodaTime.LocalDate", "LocalDate")]
        [InlineData("LocalDate", "2020-11-26", "LocalDate")]
        [InlineData("LocalDate", null, "LocalDate?")]

        [InlineData("LocalDateTime", "2020-11-26T15:27:17", "NodaTime.LocalDateTime", "LocalDateTime")]
        [InlineData("LocalDateTime", "2020-11-26T15:27:17", "LocalDateTime")]
        [InlineData("LocalDateTime", null, "LocalDateTime?")]

        [InlineData("DateTime", "2020-11-26T15:27:17", "System.DateTime", "DateTime")]
        [InlineData("DateTime", "2020-11-26T15:27:17", "DateTime")]
        [InlineData("DateTime", null, "DateTime?")]

        [InlineData("StringArray", "[a1, a2]", "string[]")]
        [InlineData("IntArray", "[1, 2]", "int[]")]

        public void DeserializeProperty(string name, string value, string type, string? type2 = null)
        {
            var mapperSettings = new DefaultMapperSettings();
            var messageList = new ConcurrentMessageList<Message>();

            new PropertyValueContract { Name = name, Value = value, Type = type }
                .ToModel(mapperSettings, messageList)
                .ToContract(mapperSettings)
                .Should().BeEquivalentTo(new PropertyValueContract { Name = name, Value = value, Type = type2 ?? type });
        }

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

        [Fact]
        public void SerializeDeserializePropertyContainer()
        {
            var initialContainer = CreateTestContainer();

            var propertyContainerContract = initialContainer.ToContract(new DefaultMapperSettings());
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

        public interface IMetadataSchemaProvider : IMetadataProvider
        {
            [JsonProperty(PropertyName = "$defs")]
            public ISchemaRepository Schemas
            {
                get
                {
                    ISchemaRepository? schemaRepository = this.GetMetadata<ISchemaRepository>();
                    if (schemaRepository == null)
                    {
                        schemaRepository = new SchemaRepository();
                        this.SetMetadata(schemaRepository);
                    }

                    return schemaRepository;
                }
                set
                {
                    this.SetMetadata(value);
                }
            }
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
  },
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
  }
}";
            var propertyContainer = CreateTestContainer().ToPropertyContainerOfType<PropertyContainer<TestMeta>>();
            ComplexObject complexObject = new ComplexObject()
            {
                Data1 = propertyContainer,
                Data2 = propertyContainer,
            };
            string jsonWithNewtonsoftJson = complexObject.ToJsonWithNewtonsoftJson(configureJsonSerializerSettings: settings =>
            {
                settings.ReferenceResolverProvider = ReferenceResolverProvider;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                settings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
            });

            var container = json.DeserializeWithNewtonsoftJson<ComplexObjectUntyped>(configureJsonSerializerSettings: settings =>
            {
                settings.ReferenceResolverProvider = ReferenceResolverProvider;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.All;
                settings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
                settings.ContractResolver = new ContractResolver();
            });
        }

        private IReferenceResolver? ReferenceResolverProvider()
        {
            return new ReferenceResolver();
        }

        class ContractResolver : IContractResolver
        {
            DefaultContractResolver defaultContractResolver = new DefaultContractResolver();

            /// <inheritdoc />
            public JsonContract ResolveContract(Type type)
            {
                JsonContract jsonContract = defaultContractResolver.ResolveContract(type);

                if (type.IsAssignableTo<IMetadataSchemaProvider>() && jsonContract is JsonObjectContract contract)
                {
                    JsonObjectContract resolveContract = (JsonObjectContract)defaultContractResolver.ResolveContract(typeof(IMetadataSchemaProvider));
                    contract.Properties.AddProperty(resolveContract.Properties[0]);
                }

                return jsonContract;
            }
        }

        class ReferenceResolver : IReferenceResolver
        {
            /// <inheritdoc />
            public object ResolveReference(object context, string reference)
            {
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void SerializeDeserializePropertyContainerCollection()
        {
            var initialContainer = CreateTestContainer();

            IPropertyContainer[] containers = new IPropertyContainer[] {initialContainer, initialContainer};

            var json1 = containers.ToJsonWithSystemTextJson();
            var json2 = containers.ToJsonWithNewtonsoftJson();

            var containers1Restored = json1.DeserializeWithSystemTextJson<IReadOnlyCollection<IPropertyContainer>>();
            var containers2Restored = json2.DeserializeWithNewtonsoftJson<IReadOnlyCollection<IPropertyContainer>>();

            containers1Restored.Should().NotBeEmpty();
            containers2Restored.Should().NotBeEmpty();
        }

        [Fact]
        public void SerializeDeserializeTypedPropertyContainerCollection()
        {
            var initialContainer = CreateTestContainer()
                    .ToPropertyContainerOfType(typeof(PropertyContainer<TestMeta>));

            IPropertyContainer[] containers = new IPropertyContainer[] { initialContainer, initialContainer };

            var json1 = containers.ToJsonWithSystemTextJson();
            var json2 = containers.ToJsonWithNewtonsoftJson();

            var containers1Restored = json1.DeserializeWithSystemTextJson<IReadOnlyCollection<PropertyContainer<TestMeta>>>();
            var containers2Restored = json2.DeserializeWithNewtonsoftJson<IReadOnlyCollection<PropertyContainer<TestMeta>>>();

            containers1Restored.Should().NotBeEmpty();
            containers2Restored.Should().NotBeEmpty();
        }

        [Fact]
        public void FormatDateTime()
        {
            // Sortable
            new FormattableFormatter(type => type == typeof(DateTime), "s")
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 123), typeof(DateTime))
                .Should().Be("2021-01-23T17:15:49");

            DateTimeFormatter.IsoShort
                .Format(new DateTime(2021, 01, 23))
                .Should().Be("2021-01-23");

            DateTimeFormatter.IsoShort
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 123))
                .Should().Be("2021-01-23T17:15:49.123");

            DateTimeFormatter.IsoShort
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 10))
                .Should().Be("2021-01-23T17:15:49.01");

            DateTimeFormatter.IsoShort
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 000))
                .Should().Be("2021-01-23T17:15:49");


            Formatter.FullRecursiveFormatter
                .TryFormat(new LocalDate(2021, 01, 23))
                .Should().Be("2021-01-23");

            Formatter.FullRecursiveFormatter
                .TryFormat(new LocalDateTime(2021, 01, 23, 17, 15, 49, 123))
                .Should().Be("2021-01-23T17:15:49.123");

            Formatter.FullRecursiveFormatter
                .TryFormat(new LocalDateTime(2021, 01, 23, 17, 15, 49, 10))
                .Should().Be("2021-01-23T17:15:49.01");

            Formatter.FullRecursiveFormatter
                .TryFormat(new LocalDateTime(2021, 01, 23, 17, 15, 49, 000))
                .Should().Be("2021-01-23T17:15:49");

            Formatter.FullRecursiveFormatter
                .TryFormat(new DateTimeOffset(2021, 01, 23, 17, 15, 49, 123, TimeSpan.FromHours(4)))
                .Should().Be("2021-01-23T17:15:49.123+04:00");

            DateTimeOffsetFormatter.IgnoringOffset
                .TryFormat(new DateTimeOffset(2021, 01, 23, 17, 15, 49, 123, TimeSpan.FromHours(4)))
                .Should().Be("2021-01-23T17:15:49.123");
        }

        [Fact]
        public void NullableIntegerFormatting()
        {
            double? number = 5.1;
            object boxedNumber = number;
            DefaultToStringFormatter.Instance
                .Format(boxedNumber)
                .Should().Be("5.1");

            double? number2 = null;
            object boxedNumber2 = number2;
            DefaultToStringFormatter.Instance
                .Format(boxedNumber2)
                .Should().Be(null);
        }

        [Fact]
        public void RecursiveFormatting()
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();

            list.Add(new KeyValuePair<string, object>("Key1", new LocalDate(2021, 01, 23)));
            list.Add(new KeyValuePair<string, object>("Key2", new []{"a1", "a2"}));
            list.Add(new KeyValuePair<string, object>("Key3", ("Internal", 5)));

            IValueFormatter fullToStringFormatter = Formatter.FullRecursiveFormatter;
            string? formattedValue = fullToStringFormatter.TryFormat(list);
            formattedValue.Should().Be("[(Key1: 2021-01-23), (Key2: [a1, a2]), (Key3: (Internal, 5))]");
        }

        [Fact]
        public void MetadataJsonSerializationOptions_should_be_copied_properly()
        {
            Fixture fixture = new Fixture();
            fixture.Customizations.Add(
                new TypeRelay(
                    typeof(ITypeMapper),
                    typeof(DefaultTypeMapper)));
            var options = fixture.Create<MetadataJsonSerializationOptions>();
            var optionsCopy = options.Copy();

            optionsCopy.Should().BeEquivalentTo(options);
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
            var jsonSerializerSettings = new JsonSerializerSettings().ConfigureJsonForPropertyContainers(configureSerialization);
            return JsonConvert.SerializeObject(entity, Formatting.Indented, jsonSerializerSettings);
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
            Action<JsonSerializerSettings>? configureJsonSerializerSettings = null,
            Action<MetadataJsonSerializationOptions>? configureSerialization = null)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
                .ConfigureJsonForPropertyContainers(configureSerialization);
            configureJsonSerializerSettings?.Invoke(jsonSerializerSettings);

            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }
    }

    public class TestMeta : IStaticPropertySet
    {
        public static readonly IProperty<string> StringProperty = new Property<string>("StringProperty");
        public static readonly IProperty<int> IntProperty = new Property<int>("IntProperty");
        public static readonly IProperty<int?> NullableIntProperty = new Property<int?>("NullableIntProperty");
        public static readonly IProperty<double> DoubleProperty = new Property<double>("DoubleProperty");
        public static readonly IProperty<LocalDate> DateProperty = new Property<LocalDate>("DateProperty");
        public static readonly IProperty<bool> BoolProperty = new Property<bool>("BoolProperty");

        public static readonly IProperty<string[]> StringArray = new Property<string[]>("StringArray");
        public static readonly IProperty<int[]> IntArray = new Property<int[]>("IntArray");

        public static IPropertySet PropertySet { get; } = new PropertySet(GetProperties());

        public static IEnumerable<IProperty> GetProperties()
        {
            yield return StringProperty;
            yield return IntProperty;
        }
    }
}

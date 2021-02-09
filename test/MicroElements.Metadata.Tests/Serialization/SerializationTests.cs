using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.Diff;
using MicroElements.Metadata.Formatters;
using MicroElements.Metadata.NewtonsoftJson;
using MicroElements.Metadata.Serialization;
using MicroElements.Metadata.SystemTextJson;
using Newtonsoft.Json;
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
            Formatter.DefaultToStringFormatter.Format(value, typeof(T));

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
        [InlineData("NullString", "null", "string")]
        [InlineData("Text", "text", "string")]

        [InlineData("IntValue", "42", "System.Int32", "int")]
        [InlineData("IntValue", "42", "int")]
        [InlineData("IntValue", "42", "int?")]
        [InlineData("IntValue", "null", "int?")]

        [InlineData("DoubleValue", "42.7", "double")]
        [InlineData("DoubleValue", "42.7", "double?")]
        [InlineData("DoubleValue", "null", "double?")]

        [InlineData("LocalDate", "2020-11-26", "NodaTime.LocalDate", "LocalDate")]
        [InlineData("LocalDate", "2020-11-26", "LocalDate")]
        [InlineData("LocalDate", "null", "LocalDate?")]

        [InlineData("LocalDateTime", "2020-11-26T15:27:17", "NodaTime.LocalDateTime", "LocalDateTime")]
        [InlineData("LocalDateTime", "2020-11-26T15:27:17", "LocalDateTime")]
        [InlineData("LocalDateTime", "null", "LocalDateTime?")]

        [InlineData("DateTime", "2020-11-26T15:27:17", "System.DateTime", "DateTime")]
        [InlineData("DateTime", "2020-11-26T15:27:17", "DateTime")]
        [InlineData("DateTime", "null", "DateTime?")]

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

        [Fact]
        public void SerializePropertyContainer()
        {
            var propertyContainer = new MutablePropertyContainer()
                .WithValue(TestMeta.StringProperty, "Text")
                .WithValue(TestMeta.IntProperty, 42)
                .WithValue(TestMeta.DoubleProperty, 10.2)
                .WithValue(TestMeta.NullableIntProperty, null)
                .WithValue(TestMeta.DateProperty, new LocalDate(2020, 12, 26))
                .WithValue(TestMeta.StringArray, new [] { "a1", "a2" })
                .WithValue(TestMeta.IntArray, new[] { 1, 2 })
                ;

            var propertyContainerContract = propertyContainer.ToContract(new DefaultMapperSettings());
            propertyContainerContract.Should().NotBeNull();

            var contractJson = propertyContainerContract.ToJsonWithSystemTextJson();

            var json1 = propertyContainer.ToJsonWithSystemTextJson();
            var container1 = JsonSerializer.Deserialize<PropertyContainer>(json1, new JsonSerializerOptions().ConfigureJsonOptions());

            var json2 = propertyContainer.ToJsonWithNewtonsoftJson();
            var container2 = json2.DeserializeWithNewtonsoftJson<IPropertyContainer>();

            ObjectDiff objectDiff = MetadataComparer.GetDiff(container1, container2);
            objectDiff.Diffs.Should().BeEmpty();
        }

        [Fact]
        public void FormatDateTime()
        {
            DateTimeIsoFormatter.Instance
                .Format(new DateTime(2021, 01, 23))
                .Should().Be("2021-01-23");

            DateTimeIsoFormatter.Instance
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 123))
                .Should().Be("2021-01-23T17:15:49");

            new FormattableFormatter(type => type == typeof(DateTime), "s")
                .Format(new DateTime(2021, 01, 23, 17, 15, 49, 123), typeof(DateTime))
                .Should().Be("2021-01-23T17:15:49");

            Formatter.DefaultToStringFormatter
                .Format(new LocalDate(2021, 01, 23))
                .Should().Be("2021-01-23");

            Formatter.DefaultToStringFormatter
                .Format(new LocalDateTime(2021, 01, 23, 17, 15, 49, 123))
                .Should().Be("2021-01-23T17:15:49");

        }
    }

    internal static class SerializationExtensions
    {
        public static JsonSerializerOptions ConfigureJsonOptions(this JsonSerializerOptions options)
        {
            options.ConfigureJsonForPropertyContainers();
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;

            return options;
        }

        public static string ToJsonWithSystemTextJson<T>(this T entity)
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions().ConfigureJsonOptions();
            return JsonSerializer.Serialize(entity, jsonSerializerOptions);
        }

        public static string ToJsonWithNewtonsoftJson<T>(this T entity)
        {
            var jsonSerializerSettings = new JsonSerializerSettings().ConfigureJsonForPropertyContainers();
            return JsonConvert.SerializeObject(entity, Formatting.Indented, jsonSerializerSettings);
        }

        public static T DeserializeWithNewtonsoftJson<T>(this string json)
        {
            var jsonSerializerSettings = new JsonSerializerSettings().ConfigureJsonForPropertyContainers();
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }
    }

    public static class TestMeta
    {
        public static readonly IProperty<string> StringProperty = new Property<string>("StringProperty");
        public static readonly IProperty<int> IntProperty = new Property<int>("IntProperty");
        public static readonly IProperty<int?> NullableIntProperty = new Property<int?>("NullableIntProperty");
        public static readonly IProperty<double> DoubleProperty = new Property<double>("DoubleProperty");
        public static readonly IProperty<LocalDate> DateProperty = new Property<LocalDate>("DateProperty");

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

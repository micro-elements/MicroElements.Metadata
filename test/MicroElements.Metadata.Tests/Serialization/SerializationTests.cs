using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.Contracts;
using MicroElements.Shared;
using NodaTime;
using Xunit;

namespace MicroElements.Metadata.Tests.Serialization
{
    public class FormatTests
    {
        [Fact]
        public void FormatValueTest()
        {
            SerializeValue<string>(null).Should().Be("null");
            SerializeValue<string>("text").Should().Be("text");
            SerializeValue<int>(42).Should().Be("42");
            SerializeValue<double>(42.7).Should().Be("42.7");
            SerializeValue<LocalDate>(new LocalDate(2020, 11, 26)).Should().Be("2020-11-26");
            SerializeValue<LocalDateTime>(new LocalDateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
            SerializeValue<DateTime>(new DateTime(2020, 11, 26, 15, 27, 17)).Should().Be("2020-11-26T15:27:17");
        }

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
                .WithValue(TestMeta.IntProperty, 42);

            var propertyContainerContract = propertyContainer.ToContract(new DefaultMapperSettings());
            propertyContainerContract.Should().NotBeNull();
        }
    }

    public static class TestMeta
    {
        public static readonly IProperty<string> StringProperty = new Property<string>("StringProperty");
        public static readonly IProperty<int> IntProperty = new Property<int>("IntProperty");
        

        public static IPropertySet PropertySet { get; } = new PropertySet(GetProperties());

        public static IEnumerable<IProperty> GetProperties()
        {
            yield return StringProperty;
            yield return IntProperty;
        }
    }

    static class ResultExtensions
    {
        [Obsolete("Use from Functional")]
        public static A GetValueOrThrow<A, TErrorCode>(this in Result<A, IError<TErrorCode>> result, bool allowNullResult = false)
            where TErrorCode : notnull
        {
            if (allowNullResult)
                return result.MatchUnsafe((a) => a, (error) => throw error.ToException());

            return result.Match((a) => a, (error) => throw error.ToException());
        }
    }
}

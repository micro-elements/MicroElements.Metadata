using System;
using System.Collections.Generic;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Formatters;
using MicroElements.Metadata.Serialization;
using NodaTime;
using Xunit;

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
        public void RecursiveFormatterBuilder()
        {
            List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();

            list.Add(new KeyValuePair<string, object>("Key1", new LocalDate(2021, 01, 23)));
            list.Add(new KeyValuePair<string, object>("Key2", new[] { "a1", "a2" }));
            list.Add(new KeyValuePair<string, object>("Key3", ("Internal", 5)));

            IValueFormatter fullToStringFormatter = FormatterBuilder
                .Create()
                .WithFormatters(DefaultFormatProvider.Instance)
                .AddStandardFormatters()
                .Build();

            string? formattedValue = fullToStringFormatter.TryFormat(list);
            formattedValue.Should().Be("[(Key1: 2021-01-23), (Key2: [a1, a2]), (Key3: (Internal, 5))]");

            fullToStringFormatter = FormatterBuilder
                .Create()
                .WithFormatters(DefaultFormatProvider.Instance)
                .AddStandardFormatters()
                .ConfigureFormatter<CollectionFormatterSettings>(settings => settings.StartSymbol = "{")
                .ConfigureFormatter<CollectionFormatterSettings>(settings => settings.EndSymbol = "}")
                .ConfigureFormatter<KeyValuePairFormatterSettings>(format => format.Format = "{0}={1}")
                .Build();

            formattedValue = fullToStringFormatter.TryFormat(list);
            formattedValue.Should().Be("{Key1=2021-01-23, Key2={a1, a2}, Key3=(Internal, 5)}");

            new KeyValuePairFormatter()
                .Configure(settings => settings.Format = "{0}={1}")
                .Format(new KeyValuePair<string, object?>("key", "value"))
                .Should().Be("key=value");
        }

        [Fact]
        public void PropertyContainerFormatter1()
        {
            var propertyContainer = new MutablePropertyContainer()
                .WithValue("key1", "value1")
                .WithValue("key2", "value2");

            string format1 = propertyContainer.ToString();
            string format2 = PropertyContainerFormatter.Instance.TryFormat(propertyContainer);
        }
    }
}

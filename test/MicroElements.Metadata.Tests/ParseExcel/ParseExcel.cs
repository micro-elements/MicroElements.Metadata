using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.OpenXml.Excel.Parsing;
using MicroElements.Validation;
using MicroElements.Validation.Rules;
using NodaTime;
using NodaTime.Text;
using Xunit;

namespace MicroElements.Metadata.Tests.ParseExcel
{
    public class ParseExcel
    {
        public class FxSheetRow : PropertyContainer
        {
            public FxSheetRow(IEnumerable<IPropertyValue> sourceValues = null) : base(sourceValues) { }
        }

        public static class FxSheetMeta
        {
            public static readonly IProperty<string> Text = new Property<string>("Text");
            public static readonly IProperty<LocalDate> Date = new Property<LocalDate>("Date");
            public static readonly IProperty<LocalTime> Time = new Property<LocalTime>("Time");
            public static readonly IProperty<DateTime> DateTime = new Property<DateTime>("DateTime");

            public static readonly IProperty<int> Integer = new Property<int>("Integer");
            public static readonly IProperty<double> Double = new Property<double>("Double");

            // Example of optional values
            public static readonly IProperty<int?> OptionalInteger = new Property<int?>("OptionalInteger");
            public static readonly IProperty<double?> OptionalDouble = new Property<double?>("OptionalDouble");
        }

        public class FxSheetParserProvider : ParserProvider
        {
            public int ParseInt(string text) => int.Parse(text);

            public int? ParseOptionalInt(string text) => Prelude.ParseInt(text).MatchUnsafe(value => value, default(int?));

            public double ParseDouble(string text) => double.Parse(text, NumberStyles.Any, CultureInfo.InvariantCulture);

            public double? ParseOptionalDouble(string text) => Prelude.ParseDouble(text, NumberStyles.Any, CultureInfo.InvariantCulture).MatchUnsafe(value => value, default(double?));

            public LocalDate ParseLocalDate(string text) => LocalDatePattern.Iso.Parse(text).Value;

            public LocalTime ParseLocalTime(string text) => LocalTimePattern.ExtendedIso.Parse(text).Value;

            public DateTime ParseDateTime(string text) => DateTime.Parse(text);

            public FxSheetParserProvider()
            {
                Source("Text").Target(FxSheetMeta.Text);

                Source("Date", ParseLocalDate).Target(FxSheetMeta.Date);
                Source2("Date", ParseLocalDate).Target(FxSheetMeta.Date);

                Source("Time", ParseLocalTime).Target(FxSheetMeta.Time);
                Source("DateTime", ParseDateTime).Target(FxSheetMeta.DateTime);

                Source("Integer", ParseInt).Target(FxSheetMeta.Integer);
                Source("Double", ParseDouble).Target(FxSheetMeta.Double);

                Source("Integer", ParseOptionalInt).Target(FxSheetMeta.OptionalInteger);
                Source("Double", ParseOptionalDouble).Target(FxSheetMeta.OptionalDouble);
            }
        }

        //TODO: Example and test
        public class LazyParserProvider : IParserProvider
        {
            public IEnumerable<IPropertyParser> CommonParsers()
            {
                yield return PropertyParser.Source("Text").Target(FxSheetMeta.Text);
            }

            /// <inheritdoc />
            public IEnumerable<IPropertyParser> GetParsers()
            {
                foreach (var propertyParser in CommonParsers())
                    yield return propertyParser;

                yield return PropertyParser.Source("Text").Target(FxSheetMeta.Text);
            }
        }

        public class EnumerableValidatorSample : IValidator
        {
            public IEnumerable<IValidationRule> GetRules()
            {
                yield return FxSheetMeta.Integer.Exists().And().NotDefault().And().ShouldBe(i => i > 5);
                yield return FxSheetMeta.Date.ShouldBe(date => date > LocalDate.MinIsoValue);

                yield return FxSheetMeta.OptionalInteger.NotNull();
            }
        }

        public class FxValidator : AbstractValidator
        {
            public FxValidator()
            {
                Add(FxSheetMeta.Text.NotNull().AsWarning());
            }
        }

        [Fact]
        public void ParseSampleExcel()
        {
            var document = SpreadsheetDocument.Open("ParseExcel/sample.xlsx", false);

            var fxSheetRows = document
                .GetSheet("Sheet1")
                .GetRowsAs<FxSheetRow>(new FxSheetParserProvider())
                .ToArray();

            fxSheetRows.Length.Should().Be(8);

            fxSheetRows[0].GetValue(FxSheetMeta.Text).Should().Be("Text_1");
            fxSheetRows[0].GetValue(FxSheetMeta.Date).Should().Be(new LocalDate(2020, 05, 01));
            fxSheetRows[0].GetValue(FxSheetMeta.Time).Should().Be(new LocalTime(10, 00, 00));
            fxSheetRows[0].GetValue(FxSheetMeta.DateTime).Should().Be(new DateTime(2020, 05, 01, 10, 00, 00, DateTimeKind.Unspecified));
            fxSheetRows[0].GetValue(FxSheetMeta.Integer).Should().Be(1);
            fxSheetRows[0].GetValue(FxSheetMeta.OptionalInteger).Should().Be(1);
            fxSheetRows[0].GetValue(FxSheetMeta.Double).Should().Be(1.01);
            fxSheetRows[0].GetValue(FxSheetMeta.OptionalDouble).Should().Be(1.01);

            fxSheetRows[1].GetValue(FxSheetMeta.Text).Should().Be("Text_2");
            fxSheetRows[1].GetValue(FxSheetMeta.Date).Should().Be(new LocalDate(1900, 01, 01));
            fxSheetRows[1].GetValue(FxSheetMeta.Time).Should().Be(new LocalTime(23, 59, 59));
            fxSheetRows[1].GetValue(FxSheetMeta.DateTime).Should().Be(new DateTime(1900, 01, 01, 23, 59, 59, DateTimeKind.Unspecified));
            fxSheetRows[1].GetValue(FxSheetMeta.Integer).Should().Be(2);
            fxSheetRows[1].GetValue(FxSheetMeta.Double).Should().Be(1.02);

            fxSheetRows[2].GetValue(FxSheetMeta.Text).Should().Be("Empty_Int");
            fxSheetRows[2].GetValue(FxSheetMeta.Integer).Should().Be(default);
            fxSheetRows[2].GetValue(FxSheetMeta.OptionalInteger).Should().Be(null);

            fxSheetRows[3].GetValue(FxSheetMeta.Text).Should().Be("Empty_Double");
            fxSheetRows[3].GetValue(FxSheetMeta.Double).Should().Be(default);
            fxSheetRows[3].GetValue(FxSheetMeta.OptionalDouble).Should().Be(null);

            fxSheetRows[4].GetValue(FxSheetMeta.Text).Should().Be(null);

            fxSheetRows[5].GetValue(FxSheetMeta.Text).Should().Be("Empty_Date");
            fxSheetRows[5].GetValue(FxSheetMeta.Date).Should().Be(default);

            fxSheetRows[6].GetValue(FxSheetMeta.Text).Should().Be("Empty_Time");
            fxSheetRows[6].GetValue(FxSheetMeta.Time).Should().Be(default);

            fxSheetRows[7].GetValue(FxSheetMeta.Text).Should().Be("Empty_DateTime");
            fxSheetRows[7].GetValue(FxSheetMeta.DateTime).Should().Be(default);

            fxSheetRows.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ValidateSampleExcel1()
        {
            var document = SpreadsheetDocument.Open("ParseExcel/sample.xlsx", false);

            var validationErrors = document
                .GetSheet("Sheet1")
                .GetRowsAs<FxSheetRow>(new FxSheetParserProvider())
                .SelectMany(row => row.Validate(new FxValidator()))
                .ToList();

            validationErrors.Should().NotBeNullOrEmpty();
            validationErrors[0].FormattedMessage.Should().Be("Text should not be null.");
            validationErrors[0].Severity.Should().Be(MessageSeverity.Warning);
        }

        [Fact]
        public void ValidateSampleExcel2()
        {
            IEnumerable<IValidationRule> GetRules()
            {
                yield return FxSheetMeta.Text.NotNull().AsWarning();
            }

            var document = SpreadsheetDocument.Open("ParseExcel/sample.xlsx", false);

            var validationErrors = document
                .GetSheet("Sheet1")
                .GetRowsAs<FxSheetRow>(new FxSheetParserProvider())
                .SelectMany(row => row.Validate(GetRules()))
                .ToList();

            validationErrors.Should().HaveCount(1);
            validationErrors[0].FormattedMessage.Should().Be("Text should not be null.");
            validationErrors[0].Severity.Should().Be(MessageSeverity.Warning);
        }

        [Fact]
        public void ValidateSampleExcel3()
        {
            IEnumerable<IValidationRule> GetRules()
            {
                yield return FxSheetMeta.Text.NotNull().AsWarning();
            }

            var document = SpreadsheetDocument.Open("ParseExcel/sample.xlsx", false);

            var validatedRows = document
                .GetSheet("Sheet1")
                .GetRowsAs<FxSheetRow>(new FxSheetParserProvider())
                .Select(row => row.ToValidationResult(GetRules()))
                .ToList();

            validatedRows[3].IsValid().Should().BeTrue();

            validatedRows[4].IsValid().Should().BeFalse();
            validatedRows[4].ValidationMessages.First().FormattedMessage.Should().Be("Text should not be null.");
            validatedRows[4].ValidationMessages.First().Severity.Should().Be(MessageSeverity.Warning);
        }

        [Fact]
        public void CellReferenceTest()
        {
            ExcelParsingExtensions.GetCellReference(0, 0, zeroBased: true).Should().Be(new StringValue("A1"));
            ExcelParsingExtensions.GetCellReference(1, 1, zeroBased: false).Should().Be(new StringValue("A1"));
        }
    }
}

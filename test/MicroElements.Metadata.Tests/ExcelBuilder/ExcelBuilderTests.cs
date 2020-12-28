using System;
using DocumentFormat.OpenXml.Spreadsheet;
using FluentAssertions;
using MicroElements.Functional;
using MicroElements.Metadata.OpenXml.Excel;
using MicroElements.Metadata.OpenXml.Excel.Reporting;
using NodaTime;
using NodaTime.Extensions;
using Xunit;

namespace MicroElements.Metadata.Tests.ExcelBuilder
{
    public class ExcelBuilderTests
    {
        public static class Sheet1Meta
        {
            public static readonly IProperty<string> Name = new Property<string>("Name");
            public static readonly IProperty<int> Age = new Property<int>("Age");
            public static readonly IProperty<LocalDate> Date = new Property<LocalDate>("Date");
            public static readonly IProperty<LocalDate> SerialDate = new Property<LocalDate>("SerialDate");
        }

        public class Sheet1Report : ReportProvider
        {
            /// <inheritdoc />
            public Sheet1Report(string reportName = "Sheet1") : base(reportName)
            {
                Add(Sheet1Meta.Name).SetExcelType(CellValues.SharedString);
                Add(Sheet1Meta.Age).SetExcelType(CellValues.Number);
                Add(Sheet1Meta.Date).SetDateIsoFormat();
                Add(Sheet1Meta.SerialDate).SetDateSerialFormat();
            }
        }

        [Fact]
        public void build_excel()
        {
            IPropertyContainer[] rows =
            {
                new MutablePropertyContainer()
                    .WithValue(Sheet1Meta.Name, "Alex")
                    .WithValue(Sheet1Meta.Age, 42)
                    .WithValue(Sheet1Meta.Date, DateTime.Today.ToLocalDateTime().Date),
                new MutablePropertyContainer()
                    .WithValue(Sheet1Meta.Name, "Helen")
                    .WithValue(Sheet1Meta.Age, 17),
            };

            var documentMetadata = new ExcelDocumentMetadata()
                    .WithValue(ExcelMetadata.DataType, CellValues.SharedString)
                    .WithValue(ExcelMetadata.FreezeTopRow, true)
                    .WithValue(ExcelMetadata.ColumnWidth, 14)
                as ExcelDocumentMetadata;

            var transposed = new ExcelSheetMetadata()
                    .WithValue(ExcelMetadata.Transpose, true)
                as ExcelSheetMetadata;

            ExcelReportBuilder
                .Create("build_excel.xlsx", documentMetadata)
                .AddReportSheet(new Sheet1Report("Sheet1"), rows)
                .AddReportSheet(new Sheet1Report("Sheet2").SetMetadata(transposed), rows)
                .SaveAndClose();
        }

        [Fact]
        public void build_excel_with_nulls()
        {
            IPropertyContainer[] rows =
            {
                new MutablePropertyContainer(),
            };

            var documentMetadata = new ExcelDocumentMetadata()
                    .WithValue(ExcelMetadata.DataType, CellValues.SharedString)
                    .WithValue(ExcelMetadata.FreezeTopRow, true)
                    .WithValue(ExcelMetadata.ColumnWidth, 14);

            ExcelReportBuilder
                .Create("build_excel.xlsx", documentMetadata)
                .AddReportSheet(new Sheet1Report("Sheet1"), rows)
                .SaveAndClose();
        }

        [Fact]
        public void build_excel_with_generated_class()
        {
            //new GeneratedClass().CreatePackage("build_excel2.xlsx");
        }

        [Fact]
        public void date_types_serialization()
        {
            new LocalDate(1900, 01, 01).ToExcelSerialDateAsString().Should().Be("1");
            new LocalDate(1900, 02, 28).ToExcelSerialDateAsString().Should().Be("59");
            //new DateTime(1900, 02, 29).ToSerialDate().Should().Be("60"); // valid in excel but not valid in dotnet (Gregorian calendar has no this date)
            new LocalDate(1900, 03, 01).ToExcelSerialDateAsString().Should().Be("61");
            new LocalDate(2020, 12, 22).ToExcelSerialDateAsString().Should().Be("44187");
            new LocalDateTime(2020, 12, 22, 12, 0,0).ToExcelSerialDateAsString().Should().Be("44187.5");
            new DateTime(2020, 12, 22, 12, 0, 0).ToExcelSerialDateAsString().Should().Be("44187.5");

            new LocalTime(12, 00, 00).ToExcelSerialDateAsString().Should().Be("0.5");
            Duration.FromHours(36).ToExcelSerialDateAsString().Should().Be("1.5");

            MapToSerialAndBack(new LocalDate(1900, 01, 01), dt => dt.ToLocalDateTime().Date);
            MapToSerialAndBack(new LocalDate(1900, 02, 28), dt => dt.ToLocalDateTime().Date);
            MapToSerialAndBack(new LocalDate(1900, 03, 01), dt => dt.ToLocalDateTime().Date);
            MapToSerialAndBack(new LocalDate(2020, 12, 22), dt => dt.ToLocalDateTime().Date);

            MapToSerialAndBack(new DateTime(2020, 12, 22), dt => dt);
            MapToSerialAndBack(new DateTime(1900, 02, 28), dt => dt);
            MapToSerialAndBack(new DateTime(1900, 03, 01), dt => dt);
            MapToSerialAndBack(new DateTime(2020, 12, 22), dt => dt);
        }

        public void MapToSerialAndBack<T>(T initial, Func<DateTime, T> fromDateTime)
        {
            T restored = Prelude
                .ParseDouble(initial.ToExcelSerialDateAsString())
                .Map(d => d.FromExcelSerialDate())
                .Map(fromDateTime)
                .GetValueOrThrow();

            restored.Should().Be(initial);
        }
    }
}

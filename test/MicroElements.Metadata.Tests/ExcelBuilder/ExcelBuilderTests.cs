using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Reporting.Excel;
using Xunit;

namespace MicroElements.Metadata.Tests.ExcelBuilder
{
    public class ExcelBuilderTests
    {
        public static class Sheet1Meta
        {
            public static readonly IProperty<string> Name = new Property<string>("Name");
            public static readonly IProperty<int> Age = new Property<int>("Age");
        }

        public class Sheet1Report : ReportProvider
        {
            /// <inheritdoc />
            public Sheet1Report(string reportName = "Sheet1") : base(reportName)
            {
                Add(Sheet1Meta.Name).SetExcelType(CellValues.SharedString);
                Add(Sheet1Meta.Age).SetExcelType(CellValues.Number);
            }
        }

        [Fact]
        public void build_excel()
        {
            IPropertyContainer[] rows =
            {
                new MutablePropertyContainer()
                    .WithValue(Sheet1Meta.Name, "Alex")
                    .WithValue(Sheet1Meta.Age, 42),
                new MutablePropertyContainer()
                    .WithValue(Sheet1Meta.Name, "Helen")
                    .WithValue(Sheet1Meta.Age, 17),
            };

            var documentMetadata = new ExcelDocumentMetadata()
                    .WithValue(ExcelMetadata.DataType, CellValues.SharedString)
                    .WithValue(ExcelMetadata.FreezeTopRow, true)
                    .WithValue(ExcelMetadata.ColumnWidth, 14)
                as ExcelDocumentMetadata;

            ExcelReportBuilder
                .Create("build_excel.xlsx", documentMetadata)
                .AddReportSheet(new Sheet1Report(), rows)
                .SaveAndClose();
        }
        [Fact]
        public void build_excel_with_generated_class()
        {
            //new GeneratedClass().CreatePackage("build_excel2.xlsx");
        }

    }

    public static class ExcelProviderExtensions
    {
        public static TMetadataProvider SetExcelType<TMetadataProvider>(this TMetadataProvider metadataProvider, CellValues cellValues)
            where TMetadataProvider : IMetadataProvider
        {
            metadataProvider.ConfigureMetadata<ExcelColumnMetadata>(metadata => metadata.SetValue(ExcelMetadata.DataType, cellValues));
            return metadataProvider;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;
using Border = DocumentFormat.OpenXml.Spreadsheet.Border;
using BottomBorder = DocumentFormat.OpenXml.Spreadsheet.BottomBorder;
using Column = DocumentFormat.OpenXml.Spreadsheet.Column;
using Columns = DocumentFormat.OpenXml.Spreadsheet.Columns;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;
using FontSize = DocumentFormat.OpenXml.Spreadsheet.FontSize;
using LeftBorder = DocumentFormat.OpenXml.Spreadsheet.LeftBorder;
using RightBorder = DocumentFormat.OpenXml.Spreadsheet.RightBorder;
using TopBorder = DocumentFormat.OpenXml.Spreadsheet.TopBorder;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Excel report builder.
    /// </summary>
    public class ExcelReportBuilder
    {
        private readonly ExcelDocumentMetadata _documentMetadata;
        private readonly DocumentContext _documentContext;

        private ExcelSheetMetadata _defaultSheetMetadata = new ExcelSheetMetadata();
        private ExcelColumnMetadata _defaultColumnMetadata = new ExcelColumnMetadata();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelReportBuilder"/> class.
        /// </summary>
        /// <param name="document">Excel document.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        public ExcelReportBuilder(SpreadsheetDocument document, ExcelDocumentMetadata documentMetadata)
        {
            _documentMetadata = documentMetadata ?? new ExcelDocumentMetadata();
            _documentContext = InitDocument(document.AssertArgumentNotNull(nameof(document)));
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="outFilePath">Output file name.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(string outFilePath, ExcelDocumentMetadata documentMetadata = null)
        {
            outFilePath.AssertArgumentNotNull(nameof(outFilePath));

            SpreadsheetDocument document = SpreadsheetDocument.Create(outFilePath, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document, documentMetadata);
            return builder;
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="targetStream">Output stream.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(Stream targetStream, ExcelDocumentMetadata documentMetadata = null)
        {
            targetStream.AssertArgumentNotNull(nameof(targetStream));

            SpreadsheetDocument document = SpreadsheetDocument.Create(targetStream, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document, documentMetadata);
            return builder;
        }

        /// <summary>
        /// Sets default sheet metadata.
        /// </summary>
        /// <param name="sheetMetadata">Default sheet metadata.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder WithDefaultSheetMetadata(ExcelSheetMetadata sheetMetadata)
        {
            _defaultSheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
            return this;
        }

        /// <summary>
        /// Sets default column metadata.
        /// </summary>
        /// <param name="columnMetadata">Default column metadata.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder WithDefaultColumnMetadata(ExcelColumnMetadata columnMetadata)
        {
            _defaultColumnMetadata = columnMetadata.AssertArgumentNotNull(nameof(columnMetadata));
            return this;
        }

        /// <summary>
        /// Saves data and flushes. Can be called multiple times.
        /// </summary>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder Save()
        {
            _documentContext.WorkbookPart.Workbook.Save();
            return this;
        }

        /// <summary>
        /// Saves data to output document and closes document.
        /// </summary>
        public void SaveAndClose()
        {
            _documentContext.WorkbookPart.Workbook.Save();
            _documentContext.Document.Close();
        }

        /// <summary>
        /// Default document initialization.
        /// </summary>
        private DocumentContext InitDocument(SpreadsheetDocument document)
        {
            if (document.WorkbookPart == null)
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();

                Workbook workbook = new Workbook();
                workbook.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

                workbookPart.Workbook = workbook;

                BookViews bookViews = new BookViews();
                WorkbookView workbookView = new WorkbookView() { XWindow = -120, YWindow = -120, WindowWidth = (UInt32Value)19440U, WindowHeight = (UInt32Value)15000U };
                bookViews.Append(workbookView);

                Sheets sheets = new Sheets();

                workbook.AppendChild(bookViews);
                workbook.AppendChild(sheets);
            }

            var documentContext = new DocumentContext(document, _documentMetadata);

            // Init Stylesheet.
            InitStylesheet(documentContext);

            // Add empty string.
            documentContext.GetOrAddSharedString("");

            // External customization
            var customizeFunc = _documentMetadata?.GetValue(ExcelDocumentMetadata.CustomizeDocument);
            customizeFunc?.Invoke(documentContext);

            return documentContext;
        }

        /// <summary>
        /// Adds new excel sheet.
        /// </summary>
        /// <param name="reportProvider">Report provider.</param>
        /// <param name="reportRows">Report rows.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder AddReportSheet(IReportProvider reportProvider, IEnumerable<IPropertyContainer> reportRows)
        {
            var sheetMetadata = reportProvider.GetMetadata<ExcelSheetMetadata>() ?? _defaultSheetMetadata;

            // Add a WorksheetPart to the WorkbookPart.
            WorkbookPart workbookPart = _documentContext.Document.WorkbookPart;
            uint sheetCount = workbookPart.GetSheetCount();
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>($"sheet{sheetCount+1}");

            var sheetContext = new SheetContext(_documentContext, worksheetPart, sheetMetadata, reportProvider);

            AddSheet(sheetContext);

            AddSheetData(sheetContext, reportRows);

            // External customization
            var customizeFunc = sheetContext.SheetMetadata?.GetValue(ExcelSheetMetadata.CustomizeSheet);
            customizeFunc?.Invoke(sheetContext);

            return this;
        }

        private void AddSheet(SheetContext sheetContext)
        {
            WorkbookPart workbookPart = sheetContext.DocumentContext.WorkbookPart;
            WorksheetPart worksheetPart = sheetContext.WorksheetPart;

            Worksheet worksheet = new Worksheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac xr xr2 xr3" } };
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            worksheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            worksheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            worksheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            worksheet.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            worksheet.AddNamespaceDeclaration("xr3", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");

            worksheetPart.Worksheet = worksheet;

            SheetViews sheetViews = worksheet.GetOrCreateSheetViews();
            SheetView sheetView = new SheetView { TabSelected = true, WorkbookViewId = (UInt32Value)0U, };
            sheetViews.AppendChild(sheetView);

            SheetFormatProperties sheetFormatProperties = new SheetFormatProperties
            {
                DefaultRowHeight = 15D,
                DyDescent = 0.25D,
            };

            ColumnContext CreateColumnContext(IPropertyRenderer renderer) =>
                new ColumnContext(
                    sheetContext,
                    renderer.GetMetadata<ExcelColumnMetadata>() ?? _defaultColumnMetadata,
                    renderer);

            sheetContext.Columns = sheetContext
                .ReportProvider
                .Renderers
                .Select(CreateColumnContext)
                .ToList();

            Columns columns = new Columns();
            if (sheetContext.IsNotTransposed)
            {
                columns = CreateColumns(sheetContext.Columns);
            }

            SheetData sheetData = new SheetData();

            //workSheet.Append(sheetDimension);
            worksheet.Append(sheetViews);
            worksheet.Append(sheetFormatProperties);
            worksheet.Append(columns);
            worksheet.Append(sheetData);
            //workSheet.Append(pageMargins);

            // Append a new worksheet and associate it with the workbook.
            Sheets sheets = workbookPart.Workbook.Sheets;
            uint sheetCount = workbookPart.GetSheetCount();
            Sheet sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetCount + 1,
                Name = sheetContext.ReportProvider.ReportName,
            };

            sheets.Append(sheet);

            bool freezeTopRow = ExcelMetadata.GetFirstDefinedValue(
                ExcelMetadata.FreezeTopRow,
                sheetContext.SheetMetadata,
                sheetContext.DocumentMetadata);

            if (freezeTopRow)
            {
                worksheet.FreezeTopRow(rowNum: 1);
            }

            sheetContext.SheetData = sheetData;
            sheetContext.Sheet = sheet;
        }

        private void AddSheetData(
            SheetContext sheetContext,
            IEnumerable<IPropertyContainer> items)
        {
            SheetData sheetData = sheetContext.SheetData;
            var columns = sheetContext.Columns;

            if (sheetContext.IsNotTransposed)
            {
                // HEADER ROW
                var headerCells = columns.Select(column => ConstructCell(column.PropertyRenderer.TargetName, CellValues.SharedString));
                sheetData.AppendChild(new Row(headerCells));

                // DATA ROWS
                foreach (var item in items.NotNull())
                {
                    var valueCells = columns.Select(renderer => ConstructCell(renderer, item));
                    sheetData.AppendChild(new Row(valueCells));
                }
            }
            else
            {
                // NAME COLUMN
                var headerCells = columns.Select(column => ConstructCell(column.PropertyRenderer.TargetName, CellValues.SharedString));
                Row[] rows = headerCells.Select(headerCell => new Row(headerCell)).ToArray();

                // VALUE COLUMN
                for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
                {
                    var column = columns[rowIndex];
                    foreach (var item in items.NotNull())
                    {
                        ConstructCell(column, item);
                    }
                }

                foreach (var item in items.NotNull())
                {
                    for (var index = 0; index < columns.Count; index++)
                    {
                        var column = columns[index];
                        Row row = rows[index];
                        Cell cell = ConstructCell(column, item);
                        row.AppendChild(cell);
                    }
                }

                foreach (Row row in rows)
                {
                    sheetData.AppendChild(row);
                }
            }
        }

        private Columns CreateColumns(IReadOnlyList<ColumnContext> columns)
        {
            Columns columnsElement = new Columns();
            for (int index = 0; index < columns.Count; index++)
            {
                var columnContext = columns[index];
                uint colNumber = (uint)(index + 1);

                int columnWidth = ExcelMetadata.GetFirstDefinedValue(
                    ExcelMetadata.ColumnWidth,
                    columnContext.ColumnMetadata,
                    columnContext.SheetMetadata,
                    columnContext.DocumentMetadata);

                columnContext.Column = new Column { Min = colNumber, Max = colNumber, Width = columnWidth, CustomWidth = true };

                // External customization
                var customizeFunc = columnContext.ColumnMetadata?.GetValue(ExcelColumnMetadata.CustomizeColumn);
                customizeFunc?.Invoke(columnContext);

                if (columnContext.Column != null)
                    columnsElement.Append(columnContext.Column);
            }

            return columnsElement;
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            string text = value;

            if (dataType == CellValues.SharedString && string.IsNullOrEmpty(text))
            {
                dataType = CellValues.String;
            }

            if (dataType == CellValues.SharedString)
            {
                text = _documentContext.GetOrAddSharedString(value);
            }

            return new Cell
            {
                CellValue = new CellValue(text),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }

        private Cell ConstructCell(ColumnContext columnContext, IPropertyContainer source)
        {
            var propertyRenderer = columnContext.PropertyRenderer;
            string textValue = propertyRenderer.Render(source);

            var cellMetadata = propertyRenderer.GetMetadata<ExcelCellMetadata>();

            CellValues dataType = ExcelMetadata.GetFirstDefinedValue(
                ExcelMetadata.DataType,
                cellMetadata,
                columnContext.ColumnMetadata,
                columnContext.SheetMetadata,
                columnContext.DocumentMetadata);

            Cell cell = ConstructCell(textValue, dataType);

            if (dataType == CellValues.Date)
            {
                cell.StyleIndex = _documentContext.GetCellFormatIndex("Date");

                var isLocalTime = propertyRenderer.PropertyType == typeof(LocalTime) || propertyRenderer.PropertyType == typeof(LocalTime?);
                if (isLocalTime)
                {
                    cell.StyleIndex = _documentContext.GetCellFormatIndex("Time");
                }
            }

            // External customization
            var customizeFunc = cellMetadata?.GetValue(ExcelCellMetadata.CustomizeCell);
            customizeFunc?.Invoke(cell);

            return cell;
        }

        private void InitStylesheet(DocumentContext document)
        {
            // Create "fonts".
            document.AddFont(
                new Font
                {
                    FontName = new FontName() {Val = "Calibri"},
                    FontSize = new FontSize() {Val = 11},
                    FontFamilyNumbering = new FontFamilyNumbering() {Val = 2},
                }, "Default");

            // Create "fills".
            document.AddFill(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.None } }, "DefaultFill");
            document.AddFill(new Fill { PatternFill = new PatternFill { PatternType = PatternValues.Gray125 } }, "Gray125");

            // Create "borders".
            document.AddBorder(
                new Border
                {
                    LeftBorder = new LeftBorder(),
                    RightBorder = new RightBorder(),
                    TopBorder = new TopBorder(),
                    BottomBorder = new BottomBorder(),
                    DiagonalBorder = new DiagonalBorder(),
                }, "Default");

            // Create "cellStyleXfs" node.
            document.AddCellStyleFormat(
                new CellFormat()
                {
                    NumberFormatId = 0,
                    FontId = 0,
                    FillId = 0,
                    BorderId = 0,
                }, "Default");

            // Create "cellXfs" node.

            //https://github.com/closedxml/closedxml/wiki/NumberFormatId-Lookup-Table

            // A default style that works for everything but DateTime
            document.AddCellFormat(
                new CellFormat
                {
                    BorderId = 0,
                    FillId = 0,
                    FontId = 0,
                    NumberFormatId = 0,
                    FormatId = 0,
                    ApplyNumberFormat = true,
                }, "General");

            document.AddCellFormat(
                new CellFormat
                {
                    BorderId = 0,
                    FillId = 0,
                    FontId = 0,
                    NumberFormatId = 22,
                    FormatId = 0,
                    ApplyNumberFormat = true,
                }, "DateTime");

            // A style that works for DateTime (just the date)
            document.AddCellFormat(
                new CellFormat
                {
                    BorderId = 0,
                    FillId = 0,
                    FontId = 0,
                    NumberFormatId = 14, // or 22 to include the time
                    FormatId = 0,
                    ApplyNumberFormat = true,
                }, "Date");

            // A style that works for Time
            document.AddCellFormat(
                new CellFormat
                {
                    BorderId = 0,
                    FillId = 0,
                    FontId = 0,
                    NumberFormatId = 21, // H:mm:ss
                    FormatId = 0,
                    ApplyNumberFormat = true,
                }, "Time");

            // Create "cellStyles".
            document.AddCellStyle(
                new CellStyle
                {
                    Name = "Normal",
                    FormatId = 0,
                    BuiltinId = 0,
                }, "Default");
        }
    }
}

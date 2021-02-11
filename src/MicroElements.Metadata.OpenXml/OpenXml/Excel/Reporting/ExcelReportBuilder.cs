// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata.OpenXml.Excel.Parsing;
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

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Excel report builder.
    /// </summary>
    public class ExcelReportBuilder
    {
        private readonly IReportBuilderSettings _settings;
        private readonly ExcelDocumentMetadata _documentMetadata;
        private readonly DocumentContext _documentContext;

        private ExcelSheetMetadata _defaultSheetMetadata = new ExcelSheetMetadata();
        private ExcelColumnMetadata _defaultColumnMetadata = new ExcelColumnMetadata();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelReportBuilder"/> class.
        /// </summary>
        /// <param name="document">Excel document.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <param name="settings">Optional report builder settings.</param>
        public ExcelReportBuilder(
            SpreadsheetDocument document,
            ExcelDocumentMetadata? documentMetadata,
            IReportBuilderSettings? settings)
        {
            _documentMetadata = documentMetadata ?? new ExcelDocumentMetadata();
            _documentContext = InitDocument(document.AssertArgumentNotNull(nameof(document)));
            _settings = settings ?? new ReportBuilderSettings();
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="outFilePath">Output file name.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <param name="settings">Optional report builder settings.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(
            string outFilePath,
            ExcelDocumentMetadata? documentMetadata = null,
            IReportBuilderSettings? settings = null)
        {
            outFilePath.AssertArgumentNotNull(nameof(outFilePath));

            SpreadsheetDocument document = SpreadsheetDocument.Create(outFilePath, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document, documentMetadata, settings);
            return builder;
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="targetStream">Output stream.</param>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <param name="settings">Optional report builder settings.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(
            Stream targetStream,
            ExcelDocumentMetadata? documentMetadata = null,
            IReportBuilderSettings? settings = null)
        {
            targetStream.AssertArgumentNotNull(nameof(targetStream));

            SpreadsheetDocument document = SpreadsheetDocument.Create(targetStream, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document, documentMetadata, settings);
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
                WorkbookView workbookView = new WorkbookView()
                {
                    XWindow = -120,
                    YWindow = -120,
                    WindowWidth = (UInt32Value)19440U,
                    WindowHeight = (UInt32Value)15000U,
                    ActiveTab = 0,
                };
                bookViews.Append(workbookView);

                Sheets sheets = new Sheets();

                workbook.AppendChild(bookViews);
                workbook.AppendChild(sheets);
            }

            var documentContext = new DocumentContext(document, _documentMetadata);

            // Init Stylesheet.
            InitStylesheet(documentContext);

            // Add empty string.
            documentContext.GetOrAddSharedString(string.Empty);

            // External customization
            var customizeFunc = _documentMetadata?.GetValue(ExcelDocumentMetadata.ConfigureDocument);
            customizeFunc?.Invoke(documentContext);

            return documentContext;
        }

        /// <summary>
        /// Adds new excel sheet.
        /// </summary>
        /// <param name="reportRenderer">Report renderer.</param>
        /// <param name="reportRows">Report rows.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder AddReportSheet(IReportRenderer reportRenderer, IEnumerable<IPropertyContainer> reportRows)
        {
            reportRenderer.AssertArgumentNotNull(nameof(reportRenderer));
            reportRows.AssertArgumentNotNull(nameof(reportRows));

            var sheetMetadata = reportRenderer.GetMetadata<ExcelSheetMetadata>() ?? _defaultSheetMetadata;

            // Add a WorksheetPart to the WorkbookPart.
            WorkbookPart workbookPart = _documentContext.Document.WorkbookPart;
            uint sheetCount = workbookPart.GetSheetCount();
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>($"sheet{sheetCount+1}");

            var sheetContext = new SheetContext(_documentContext, worksheetPart, sheetMetadata, reportRenderer);

            AddSheet(sheetContext);

            AddSheetData(sheetContext, reportRows);

            sheetContext.SheetElement.FillCellReferences(forceFill: true);

            // External customization
            var configureSheet = ExcelSheetMetadata.ConfigureSheet.GetFirstDefinedValue(
                sheetContext.SheetMetadata,
                _documentMetadata);

            configureSheet?.Invoke(sheetContext);

            return this;
        }

        /// <summary>
        /// Adds new excel sheet.
        /// </summary>
        /// <param name="reportProvider">Report provider.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder AddReportSheet(IReportProvider reportProvider)
        {
            reportProvider.AssertArgumentNotNull(nameof(reportProvider));

            var reportRows = reportProvider.GetReportRows();

            return AddReportSheet(reportProvider, reportRows);
        }

        private void AddSheet(SheetContext sheetContext)
        {
            WorkbookPart workbookPart = sheetContext.DocumentContext.WorkbookPart;
            WorksheetPart worksheetPart = sheetContext.WorksheetPart;
            uint sheetCount = workbookPart.GetSheetCount();

            Worksheet worksheet = new Worksheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac xr xr2 xr3" } };
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            worksheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            worksheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            worksheet.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            worksheet.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            worksheet.AddNamespaceDeclaration("xr3", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");

            worksheetPart.Worksheet = worksheet;

            SheetViews sheetViews = worksheet.GetOrCreateSheetViews();
            SheetView sheetView = new SheetView { WorkbookViewId = (UInt32Value)0U };
            if (sheetCount == 0)
            {
                sheetView.TabSelected = true;
            }

            sheetViews.AppendChild(sheetView);

            SheetFormatProperties sheetFormatProperties = new SheetFormatProperties
            {
                DefaultRowHeight = 15D,
                DyDescent = 0.25D,
            };

            ColumnContext CreateColumnContext(IPropertyRenderer renderer)
            {
                // FreezeMetadata for optimization
                renderer.FreezeMetadata();

                return new ColumnContext(
                    sheetContext,
                    renderer.GetMetadata<ExcelColumnMetadata>() ?? _defaultColumnMetadata,
                    renderer);
            }

            sheetContext.Columns = sheetContext
                .ReportRenderer
                .Renderers
                .Select(CreateColumnContext)
                .ToList();

            Columns columns = sheetContext.IsNotTransposed ? CreateColumns(sheetContext.Columns) : CreateColumnsTransposed();

            SheetData sheetData = new SheetData();

            //workSheet.Append(sheetDimension);
            worksheet.Append(sheetViews);
            worksheet.Append(sheetFormatProperties);

            worksheet.Append(columns);
            worksheet.Append(sheetData);
            //workSheet.Append(pageMargins);

            // Append a new worksheet and associate it with the workbook.
            Sheets sheets = workbookPart.Workbook.Sheets;
            Sheet sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetCount + 1,
                Name = sheetContext.ReportRenderer.ReportName,
            };

            sheets.Append(sheet);

            bool freezeTopRow = ExcelMetadata.FreezeTopRow.GetFirstDefinedValue(
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
            IEnumerable<IPropertyContainer> dataRows)
        {
            SheetData sheetData = sheetContext.SheetData;
            var columns = sheetContext.Columns;

            var configureRow = ExcelSheetMetadata.ConfigureRow.GetFirstDefinedValue(
                sheetContext.SheetMetadata,
                sheetContext.DocumentContext.DocumentMetadata);

            if (sheetContext.IsNotTransposed)
            {
                // HEADER ROW
                var headerCells = columns.Select(ConstructHeaderCell);
                sheetData.AppendChild(new Row(headerCells));

                // DATA ROWS
                foreach (var dataRow in dataRows)
                {
                    // Create cells
                    CellContext[] cellContexts = new CellContext[columns.Count];
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var columnContext = columns[i];
                        cellContexts[i] = ConstructCell(columnContext, dataRow, callCustomize: false);
                    }

                    // Create row
                    Row excelRow = new Row(cellContexts.Select(context => context.Cell));

                    // Customize row
                    configureRow?.Invoke(new RowContext(cellContexts, excelRow, dataRow));

                    // Customize cells
                    for (var i = 0; i < columns.Count; i++)
                    {
                        CellContext cellContext = cellContexts[i];
                        var configureCell = ExcelCellMetadata.ConfigureCell.GetFirstDefinedValue(
                            cellContext.CellMetadata,
                            cellContext.ColumnContext.ColumnMetadata,
                            sheetContext.SheetMetadata,
                            sheetContext.DocumentContext.DocumentMetadata);

                        configureCell?.Invoke(cellContext);
                    }

                    sheetData.AppendChild(excelRow);
                }
            }
            else
            {
                // NAME COLUMN
                var headerCells = columns.Select(ConstructHeaderCell);
                Row[] excelRows = headerCells.Select(headerCell => new Row(headerCell)).ToArray();

                // VALUE COLUMN
                dataRows = dataRows.ToArray();
                foreach (var dataRow in dataRows)
                {
                    for (var index = 0; index < columns.Count; index++)
                    {
                        var column = columns[index];
                        Row row = excelRows[index];
                        Cell cell = ConstructCell(column, dataRow).Cell;
                        row.AppendChild(cell);
                    }
                }

                foreach (Row row in excelRows)
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

                int columnWidth = ExcelMetadata.ColumnWidth.GetFirstDefinedValue(
                    columnContext.ColumnMetadata,
                    columnContext.SheetMetadata,
                    columnContext.DocumentMetadata);

                columnContext.Column = new Column
                {
                    Min = colNumber,
                    Max = colNumber,
                    Width = columnWidth,
                    CustomWidth = true,
                };

                // External customization
                var customizeFunc = columnContext.ColumnMetadata?.GetValue(ExcelColumnMetadata.ConfigureColumn);
                customizeFunc?.Invoke(columnContext);

                columnsElement.AppendChild(columnContext.Column);
            }

            return columnsElement;
        }

        private Columns CreateColumnsTransposed()
        {
            Columns columnsElement = new Columns();

            columnsElement.Append(new Column { Min = 1, Max = 1, Width = 16, CustomWidth = true });
            columnsElement.Append(new Column { Min = 2, Max = 10, Width = 30, CustomWidth = true });

            return columnsElement;
        }

        private Cell CreateCell(string? value, CellValues dataType)
        {
            string? cellText = value;

            if (dataType == CellValues.SharedString && string.IsNullOrEmpty(cellText))
                dataType = CellValues.String;

            if (dataType == CellValues.SharedString)
                cellText = _documentContext.GetOrAddSharedString(value);

            Cell cell = _settings.CellFactory.CreateCell(cellText, dataType);

            return cell;
        }

        private Cell ConstructHeaderCell(ColumnContext columnContext)
        {
            Cell headerCell = CreateCell(columnContext.PropertyRenderer.TargetName, CellValues.String);

            var propertyRenderer = columnContext.PropertyRenderer;
            ExcelColumnMetadata? excelColumnMetadata = propertyRenderer.GetMetadata<ExcelColumnMetadata>();

            // External customization
            var customizeFunc = excelColumnMetadata?.GetValue(ExcelColumnMetadata.ConfigureHeaderCell);
            if (customizeFunc != null)
            {
                customizeFunc.Invoke(new CellContext(columnContext, excelColumnMetadata!, headerCell));
            }

            return headerCell;
        }

        private CellContext ConstructCell(ColumnContext columnContext, IPropertyContainer source, bool callCustomize = true)
        {
            var propertyRenderer = columnContext.PropertyRenderer;

            // Render value
            string? textValue = propertyRenderer.Render(source);
            textValue = textValue != null ? _settings.StringProvider.GetString(textValue) : null;

            var cellMetadata = propertyRenderer.GetMetadata<ExcelCellMetadata>();

            CellValues dataType = ExcelMetadata.DataType.GetFirstDefinedValue(
                cellMetadata,
                columnContext.ColumnMetadata,
                columnContext.SheetMetadata,
                columnContext.DocumentMetadata);

            Cell cell = CreateCell(textValue, dataType);

            if (dataType == CellValues.Date && cell.StyleIndex == null)
            {
                cell.StyleIndex = _documentContext.GetCellFormatIndex("Date");

                var isLocalTime = propertyRenderer.PropertyType == typeof(LocalTime) || propertyRenderer.PropertyType == typeof(LocalTime?);
                if (isLocalTime)
                {
                    cell.StyleIndex = _documentContext.GetCellFormatIndex("Time");
                }
            }

            CellContext cellContext = new CellContext(columnContext, cellMetadata, cell);

            // External customization
            if (callCustomize)
            {
                var customizeFunc = cellMetadata?.GetValue(ExcelCellMetadata.ConfigureCell);
                customizeFunc?.Invoke(cellContext);
            }

            return cellContext;
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

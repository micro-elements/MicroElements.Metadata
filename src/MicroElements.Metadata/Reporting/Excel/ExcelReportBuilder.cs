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
using Fonts = DocumentFormat.OpenXml.Spreadsheet.Fonts;
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
        private readonly SpreadsheetDocument _document;
        private readonly DocumentContext _documentContext;
        private uint _lastSheetId = 1;

        private ExcelDocumentMetadata _documentMetadata = new ExcelDocumentMetadata();
        private ExcelSheetMetadata _defaultSheetMetadata = new ExcelSheetMetadata();
        private ExcelColumnMetadata _defaultColumnMetadata = new ExcelColumnMetadata();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelReportBuilder"/> class.
        /// </summary>
        /// <param name="document">Excel document.</param>
        public ExcelReportBuilder(SpreadsheetDocument document)
        {
            _document = document.AssertArgumentNotNull(nameof(document));
            _documentContext = InitDocument(_document);
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="outFilePath">Output file name.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(string outFilePath)
        {
            outFilePath.AssertArgumentNotNull(nameof(outFilePath));

            SpreadsheetDocument document = SpreadsheetDocument.Create(outFilePath, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document);
            return builder;
        }

        /// <summary>
        /// Creates new empty excel document and builder.
        /// </summary>
        /// <param name="targetStream">Output stream.</param>
        /// <returns>Builder instance.</returns>
        public static ExcelReportBuilder Create(Stream targetStream)
        {
            targetStream.AssertArgumentNotNull(nameof(targetStream));

            SpreadsheetDocument document = SpreadsheetDocument.Create(targetStream, SpreadsheetDocumentType.Workbook);
            var builder = new ExcelReportBuilder(document);
            return builder;
        }

        /// <summary>
        /// Sets excel document metadata.
        /// </summary>
        /// <param name="documentMetadata">Default excel document metadata.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder WithDocumentMetadata(ExcelDocumentMetadata documentMetadata)
        {
            _documentMetadata = documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));
            return this;
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
            _document.WorkbookPart.Workbook.Save();
            return this;
        }

        /// <summary>
        /// Saves data to output document and closes document.
        /// </summary>
        public void SaveAndClose()
        {
            _document.WorkbookPart.Workbook.Save();
            _document.Close();
        }

        /// <summary>
        /// Default document initialization.
        /// </summary>
        private DocumentContext InitDocument(SpreadsheetDocument document)
        {
            if (_document.WorkbookPart == null)
            {
                _document.AddWorkbookPart();
                _document.WorkbookPart.Workbook = new Workbook();

                // Add Stylesheet.
                var workbookStylesPart = _document.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                workbookStylesPart.Stylesheet = GetStylesheet();

                // Add Sheets to the Workbook.
                document.WorkbookPart.Workbook.AppendChild(new Sheets());
            }

            var documentContext = new DocumentContext(_document, _documentMetadata);

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
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
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

            SheetData sheetData = new SheetData();

            Worksheet workSheet = new Worksheet(sheetData);
            worksheetPart.Worksheet = workSheet;

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

            if (sheetContext.IsNotTransposed)
            {
                Columns columnsElement = CreateColumns(sheetContext.Columns);
                workSheet.InsertAt(columnsElement, 0);
            }

            bool freezeTopRow = ExcelMetadata.GetFirstDefinedValue(
                ExcelMetadata.FreezeTopRow,
                sheetContext.SheetMetadata,
                sheetContext.DocumentMetadata,
                defaultValue: true);

            if (freezeTopRow)
            {
                workSheet.FreezeTopRow(rowNum: 1);
            }

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = _lastSheetId++,
                Name = sheetContext.ReportProvider.ReportName,
            };

            sheetContext.SheetData = sheetData;
            sheetContext.Sheet = sheet;

            workbookPart.Workbook.Sheets.Append(sheetContext.Sheet);
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
                var headerCells = columns.Select(column => ConstructCell(column.PropertyRenderer.TargetName, CellValues.String));
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
                var headerCells = columns.Select(column => ConstructCell(column.PropertyRenderer.TargetName, CellValues.String));
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
                    columnContext.DocumentMetadata,
                    defaultValue: 14);

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
            return new Cell()
            {
                CellValue = new CellValue(value),
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
                columnContext.DocumentMetadata,
                defaultValue: CellValues.String);

            Cell cell = ConstructCell(textValue, dataType);
            if (dataType == CellValues.Date)
            {
                cell.StyleIndex = 1;

                var isLocalTime = propertyRenderer.PropertyType == typeof(LocalTime) || propertyRenderer.PropertyType == typeof(LocalTime?);
                if (isLocalTime)
                {
                    cell.StyleIndex = 2;
                }
            }

            // External customization
            var customizeFunc = cellMetadata?.GetValue(ExcelCellMetadata.CustomizeCell);
            customizeFunc?.Invoke(cell);

            return cell;
        }

        private static Stylesheet GetStylesheet()
        {
            var StyleSheet = new Stylesheet();

            // Create "fonts" node.
            var Fonts = new Fonts();
            Fonts.Append(new Font()
            {
                FontName = new FontName() { Val = "Calibri" },
                FontSize = new FontSize() { Val = 11 },
                FontFamilyNumbering = new FontFamilyNumbering() { Val = 2 },
            });

            Fonts.Count = (uint)Fonts.ChildElements.Count;

            // Create "fills" node.
            var Fills = new Fills();
            Fills.Append(new Fill()
            {
                PatternFill = new PatternFill() { PatternType = PatternValues.None }
            });
            Fills.Append(new Fill()
            {
                PatternFill = new PatternFill() { PatternType = PatternValues.Gray125 }
            });

            Fills.Count = (uint)Fills.ChildElements.Count;

            // Create "borders" node.
            var Borders = new Borders();
            Borders.Append(new Border()
            {
                LeftBorder = new LeftBorder(),
                RightBorder = new RightBorder(),
                TopBorder = new TopBorder(),
                BottomBorder = new BottomBorder(),
                DiagonalBorder = new DiagonalBorder()
            });

            Borders.Count = (uint)Borders.ChildElements.Count;

            // Create "cellStyleXfs" node.
            var CellStyleFormats = new CellStyleFormats();
            CellStyleFormats.Append(new CellFormat()
            {
                NumberFormatId = 0,
                FontId = 0,
                FillId = 0,
                BorderId = 0
            });

            CellStyleFormats.Count = (uint)CellStyleFormats.ChildElements.Count;

            // Create "cellXfs" node.
            var CellFormats = new CellFormats();

            //https://github.com/closedxml/closedxml/wiki/NumberFormatId-Lookup-Table

            // A default style that works for everything but DateTime
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 0,//General
                FormatId = 0,
                ApplyNumberFormat = true
            });

            // A style that works for DateTime (just the date)
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 14, // or 22 to include the time
                FormatId = 0,
                ApplyNumberFormat = true
            });

            // A style that works for Time
            CellFormats.Append(new CellFormat()
            {
                BorderId = 0,
                FillId = 0,
                FontId = 0,
                NumberFormatId = 21, // H:mm:ss
                FormatId = 0,
                ApplyNumberFormat = true
            });

            CellFormats.Count = (uint)CellFormats.ChildElements.Count;

            // Create "cellStyles" node.
            var CellStyles = new CellStyles();
            CellStyles.Append(new CellStyle()
            {
                Name = "Normal",
                FormatId = 0,
                BuiltinId = 0
            });
            CellStyles.Count = (uint)CellStyles.ChildElements.Count;

            // Append all nodes in order.
            StyleSheet.Append(Fonts);
            StyleSheet.Append(Fills);
            StyleSheet.Append(Borders);
            StyleSheet.Append(CellStyleFormats);
            StyleSheet.Append(CellFormats);
            StyleSheet.Append(CellStyles);

            return StyleSheet;
        }
    }

    public static class ExcelExtensions
    {
        public static SheetViews GetOrCreateSheetViews(this Worksheet workSheet)
        {
            if (workSheet.SheetViews == null)
            {
                workSheet.SheetViews = new SheetViews();
            }

            return workSheet.SheetViews;
        }

        public static Worksheet FreezeTopRow(this Worksheet workSheet, int rowNum = 1)
        {
            SheetViews sheetViews = workSheet.GetOrCreateSheetViews();

            SheetView sheetView = new SheetView { TabSelected = true, WorkbookViewId = (UInt32Value)0U };
            sheetViews.AppendChild(sheetView);

            Selection selection = new Selection { Pane = PaneValues.BottomLeft };

            // the freeze pane
            int rowNumWithData = rowNum + 1;
            Pane pane = new Pane
            {
                VerticalSplit = 1,
                TopLeftCell = $"A{rowNumWithData}",
                ActivePane = PaneValues.BottomLeft,
                State = PaneStateValues.Frozen,
            };

            // Selection selection = new Selection() { Pane = PaneValues.BottomLeft };
            sheetView.Append(pane);
            sheetView.Append(selection);

            return workSheet;
        }

    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
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
        private readonly WorkbookPart _workbookPart;
        private uint _lastSheetId = 1;
        private IExcelMetadata _defaultExcelMeta = new ExcelMetadata { DataType = CellValues.String };

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelReportBuilder"/> class.
        /// </summary>
        /// <param name="document">Excel document.</param>
        public ExcelReportBuilder(SpreadsheetDocument document)
        {
            _document = document.AssertArgumentNotNull(nameof(document));
            _workbookPart = document.AddWorkbookPart();
            InitDocument(_document, _workbookPart);
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
        /// Sets default excel metadata.
        /// </summary>
        /// <param name="defaultExcelMeta">Default excel metadata.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder WithDefaultExcelMetadata(IExcelMetadata defaultExcelMeta)
        {
            _defaultExcelMeta = defaultExcelMeta.AssertArgumentNotNull(nameof(defaultExcelMeta));
            return this;
        }

        /// <summary>
        /// Saves data and flushes. Can be called multiple times.
        /// </summary>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder Save()
        {
            _workbookPart.Workbook.Save();
            return this;
        }

        /// <summary>
        /// Saves data to output document and closes document.
        /// </summary>
        public void SaveAndClose()
        {
            _workbookPart.Workbook.Save();
            _document.Close();
        }

        /// <summary>
        /// Adds new excel sheet.
        /// </summary>
        /// <param name="reportProvider">Report provider.</param>
        /// <param name="reportRows">Report rows.</param>
        /// <returns>Builder instance.</returns>
        public ExcelReportBuilder AddReportSheet(IReportProvider reportProvider, IEnumerable<IPropertyContainer> reportRows)
        {
            AddReportSheet(_workbookPart, reportProvider, reportRows);
            return this;
        }

        /// <summary>
        /// Default document initialization.
        /// </summary>
        private static void InitDocument(SpreadsheetDocument document, WorkbookPart workbookPart)
        {
            workbookPart.Workbook = new Workbook();

            // Add Stylesheet.
            var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            workbookStylesPart.Stylesheet = GetStylesheet();

            // Add Sheets to the Workbook.
            document.WorkbookPart.Workbook.AppendChild(new Sheets());
        }

        private void AddReportSheet(WorkbookPart workbookPart, IReportProvider reportProvider, IEnumerable<IPropertyContainer> items)
        {
            var sheetData = AddSheet(workbookPart, reportProvider.ReportName, _lastSheetId++, reportProvider);

            var headerCells = reportProvider.Renderers.Select(renderer => ConstructCell(renderer.TargetName, CellValues.String));
            sheetData.AppendChild(new Row(headerCells));

            foreach (var item in items.NotNull())
            {
                var valueCells = reportProvider.Renderers.Select(renderer => ConstructCell(renderer, item));
                sheetData.AppendChild(new Row(valueCells));
            }
        }

        private SheetData AddSheet(WorkbookPart workbookPart, string name, uint sheetId, IReportProvider reportProvider)
        {
            bool freezeTopRow = true;
            bool createColumns = true;

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

            SheetData sheetData = new SheetData();

            Worksheet workSheet = new Worksheet(sheetData);
            worksheetPart.Worksheet = workSheet;

            if (createColumns)
            {
                Columns columns = CreateColumns(reportProvider);
                workSheet.InsertAt(columns, 0);
            }

            if (freezeTopRow)
            {
                SheetViews sheetViews = new SheetViews();
                workSheet.SheetViews = sheetViews;

                SheetView sheetView = new SheetView { TabSelected = true, WorkbookViewId = (UInt32Value)0U };
                sheetViews.AppendChild(sheetView);

                Selection selection = new Selection { Pane = PaneValues.BottomLeft };

                // the freeze pane
                Pane pane = new Pane
                {
                    VerticalSplit = 1D,
                    TopLeftCell = "A2",
                    ActivePane = PaneValues.BottomLeft,
                    State = PaneStateValues.Frozen,
                };

                // Selection selection = new Selection() { Pane = PaneValues.BottomLeft };
                sheetView.Append(pane);
                sheetView.Append(selection);
            }

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = name,
            };
            workbookPart.Workbook.Sheets.Append(sheet);
            return sheetData;
        }

        private Columns CreateColumns(IReportProvider reportProvider)
        {
            Columns columns = new Columns();
            for (int index = 0; index < reportProvider.Renderers.Count; index++)
            {
                var propertyRenderer = reportProvider.Renderers[index];
                uint colNumber = (uint)(index + 1);
                int columnWidth = 14;
                var excelMeta = propertyRenderer.GetMetadata<IExcelMetadata>() ?? _defaultExcelMeta;
                CellValues dataType = excelMeta.DataType;
                if (dataType == CellValues.Number)
                {
                    columnWidth = 9;
                }

                Column column = new Column { Min = colNumber, Max = colNumber, Width = columnWidth, CustomWidth = true };
                columns.Append(column);
            }

            return columns;
        }

        private Cell ConstructCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }

        private Cell ConstructCell(IPropertyRenderer propertyRenderer, IPropertyContainer source)
        {
            string textValue = propertyRenderer.Render(source);

            var excelMeta = propertyRenderer.GetMetadata<IExcelMetadata>() ?? _defaultExcelMeta;
            CellValues dataType = excelMeta.DataType;

            Cell cell = new Cell
            {
                CellValue = new CellValue(textValue),
                DataType = new EnumValue<CellValues>(dataType),
            };
            if (dataType == CellValues.Date)
            {
                cell.StyleIndex = 1;

                if (propertyRenderer.PropertyType == typeof(LocalTime))
                {
                    cell.StyleIndex = 2;
                }
            }

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
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;
using NodaTime;
using NodaTime.Text;

namespace MicroElements.Parsing
{
    /// <summary>
    /// Excel extensions.
    /// </summary>
    public static class ExcelExtensions
    {
        /// <summary>
        /// Gets sheet by name.
        /// </summary>
        /// <param name="document">OpenXml document.</param>
        /// <param name="name">Sheet name.</param>
        /// <param name="fillCellReferences">Will fill cell references if references are empty.</param>
        /// <returns>Sheet element.</returns>
        public static ExcelElement<Sheet> GetSheet(this SpreadsheetDocument document, string name, bool fillCellReferences = true)
        {
            var sheets = document.WorkbookPart.Workbook.Sheets.Cast<Sheet>();
            Sheet? sheet = sheets.FirstOrDefault(s => s.Name == name);
            ExcelElement<Sheet> result = new ExcelElement<Sheet>(document, sheet);

            if (fillCellReferences)
                result.FillCellReferences();
            return result;
        }

        /// <summary>
        /// Gets columns from sheet.
        /// </summary>
        /// <param name="sheet">Source sheet.</param>
        /// <returns>Sheet rows.</returns>
        public static IEnumerable<ExcelElement<Column>> GetColumns(this ExcelElement<Sheet> sheet)
        {
            if (sheet.Data != null)
            {
                string sheetId = sheet.Data.Id.Value;
                var worksheetPart = (WorksheetPart)sheet.Doc.WorkbookPart.GetPartById(sheetId);
                var worksheet = worksheetPart.Worksheet;
                var excelColumns = worksheet.GetFirstChild<Columns>();
                var columns = excelColumns
                    .Descendants<Column>()
                    .Select(column => new ExcelElement<Column>(sheet.Doc, column));
                return columns;
            }

            return Array.Empty<ExcelElement<Column>>();
        }

        /// <summary>
        /// Gets rows from sheet.
        /// </summary>
        /// <param name="sheet">Source sheet.</param>
        /// <returns>Sheet rows.</returns>
        public static IEnumerable<ExcelElement<Row>> GetRows(this ExcelElement<Sheet> sheet)
        {
            if (sheet.Data != null)
            {
                string sheetId = sheet.Data.Id.Value;
                var worksheetPart = (WorksheetPart)sheet.Doc.WorkbookPart.GetPartById(sheetId);
                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.GetFirstChild<SheetData>();
                var rows = sheetData
                    .Descendants<Row>()
                    .Select(row => new ExcelElement<Row>(sheet.Doc, row));
                return rows;
            }

            return Array.Empty<ExcelElement<Row>>();
        }

        /// <summary>
        /// Gets cell reference by column row index.
        /// Example: (0,0)->A1.
        /// </summary>
        /// <param name="column">Column index.</param>
        /// <param name="row">Row index.</param>
        /// <param name="zeroBased">Is column and row zero based.</param>
        /// <returns>Cell reference.</returns>
        public static StringValue GetCellReference(int column, int row, bool zeroBased = true)
        {
            int columnIndex = zeroBased ? column : column - 1;
            string columnName = GetColumnName(string.Empty, columnIndex);
            int rowName = zeroBased ? row + 1 : row;
            return new StringValue($"{columnName}{rowName}");

            static string GetColumnName(string prefix, int columnIndex = 0)
            {
                return columnIndex < 26
                    ? $"{prefix}{(char)(65 + columnIndex)}"
                    : GetColumnName(GetColumnName(prefix, ((columnIndex - (columnIndex % 26)) / 26) - 1), columnIndex % 26);
            }
        }

        /// <summary>
        /// Gets column reference from cell reference.
        /// For example: A1->A, CD22->CD.
        /// </summary>
        /// <param name="cellReference">Cell reference.</param>
        /// <returns>Column reference.</returns>
        public static string GetColumnReference(this StringValue cellReference)
        {
            cellReference.AssertArgumentNotNull(nameof(cellReference));

            return cellReference.Value.GetColumnReference();
        }

        /// <summary>
        /// Gets column reference from cell reference.
        /// For example: A1->A, CD22->CD.
        /// </summary>
        /// <param name="cellReference">Cell reference.</param>
        /// <returns>Column reference.</returns>
        public static string GetColumnReference(this string cellReference)
        {
            cellReference.AssertArgumentNotNull(nameof(cellReference));

            if (cellReference.Length == 2)
                return cellReference.Substring(0, 1);

            return new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        }

        public class TableDataRow
        {
            public IReadOnlyDictionary<string, string> Data { get; }

            public ExcelElement<Row> Row { get; }

            public TableDataRow(IReadOnlyDictionary<string, string> data, ExcelElement<Row> row)
            {
                Data = data;
                Row = row;
            }
        }

        public static IEnumerable<TableDataRow> AsParsedRows(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider = null)
        {
            ExcelElement<HeaderCell>[] headers = null;
            foreach (var row in rows)
            {
                if (headers == null)
                {
                    headers = row.GetHeaders();
                    continue;
                }

                var rowValues = row.GetRowValues(headers: headers, parserProvider: parserProvider);

                // skip empty line
                if (rowValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var headerNames = headers.Select(header => header.Data.Name ?? header.Data.ColumnReference).ToArrayDebug();
                var expandoObject = headerNames
                    .Zip(rowValues, (header, value) => (header, value))
                    .ToDictionary(tuple => tuple.header, tuple => tuple.value);

                yield return new TableDataRow(expandoObject, row);
            }
        }

        public static IEnumerable<IReadOnlyDictionary<string, string>> AsDictionaryList(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider)
        {
            ExcelElement<HeaderCell>[] headers = null;
            foreach (var row in rows)
            {
                if (headers == null)
                {
                    headers = row.GetHeaders();
                    continue;
                }

                var rowValues = row.GetRowValues(headers: headers, parserProvider: parserProvider);

                // skip empty line
                if (rowValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var headerNames = headers.Select(header => header.Data.Name ?? header.Data.ColumnReference).ToArrayDebug();
                var expandoObject = headerNames
                    .Zip(rowValues, (header, value) => (header, value))
                    .ToDictionary(tuple => tuple.header, tuple => tuple.value);

                yield return expandoObject;
            }
        }

        public static ExcelElement<Cell>[] GetRowCells(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<Cell>>();

            var rowCells = row.Data
                .GetRowCells()
                .Select(cell => new ExcelElement<Cell>(row.Doc, cell))
                .ToArray();

            return rowCells;
        }

        public static IEnumerable<Cell> GetRowCells(this Row row) => row.Descendants<Cell>();

        public static ExcelElement<HeaderCell>[] GetHeaders(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<HeaderCell>>();

            var cells = row.GetRowCells();
            var headers = cells.Select(cell => new ExcelElement<HeaderCell>(row.Doc, new HeaderCell(cell))).ToArray();
            return headers;
        }

        public static string[] GetRowValues(
            this ExcelElement<Row> row,
            ExcelElement<HeaderCell>[] headers,
            IParserProvider parserProvider,
            string nullValue = null)
        {
            if (row.IsEmpty())
                return Array.Empty<string>();

            var cells = row.GetRowCells();

            string[] rowValues = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                // Find cell for the same column.
                var cell = cells.FirstOrDefault(c => c.Data.CellReference.GetColumnReference() == header.Data.ColumnReference);

                if (cell != null)
                {
                    // Set propertyParser for cell according column name
                    var propertyParser = parserProvider.GetParsers().FirstOrDefault(parser => parser.SourceName == header.Data.Name);
                    if (propertyParser != null)
                    {
                        cell.SetMetadata(propertyParser);
                    }

                    rowValues[i] = cell.GetCellValue(nullValue);
                }
                else
                {
                    rowValues[i] = nullValue;
                }
            }

            return rowValues;
        }

        private static WorkbookPart GetWorkbookPartFromCell(Cell cell)
        {
            Worksheet workSheet = cell.Ancestors<Worksheet>().FirstOrDefault();
            SpreadsheetDocument doc = workSheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
            return doc.WorkbookPart;
        }

        private static CellFormat GetCellFormat(this ExcelElement<Cell> cell)
        {
            WorkbookPart workbookPart = GetWorkbookPartFromCell(cell);
            int styleIndex = (int)cell.Data.StyleIndex.Value;
            CellFormat cellFormat = (CellFormat)workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
            return cellFormat;
        }

        private static string GetFormattedValue(this ExcelElement<Cell> cell)
        {
            string value;
            var cellFormat = cell.GetCellFormat();
            if (cellFormat.NumberFormatId != 0)
            {
                var elements = cell.Doc.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats.Elements<NumberingFormat>().ToList();
                string format = elements.FirstOrDefault(i => i.NumberFormatId.Value == cellFormat.NumberFormatId.Value)?.FormatCode;

                //Note: Look also: https://stackoverflow.com/questions/13176832/reading-a-date-from-xlsx-using-open-xml-sdk
                format ??= "d/m/yyyy";
                double number = double.Parse(cell.Data.InnerText);
                value = number.ToString(format);
            }
            else
            {
                value = cell.Data.InnerText;
            }

            return value;
        }

        /// <summary>
        /// Gets text value.
        /// Uses SharedStringTable if needed.
        /// For DateTime, LocalDate and LocalTime tries to convert double excel value to ISO format.
        /// </summary>
        public static string? GetCellValue(this ExcelElement<Cell> cell, string? nullValue = null)
        {
            Cell cellData = cell.Data;
            string cellValue = cellData.CellValue?.InnerText ?? nullValue;
            string cellTextValue = null;

            if (cellValue != null && cellData.DataType != null && cellData.DataType.Value == CellValues.SharedString)
            {
                cellTextValue = cell.Doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(cellValue)).InnerText;
            }

            if (cellTextValue == null && cellValue != null && cellData.DataType == null)
            {
                var propertyParser = cell.GetMetadata<IPropertyParser>();

                if (propertyParser != null)
                {
                    if (propertyParser.TargetType == typeof(LocalDate) || propertyParser.TargetType == typeof(LocalDate?))
                    {
                        var parseResult = LocalDatePattern.Iso.Parse(cellValue);
                        if (parseResult.Success)
                        {
                            cellTextValue = cellValue;
                        }
                        else
                        {
                            Prelude
                                .ParseDouble(cellValue)
                                .Map(FromExcelSerialDate)
                                .Map(dt => dt.ToString("yyyy-MM-dd"))
                                .Match(s => cellTextValue = s, () => { });
                        }
                    }
                    else if (propertyParser.TargetType == typeof(LocalTime) || propertyParser.TargetType == typeof(LocalTime?))
                    {
                        var parseResult = LocalTimePattern.ExtendedIso.Parse(cellValue);
                        if (parseResult.Success)
                        {
                            cellTextValue = cellValue;
                        }
                        else
                        {
                            Prelude
                                .ParseDouble(cellValue)
                                .Map(FromExcelSerialDate)
                                .Map(dt => dt.ToString("HH:mm:ss"))
                                .Match(s => cellTextValue = s, () => { });
                        }
                    }
                    else if (propertyParser.TargetType == typeof(DateTime) || propertyParser.TargetType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(cellValue, out _))
                        {
                            cellTextValue = cellValue;
                        }
                        else
                        {
                            Prelude
                                .ParseDouble(cellValue)
                                .Map(FromExcelSerialDate)
                                .Map(dt => dt.ToString("yyyy-MM-ddTHH:mm:ss"))
                                .Match(s => cellTextValue = s, () => { });
                        }
                    }
                }
            }

            return cellTextValue ?? cellValue;
        }

        /// <summary>
        /// Source: https://stackoverflow.com/questions/727466/how-do-i-convert-an-excel-serial-date-number-to-a-net-datetime
        /// </summary>
        public static DateTime FromExcelSerialDate(double serialDate)
        {
            // NOTE: DateTime.FromOADate parses 1 as 1899-12-31. Correct value is 1900-01-01
            // return DateTime.FromOADate(serialDate);
            if (serialDate > 59)
                serialDate -= 1; //Excel/Lotus 2/29/1900 bug
            return new DateTime(1899, 12, 31).AddDays(serialDate);
        }

        /// <summary>
        /// Maps rows to entities with specified <paramref name="parserProvider"/>.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="sheet">Sheet.</param>
        /// <param name="parserProvider"><see cref="IParserProvider"/>.</param>
        /// <param name="factory">Factory.</param>
        /// <returns>Enumeration of <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> GetRowsAs<T>(
            this ExcelElement<Sheet> sheet,
            IParserProvider parserProvider,
            Func<IReadOnlyList<IPropertyValue>, T> factory = null)
        {
            if (factory == null)
                factory = list => (T)Activator.CreateInstance(typeof(T), list);

            if (sheet.IsEmpty())
                return Array.Empty<T>();

            return sheet
                .GetRows()
                .MapRows(parserProvider, factory);
        }

        public static IEnumerable<T> MapRows<T>(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider,
            Func<IReadOnlyList<IPropertyValue>, T>? factory = null)
        {
            rows.AssertArgumentNotNull(nameof(rows));
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            if (factory == null)
                factory = list => (T)Activator.CreateInstance(typeof(T), list);

            return rows
                .AsDictionaryList(parserProvider)
                .Select(parserProvider.ParseProperties)
                .Select(factory);
        }

        public static bool NeedFillCellReferences(this ExcelElement<Sheet> sheet)
        {
            if (sheet.IsEmpty())
                return false;

            bool needFill = sheet.GetRows().FirstOrDefault()?.GetRowCells().FirstOrDefault()?.Data?.CellReference == null;
            return needFill;
        }

        public static ExcelElement<Sheet> FillCellReferences(this ExcelElement<Sheet> sheet, bool forceFill = false, bool zeroBased = false)
        {
            if (forceFill || NeedFillCellReferences(sheet))
            {
                sheet
                    .GetRows()
                    .FillCellReferences(zeroBased: zeroBased)
                    .Iterate();
            }

            return sheet;
        }

        public static IEnumerable<ExcelElement<Row>> FillCellReferences(this IEnumerable<ExcelElement<Row>> rows, bool zeroBased = false)
        {
            int rowIndex = zeroBased ? 0 : 1;
            foreach (ExcelElement<Row> row in rows)
            {
                FillCellReferences(row, rowIndex, zeroBased: false);

                rowIndex++;
                yield return row;
            }
        }

        private static void FillCellReferences(Row row, int rowIndex, bool zeroBased = false)
        {
            // Set row index
            row.RowIndex = (uint)rowIndex;

            var rowCells = row.GetRowCells().ToArray();
            for (int colIndexZeroBased = 0; colIndexZeroBased < rowCells.Length; colIndexZeroBased++)
            {
                // Set cell reference
                var colIndex = zeroBased ? colIndexZeroBased : colIndexZeroBased + 1;
                rowCells[colIndexZeroBased].CellReference = GetCellReference(colIndex, rowIndex, zeroBased: zeroBased);
            }
        }
    }
}

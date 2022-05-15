// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.CodeContracts;
using MicroElements.Collections.Extensions.Iterate;
using MicroElements.Metadata.Parsing;
using MicroElements.Validation;
using NodaTime;
using NodaTime.Text;
using Message = MicroElements.Functional.Message;

namespace MicroElements.Metadata.OpenXml.Excel.Parsing
{
    /// <summary>
    /// Excel extensions.
    /// </summary>
    public static partial class ExcelParsingExtensions
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
                    .GetChildren<Column>()
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
            return GetOpenXmlRows(sheet)
                .Zip(Enumerable.Repeat(sheet, int.MaxValue), (row, sh) => new ExcelElement<Row>(sh.Doc, row));
        }

        /// <summary>
        /// Gets rows from sheet.
        /// </summary>
        /// <param name="sheet">Source sheet.</param>
        /// <returns>Sheet rows.</returns>
        public static IEnumerable<Row> GetOpenXmlRows(this ExcelElement<Sheet> sheet)
        {
            if (sheet.Data != null)
            {
                string sheetId = sheet.Data.Id.Value;
                var worksheetPart = (WorksheetPart)sheet.Doc.WorkbookPart.GetPartById(sheetId);
                var worksheet = worksheetPart.Worksheet;
                var sheetData = worksheet.GetFirstChild<SheetData>();
                var rows = sheetData.GetChildren<Row>();
                return rows;
            }

            return Array.Empty<Row>();
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
            string columnName = GetColumnName(columnIndex);
            int rowName = zeroBased ? row + 1 : row;
            return new StringValue(string.Concat(columnName, rowName.ToString()));
        }

        private static readonly ConcurrentDictionary<int, string> _columnIndexes = new ConcurrentDictionary<int, string>();

        /// <summary>
        /// Gets column index (cached).
        /// </summary>
        public static string GetColumnName(int columnIndex = 0)
        {
            return _columnIndexes.GetOrAdd(columnIndex, i => GetColumnName(string.Empty, i));
        }

        private static string GetColumnName(string prefix, int columnIndex = 0)
        {
            return columnIndex < 26
                ? $"{prefix}{(char)(65 + columnIndex)}"
                : GetColumnName(GetColumnName(prefix, ((columnIndex - (columnIndex % 26)) / 26) - 1), columnIndex % 26);
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

        /// <summary>
        /// Gets row cells.
        /// </summary>
        /// <param name="row">Source row.</param>
        /// <returns>Cells.</returns>
        public static ExcelElement<Cell>[] GetRowCells(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<Cell>>();

            var rowCells = row.Data!
                .GetRowCells()
                .Select(cell => new ExcelElement<Cell>(row.Doc, cell))
                .ToArray();

            return rowCells;
        }

        /// <summary>
        /// Gets OpenXmlElement children.
        /// </summary>
        /// <param name="container">Source element.</param>
        /// <returns>Children enumeration.</returns>
        [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "GC hint.")]
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "GC hint.")]
        public static IEnumerable<T> GetChildren<T>(this OpenXmlElement container)
            where T : OpenXmlElement
        {
            if (container.HasChildren && container.FirstChild != null)
            {
                OpenXmlElement? element;
                for (element = container.FirstChild; element != null; element = element.NextSibling())
                {
                    if (element is T child)
                        yield return child;
                }

                // GC hint.
                element = null;
            }
        }

        /// <summary>
        /// Gets row cells.
        /// </summary>
        /// <param name="row">Source row.</param>
        /// <returns>Cells.</returns>
        public static IEnumerable<Cell> GetRowCells(this Row row)
        {
            return row.GetChildren<Cell>();
        }

        /// <summary>
        /// Gets row cells as <see cref="HeaderCell"/>.
        /// </summary>
        /// <param name="row">Source row.</param>
        /// <returns>HeaderCells.</returns>
        public static ExcelElement<HeaderCell>[] GetHeaders(this ExcelElement<Row> row)
        {
            if (row.IsEmpty())
                return Array.Empty<ExcelElement<HeaderCell>>();

            var cells = row.GetRowCells();
            var headers = cells.Select(cell => new ExcelElement<HeaderCell>(row.Doc, new HeaderCell(cell))).ToArray();
            return headers;
        }

        /// <summary>
        /// Gets value for each header.
        /// <see cref="IPropertyParser"/> will be attached to cells where cell header name same as <see cref="IPropertyParser.SourceName"/>.
        /// </summary>
        /// <param name="row">Source excel row.</param>
        /// <param name="headers">Headers to use.</param>
        /// <param name="nullValue">Null value for absent cells.</param>
        /// <returns>Cell values for provided headers.</returns>
        public static string?[] GetRowValues(
            this ExcelElement<Row> row,
            ExcelElement<HeaderCell>[] headers,
            string? nullValue = null)
        {
            if (row.IsEmpty())
                return Array.Empty<string>();

            var cells = row.GetRowCells();

            string?[] rowValues = new string?[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];

                // Find cell for the same column.
                var cell = cells.FirstOrDefault(c => c.Data.CellReference.GetColumnReference() == header.Data.ColumnReference);

                if (cell != null)
                {
                    IPropertyParser? propertyParser = header.GetMetadata<IPropertyParser>();
                    rowValues[i] = cell.GetCellValue(nullValue, propertyParser);

                    //StringValue cellReference = cell.Data.CellReference;
                }
                else
                {
                    // TODO: NULL string has no knowledge about value: absent or null as value
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
                var elements = cell.Doc.WorkbookPart.WorkbookStylesPart.Stylesheet.NumberingFormats.Elements<NumberingFormat>();
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
        public static string? GetCellValue(this ExcelElement<Cell> cell, string? nullValue = null, IPropertyParser? propertyParser = null)
        {
            Cell? cellData = cell.Data;
            string? cellValue = cellData?.CellValue?.InnerText ?? nullValue;
            string? cellTextValue = null;

            if (cellData == null)
                return cellValue;

            CellValues? dataTypeValue = cellData.DataType?.Value;

            if (cellValue != null && dataTypeValue == CellValues.SharedString)
            {
                cellTextValue = cell.Doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(cellValue)).InnerText;
            }

            if (cellTextValue == null && cellValue != null)
            {
                propertyParser ??= cell.GetMetadata<IPropertyParser>();

                if (propertyParser != null)
                    cellTextValue = TryParseAsDateType(cellValue, propertyParser.TargetType);
            }

            return cellTextValue ?? cellValue;
        }

        private static string? TryParseAsDateType(string cellValue, Type targetType)
        {
            string? cellTextValue = null;

            if (targetType == typeof(LocalDate) || targetType == typeof(LocalDate?))
            {
                var parseResult = LocalDatePattern.Iso.Parse(cellValue);
                if (parseResult.Success)
                {
                    cellTextValue = cellValue;
                }
                else
                {
                    cellTextValue = Parser
                        .ParseDouble(cellValue)
                        .Map(d => d.FromExcelSerialDate())
                        .Map(dt => dt.ToString("yyyy-MM-dd"))
                        .Value;
                }
            }
            else if (targetType == typeof(LocalTime) || targetType == typeof(LocalTime?))
            {
                var parseResult = LocalTimePattern.ExtendedIso.Parse(cellValue);
                if (parseResult.Success)
                {
                    cellTextValue = cellValue;
                }
                else
                {
                    cellTextValue = Parser
                        .ParseDouble(cellValue)
                        .Map(d => d.FromExcelSerialDate())
                        .Map(dt => dt.ToString("HH:mm:ss"))
                        .Value;
                }
            }
            else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (DateTime.TryParse(cellValue, out _))
                {
                    cellTextValue = cellValue;
                }
                else
                {
                    cellTextValue = Parser
                        .ParseDouble(cellValue)
                        .Map(d => d.FromExcelSerialDate())
                        .Map(dt => dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF"))
                        .Value;
                }
            }

            return cellTextValue;
        }

        /// <summary>
        /// Gets rows as enumeration of dictionaries.
        /// </summary>
        /// <param name="rows">Source rows.</param>
        /// <param name="parserProvider">Parser provider.</param>
        /// <returns>List of dictionaries.</returns>
        public static IEnumerable<IReadOnlyDictionary<string, string?>> AsDictionaryList(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider)
        {
            return ExtractRowValues(rows, parserProvider);
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
            Func<IReadOnlyList<IPropertyValue>, T>? factory = null)
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

        public static IEnumerable<ValidationResult<T>> MapAndValidateRows<T>(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider,
            Func<ParsedData, T>? factory = null)
        {
            rows.AssertArgumentNotNull(nameof(rows));
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            var dictionaryList = rows.AsDictionaryList(parserProvider);

            return dictionaryList.MapAndValidateRows(parserProvider, factory);
        }

        public static IEnumerable<ValidationResult<T>> MapAndValidateRows<T>(
            this IEnumerable<IReadOnlyDictionary<string, string?>> dictionaryList,
            IParserProvider parserProvider,
            Func<ParsedData, T>? factory = null)
        {
            dictionaryList.AssertArgumentNotNull(nameof(dictionaryList));
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            if (factory == null)
                factory = (context) => (T)Activator.CreateInstance(typeof(T), context.Values);

            IEnumerable<IReadOnlyList<ParseResult<IPropertyValue>>> parseResults =
                ParseRowValues(dictionaryList, parserProvider);

            foreach (IReadOnlyList<ParseResult<IPropertyValue>> parseResult in parseResults)
            {
                IPropertyValue[] resultArray = new IPropertyValue[parseResult.Count];
                Message[]? errorArray = null;
                int iResult = 0;
                int iError = 0;

                for (int i = 0; i < parseResult.Count; i++)
                {
                    ParseResult<IPropertyValue> result = parseResult[i];
                    if (result.IsSuccess)
                    {
                        resultArray[iResult++] = result.Value!;
                    }
                    else
                    {
                        if (errorArray == null)
                            errorArray = new Message[parseResult.Count];
                        errorArray[iError++] = result.Error!;
                    }
                }

                var results = resultArray[0..iResult];
                var errors = errorArray?[0..iError];
                var mapContext = new ParsedData(results, errors);

                T data = factory(mapContext);
                yield return new ValidationResult<T>(data, errors);
            }
        }

        public static bool NeedFillCellReferences(this ExcelElement<Sheet> sheet)
        {
            if (sheet.IsEmpty())
                return false;

            bool needFill = sheet.GetOpenXmlRows().FirstOrDefault()?.GetRowCells().FirstOrDefault()?.CellReference == null;
            return needFill;
        }

        public static ExcelElement<Sheet> FillCellReferences(this ExcelElement<Sheet> sheet, bool forceFill = false, bool zeroBased = false)
        {
            if (forceFill || NeedFillCellReferences(sheet))
            {
                sheet
                    .GetOpenXmlRows()
                    .FillCellReferences(zeroBased: zeroBased)
                    .Iterate();
            }

            return sheet;
        }

        public static IEnumerable<Row> FillCellReferences(this IEnumerable<Row> rows, bool zeroBased = false)
        {
            int rowIndex = zeroBased ? 0 : 1;
            foreach (Row row in rows)
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

            int colIndexZeroBased = 0;
            foreach (Cell rowCell in row.GetRowCells())
            {
                // Set cell reference
                var colIndex = zeroBased ? colIndexZeroBased : colIndexZeroBased + 1;
                rowCell.CellReference = GetCellReference(colIndex, rowIndex, zeroBased: zeroBased);
                colIndexZeroBased++;
            }
        }
    }

    public static partial class ExcelParsingExtensions
    {
        /// <summary>
        /// Extracts rows as enumeration of dictionaries.
        /// </summary>
        /// <param name="rows">Source rows.</param>
        /// <param name="parserProvider">Parser provider.</param>
        /// <returns>List of dictionaries.</returns>
        public static IEnumerable<IReadOnlyDictionary<string, string?>> ExtractRowValues(
            this IEnumerable<ExcelElement<Row>> rows,
            IParserProvider parserProvider)
        {
            rows.AssertArgumentNotNull(nameof(rows));
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            ExcelElement<HeaderCell>[]? headers = null;
            string[]? headerNames = null;

            foreach (var row in rows)
            {
                if (headers == null)
                {
                    // Use first row as headers
                    headers = row.GetHeaders();

                    // Associate parser for each header
                    foreach (var header in headers)
                    {
                        // TODO: RIGID SEARCH. Use predicate?
                        var propertyParser = parserProvider.GetParsers().FirstOrDefault(parser => parser.SourceName == header.Data.Name);
                        if (propertyParser != null)
                        {
                            header.SetMetadata(propertyParser);
                        }
                    }

                    headerNames = headers.Select(header => header.Data.Name ?? header.Data.ColumnReference).ToArray();

                    continue;
                }

                // TODO: CellReference can be extracted here!!!
                var rowValues = row.GetRowValues(headers);

                // skip empty line
                if (rowValues.All(string.IsNullOrWhiteSpace))
                    continue;

                var expandoObject = headerNames
                    .Zip(rowValues, (header, value) => (header, value))
                    .ToDictionary(tuple => tuple.header, tuple => tuple.value);

                //expandoObject.AsMetadataProvider().SetMetadata("RowReference", row.Data.RowIndex);

                yield return expandoObject;
            }
        }

        public static IEnumerable<IReadOnlyList<ParseResult<IPropertyValue>>> ParseRowValues(
            this IEnumerable<IReadOnlyDictionary<string, string?>> dictionaryList,
            IParserProvider parserProvider)
        {
            dictionaryList.AssertArgumentNotNull(nameof(dictionaryList));
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));

            IEnumerable<IReadOnlyList<ParseResult<IPropertyValue>>> parseResults = dictionaryList
                .Select(parserProvider.ParsePropertiesAsParseResults);

            return parseResults;
        }

        public readonly struct ParsedData
        {
            public readonly IReadOnlyList<IPropertyValue> Values;
            public readonly IReadOnlyList<Message>? Errors;

            public ParsedData(IReadOnlyList<IPropertyValue> values, IReadOnlyList<Message>? errors)
            {
                Values = values;
                Errors = errors;
            }
        }

        public static ParsedData Merge(this IReadOnlyList<ParseResult<IPropertyValue>> parseResultList)
        {
            IPropertyValue[] resultArray = new IPropertyValue[parseResultList.Count];
            Message[]? errorArray = null;
            int iResult = 0;
            int iError = 0;

            for (int i = 0; i < parseResultList.Count; i++)
            {
                ParseResult<IPropertyValue> parseResult = parseResultList[i];
                if (parseResult.IsSuccess)
                {
                    resultArray[iResult++] = parseResult.Value!;
                }
                else
                {
                    if (errorArray == null)
                        errorArray = new Message[parseResultList.Count];
                    errorArray[iError++] = parseResult.Error!;
                }
            }

            var results = resultArray[0..iResult];
            var errors = errorArray?[0..iError];
            var mapContext = new ParsedData(results, errors);

            return mapContext;
        }

        public static IEnumerable<ParsedData> Merge(this IEnumerable<IReadOnlyList<ParseResult<IPropertyValue>>> parseResultLists)
        {
            parseResultLists.AssertArgumentNotNull(nameof(parseResultLists));

            return parseResultLists.Select(list => list.Merge());
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Parsing
{
    public class ExcelElement<TOpenXmlElement> : IMetadataProvider
    {
        /// <summary>
        /// Документ в котором содержится элемент.
        /// </summary>
        public SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Данные Excel.
        /// </summary>
        public TOpenXmlElement Data { get; }

        public ExcelElement(SpreadsheetDocument doc, TOpenXmlElement data)
        {
            Doc = doc.AssertArgumentNotNull(nameof(doc));
            Data = data;
        }

        public static implicit operator TOpenXmlElement(ExcelElement<TOpenXmlElement> dataSource) => dataSource.Data;

        public T Match<T>(Func<ExcelElement<TOpenXmlElement>, T> process, Func<T> error) => Data != null ? process(this) : error();

        public bool IsEmpty() => Data == null;

        /// <inheritdoc />
        public override string ToString() => $"{Data}";
    }

    public class Column
    {
        public ExcelElement<Cell> Cell { get; }

        public string CellReference => Cell.Data.CellReference;

        public string ColumnReference => Cell.Data.CellReference.GetColumnReference();

        public string Name { get; }

        public Column(ExcelElement<Cell> cell)
        {
            Cell = cell;
            Name = cell.GetCellValue();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(ColumnReference)}: {ColumnReference}, {nameof(Name)}: {Name}";
        }
    }
}

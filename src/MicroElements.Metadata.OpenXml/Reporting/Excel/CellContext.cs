// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Parsing;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Context for cell rendering.
    /// </summary>
    public class CellContext
    {
        /// <summary>
        /// Gets ColumnContext for this cell.
        /// </summary>
        public ColumnContext ColumnContext { get; }

        /// <summary>
        /// Gets cell metadata.
        /// </summary>
        public IExcelMetadata? CellMetadata { get; }

        /// <summary>
        /// Gets OpenXml cell.
        /// </summary>
        public Cell Cell { get; }

        /// <summary>
        /// Gets <see cref="IPropertyRenderer"/> for this cell.
        /// </summary>
        public IPropertyRenderer PropertyRenderer => ColumnContext.PropertyRenderer;

        /// <summary>
        /// Gets DocumentContext for this cell.
        /// </summary>
        public DocumentContext DocumentContext => ColumnContext.SheetContext.DocumentContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CellContext"/> class.
        /// </summary>
        /// <param name="columnContext">ColumnContext for this cell.</param>
        /// <param name="cellMetadata">Cell metadata.</param>
        /// <param name="cell">OpenXml cell.</param>
        public CellContext(ColumnContext columnContext, IExcelMetadata? cellMetadata, Cell cell)
        {
            ColumnContext = columnContext.AssertArgumentNotNull(nameof(columnContext));
            CellMetadata = cellMetadata;
            Cell = cell;
        }

        /// <summary>
        /// Gets cell text value.
        /// </summary>
        /// <returns>Text value.</returns>
        public string? GetCellValue()
        {
            ExcelElement<Cell> excelCell = new ExcelElement<Cell>(ColumnContext.SheetContext.DocumentContext.Document, Cell);
            string? cellValue = excelCell.GetCellValue();
            return cellValue;
        }
    }
}

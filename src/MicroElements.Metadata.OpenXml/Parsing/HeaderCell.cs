// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Parsing
{
    /// <summary>
    /// Represents header cell.
    /// </summary>
    public class HeaderCell
    {
        /// <summary>
        /// Gets header cell.
        /// </summary>
        public ExcelElement<Cell> Cell { get; }

        /// <summary>
        /// Gets column reference.
        /// </summary>
        public string ColumnReference => Cell.Data.CellReference.GetColumnReference();

        /// <summary>
        /// Gets header name.
        /// </summary>
        public string Name { get; }

        public HeaderCell(ExcelElement<Cell> cell)
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

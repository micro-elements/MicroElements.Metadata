// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using DocumentFormat.OpenXml;
using MicroElements.CodeContracts;
using MicroElements.Collections.Cache;

namespace MicroElements.Metadata.OpenXml.Excel.Parsing
{
    public static partial class ExcelParsingExtensions
    {
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

        /// <summary>
        /// Gets column index (cached).
        /// </summary>
        public static string GetColumnName(int columnIndex = 0)
        {
            return Cache
                .Instance<int, string>("ColumnName")
                .GetOrAdd(columnIndex, i => GetColumnName(string.Empty, i));
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
    }
}

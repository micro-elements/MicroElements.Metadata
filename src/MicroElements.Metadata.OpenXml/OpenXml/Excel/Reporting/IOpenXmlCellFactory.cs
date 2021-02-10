// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    public interface IOpenXmlCellFactory
    {
        Cell CreateCell(string? cellText, CellValues dataType);
    }

    public class DefaultOpenXmlCellFactory : IOpenXmlCellFactory
    {
        /// <inheritdoc />
        public Cell CreateCell(string? cellText, CellValues dataType)
        {
            Cell cell = new Cell
            {
                CellValue = new CellValue(cellText),
                DataType = new EnumValue<CellValues>(dataType),
            };

            return cell;
        }
    }

    public class CachedOpenXmlCellFactory : IOpenXmlCellFactory
    {
        private class Cache
        {
            internal readonly ConcurrentDictionary<(string, CellValues), CellValue> CellValues = new ConcurrentDictionary<(string, CellValues), CellValue>();
            internal readonly ConcurrentDictionary<CellValues, EnumValue<CellValues>> EnumValues = new ConcurrentDictionary<CellValues, EnumValue<CellValues>>();
        }

        private readonly Cache _cache = new Cache();

        /// <inheritdoc />
        public Cell CreateCell(string? cellText, CellValues dataType)
        {
            CellValue? cellValue = null;
            if (cellText != null)
            {
                cellValue = _cache.CellValues.GetOrAdd((cellText, dataType), tuple => new CellValue(tuple.Item1));
                if (cellValue.Parent != null)
                {
                    // HACK: Parent has internal set
                    typeof(CellValue).GetProperty("Parent").SetValue(cellValue, null);
                }
            }

            Cell cell = new Cell
            {
                CellValue = cellValue,
                DataType = _cache.EnumValues.GetOrAdd(dataType, values => new EnumValue<CellValues>(values)),
            };

            return cell;
        }
    }
}

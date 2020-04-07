// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Excel metadata to customize excel generation.
    /// </summary>
    public interface IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues DataType { get; }
    }

    public interface IExcelDocumentMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues DataType { get; }
    }

    public interface IExcelSheetMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues? DataType { get; }
    }

    public interface IExcelColumnMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues? DataType { get; }
    }

    public interface IExcelCellMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues? DataType { get; }
    }

    public static class aaa
    {
        public static IExcelCellMetadata Merge(IExcelColumnMetadata columnMetadata, IExcelCellMetadata cellMetadata)
        {
            return cellMetadata;
        }
    }

    /// <summary>
    /// Excel metadata to customize excel generation.
    /// </summary>
    public class ExcelMetadata : IExcelMetadata
    {
        /// <inheritdoc />
        public CellValues DataType { get; set; }
    }
}

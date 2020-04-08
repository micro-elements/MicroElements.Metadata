// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Marker interface for excel metadata.
    /// </summary>
    public interface IExcelMetadata { }

    /// <summary>
    /// Excel Document customizations.
    /// </summary>
    public interface IExcelDocumentMetadata : IHasDataType, IHasFreezeTopRow, IHasColumnWidth
    {
    }

    /// <summary>
    /// Excel Sheet customizations.
    /// </summary>
    public interface IExcelSheetMetadata : IHasDataType, IHasFreezeTopRow, IHasColumnWidth
    {
    }

    /// <summary>
    /// Excel Column customizations.
    /// </summary>
    public interface IExcelColumnMetadata : IHasDataType, IHasColumnWidth
    {
    }

    /// <summary>
    /// Excel Cell customizations.
    /// </summary>
    public interface IExcelCellMetadata : IHasDataType
    {
    }

    /// <summary>
    /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell. 
    /// </summary>
    public interface IHasDataType : IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        CellValues? DataType { get; }
    }

    public interface IHasFreezeTopRow : IExcelMetadata
    {
        /// <summary>
        /// Freeze top row.
        /// </summary>
        bool? FreezeTopRow { get; }
    }

    public interface IHasColumnWidth : IExcelMetadata
    {
        /// <summary>
        /// Gets column width.
        /// </summary>
        int? ColumnWidth { get; }
    }

    public static class ExcelMetadata
    {
        public static class Default
        {
            public static IExcelDocumentMetadata ExcelDocumentMetadata = new ExcelDocumentMetadata
            {
                DataType = CellValues.String,
                FreezeTopRow = true,
                ColumnWidth = 14,
            };
            public static IExcelSheetMetadata ExcelSheetMetadata = new ExcelSheetMetadata { };
            public static IExcelColumnMetadata ExcelColumnMetadata = new ExcelColumnMetadata { };
            public static IExcelCellMetadata ExcelCellMetadata = new ExcelCellMetadata { };
        }

        public static T GetDefinedValue<T>(
            T? data1,
            T? data2,
            T? data3 = null,
            T? data4 = null,
            T defaultValue = default)
            where T : struct
        {
            if (data1 != null)
                return data1.Value;

            if (data2 != null)
                return data2.Value;

            if (data3 != null)
                return data3.Value;

            if (data4 != null)
                return data4.Value;

            return defaultValue;
        }
    }

    public class ExcelDocumentMetadata : IExcelDocumentMetadata
    {
        /// <inheritdoc />
        public CellValues? DataType { get; set; }

        /// <inheritdoc />
        public bool? FreezeTopRow { get; set; }

        /// <inheritdoc />
        public int? ColumnWidth { get; set; }
    }

    public class ExcelSheetMetadata : IExcelSheetMetadata
    {
        /// <inheritdoc />
        public CellValues? DataType { get; set; }

        /// <inheritdoc />
        public bool? FreezeTopRow { get; set; }

        /// <inheritdoc />
        public int? ColumnWidth { get; set; }
    }

    public class ExcelColumnMetadata : IExcelColumnMetadata
    {
        /// <inheritdoc />
        public CellValues? DataType { get; set; }

        /// <inheritdoc />
        public int? ColumnWidth { get; set; }
    }

    public class ExcelCellMetadata : IExcelCellMetadata
    {
        /// <inheritdoc />
        public CellValues? DataType { get; set; }
    }

    public class DocumentContext
    {
        public IExcelDocumentMetadata DocumentMetadata { get; }

        public DocumentContext(IExcelDocumentMetadata documentMetadata)
        {
            DocumentMetadata = documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));
        }
    }

    public class SheetContext
    {
        public DocumentContext DocumentContext { get; }

        public IExcelDocumentMetadata DocumentMetadata => DocumentContext.DocumentMetadata;

        public IExcelSheetMetadata SheetMetadata { get; }

        public SheetContext(DocumentContext documentContext, IExcelSheetMetadata sheetMetadata)
        {
            DocumentContext = documentContext.AssertArgumentNotNull(nameof(documentContext));
            SheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
        }
    }

    public class ColumnContext
    {
        public SheetContext SheetContext { get; }

        public IExcelDocumentMetadata DocumentMetadata => SheetContext.DocumentMetadata;

        public IExcelSheetMetadata SheetMetadata => SheetContext.SheetMetadata;

        public IExcelColumnMetadata ColumnMetadata { get; }

        public IPropertyRenderer PropertyRenderer { get; }

        public ColumnContext(SheetContext sheetContext, IExcelColumnMetadata columnMetadata, IPropertyRenderer propertyRenderer)
        {
            SheetContext = sheetContext.AssertArgumentNotNull(nameof(sheetContext));
            ColumnMetadata = columnMetadata.AssertArgumentNotNull(nameof(columnMetadata));
            PropertyRenderer = propertyRenderer.AssertArgumentNotNull(nameof(propertyRenderer));
        }
    }

    public class CellContext
    {
        public ColumnContext ColumnContext { get; }

        public IExcelDocumentMetadata DocumentMetadata => ColumnContext.DocumentMetadata;

        public IExcelSheetMetadata SheetMetadata => ColumnContext.SheetMetadata;

        public IExcelColumnMetadata ColumnMetadata => ColumnContext.ColumnMetadata;

        public IExcelCellMetadata CellMetadata { get; }

        public CellContext(ColumnContext columnContext, IExcelCellMetadata cellMetadata)
        {
            ColumnContext = columnContext.AssertArgumentNotNull(nameof(columnContext));
            CellMetadata = cellMetadata.AssertArgumentNotNull(nameof(cellMetadata));
        }
    }
}

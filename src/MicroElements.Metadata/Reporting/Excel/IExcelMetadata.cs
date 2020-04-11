// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Marker interface for excel metadata.
    /// </summary>
    public interface IExcelMetadata : IPropertyContainer
    {
    }

    public static class ExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell.
        /// </summary>
        public static readonly IProperty<CellValues> DataType = new Property<CellValues>("DataType").SetDefaultValue(CellValues.String);

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = new Property<int>("ColumnWidth").SetDefaultValue(14);

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static readonly IProperty<bool> FreezeTopRow = new Property<bool>("FreezeTopRow").SetDefaultValue(true);

        /// <summary>
        /// Transpose sheet.
        /// </summary>
        public static readonly IProperty<bool> Transpose = new Property<bool>("Transpose");

        public static T GetDefinedValue<T>(
            IProperty<T> property,
            IPropertyContainer data1,
            IPropertyContainer data2 = null,
            IPropertyContainer data3 = null,
            IPropertyContainer data4 = null,
            T defaultValue = default)
        {
            IPropertyValue<T> propertyValue = null;

            if (data1 != null)
            {
                propertyValue = data1.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (data2 != null)
            {
                propertyValue = data2.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (data3 != null)
            {
                propertyValue = data3.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (data4 != null)
            {
                propertyValue = data4.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Excel Document customizations.
    /// </summary>
    public class ExcelDocumentMetadata : MutablePropertyContainer, IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell.
        /// </summary>
        public static IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static IProperty<bool> FreezeTopRow = ExcelMetadata.FreezeTopRow;
    }

    /// <summary>
    /// Excel Sheet customizations.
    /// </summary>
    public class ExcelSheetMetadata : MutablePropertyContainer, IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell.
        /// </summary>
        public static IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static IProperty<bool> FreezeTopRow = ExcelMetadata.FreezeTopRow;
    }

    /// <summary>
    /// Excel Column customizations.
    /// </summary>
    public class ExcelColumnMetadata : MutablePropertyContainer, IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell.
        /// </summary>
        public static IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;
    }

    /// <summary>
    /// Excel Cell customizations.
    /// </summary>
    public class ExcelCellMetadata : MutablePropertyContainer, IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        public static IProperty<CellValues> DataType = ExcelMetadata.DataType;
    }

    public class DocumentContext
    {
        public IExcelMetadata DocumentMetadata { get; }

        public DocumentContext(IExcelMetadata documentMetadata)
        {
            DocumentMetadata = documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));
        }
    }

    public class SheetContext
    {
        public DocumentContext DocumentContext { get; }

        public IExcelMetadata DocumentMetadata => DocumentContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata { get; }

        public SheetContext(DocumentContext documentContext, IExcelMetadata sheetMetadata)
        {
            DocumentContext = documentContext.AssertArgumentNotNull(nameof(documentContext));
            SheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
        }
    }

    public class ColumnContext
    {
        public SheetContext SheetContext { get; }

        public IExcelMetadata DocumentMetadata => SheetContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata => SheetContext.SheetMetadata;

        public IExcelMetadata ColumnMetadata { get; }

        public IPropertyRenderer PropertyRenderer { get; }

        public ColumnContext(SheetContext sheetContext, IExcelMetadata columnMetadata, IPropertyRenderer propertyRenderer)
        {
            SheetContext = sheetContext.AssertArgumentNotNull(nameof(sheetContext));
            ColumnMetadata = columnMetadata.AssertArgumentNotNull(nameof(columnMetadata));
            PropertyRenderer = propertyRenderer.AssertArgumentNotNull(nameof(propertyRenderer));
        }
    }

    public class CellContext
    {
        public ColumnContext ColumnContext { get; }

        public IExcelMetadata DocumentMetadata => ColumnContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata => ColumnContext.SheetMetadata;

        public IExcelMetadata ColumnMetadata => ColumnContext.ColumnMetadata;

        public IExcelMetadata CellMetadata { get; }

        public CellContext(ColumnContext columnContext, IExcelMetadata cellMetadata)
        {
            ColumnContext = columnContext.AssertArgumentNotNull(nameof(columnContext));
            CellMetadata = cellMetadata.AssertArgumentNotNull(nameof(cellMetadata));
        }
    }
}

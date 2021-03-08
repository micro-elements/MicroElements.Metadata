// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Marker interface for excel metadata.
    /// </summary>
    public interface IExcelMetadata : IPropertyContainer
    {
    }

    /// <summary>
    /// Contains excel customization properties and helper methods.
    /// </summary>
    public static class ExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// Base excel meta. Every cell has type but metadata can be provided hierarchically: document->sheet->column->cell.
        /// </summary>
        public static readonly IProperty<CellValues> DataType = new Property<CellValues>("DataType").WithDefaultValue(CellValues.SharedString);

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = new Property<int>("ColumnWidth").WithDefaultValue(14);

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static readonly IProperty<bool> FreezeTopRow = new Property<bool>("FreezeTopRow").WithDefaultValue(false);

        /// <summary>
        /// Transpose sheet.
        /// </summary>
        public static readonly IProperty<bool> Transpose = new Property<bool>("Transpose").WithDefaultValue(false);

        /// <summary>
        /// Gets property value from the first source where value is defined.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to search.</param>
        /// <param name="source1">Source 1.</param>
        /// <param name="source2">Source 2.</param>
        /// <param name="source3">Source 3.</param>
        /// <param name="source4">Source 4.</param>
        /// <returns>Property value or default value.</returns>
        [return: MaybeNull]
        public static T GetFirstDefinedValue<T>(
            this IProperty<T> property,
            IPropertyContainer? source1 = null,
            IPropertyContainer? source2 = null,
            IPropertyContainer? source3 = null,
            IPropertyContainer? source4 = null)
        {
            IPropertyValue<T>? propertyValue;

            if (source1 != null)
            {
                propertyValue = source1.GetPropertyValue(property, SearchOptions.ExistingOnly);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source2 != null)
            {
                propertyValue = source2.GetPropertyValue(property, SearchOptions.ExistingOnly);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source3 != null)
            {
                propertyValue = source3.GetPropertyValue(property, SearchOptions.ExistingOnly);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source4 != null)
            {
                propertyValue = source4.GetPropertyValue(property, SearchOptions.ExistingOnly);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            return property.DefaultValue.Value;
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
        public static readonly IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static readonly IProperty<bool> FreezeTopRow = ExcelMetadata.FreezeTopRow;

        /// <summary>
        /// Document customization function.
        /// </summary>
        public static readonly IProperty<Action<DocumentContext>> ConfigureDocument = new Property<Action<DocumentContext>>("ConfigureDocument");
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
        public static readonly IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static readonly IProperty<bool> FreezeTopRow = ExcelMetadata.FreezeTopRow;

        /// <summary>
        /// Sheet customization function.
        /// </summary>
        public static readonly IProperty<Action<SheetContext>> ConfigureSheet = new Property<Action<SheetContext>>("ConfigureSheet");

        /// <summary>
        /// Row customization function.
        /// </summary>
        public static readonly IProperty<Action<RowContext>> ConfigureRow = new Property<Action<RowContext>>("ConfigureRow");
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
        public static readonly IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = ExcelMetadata.ColumnWidth;

        /// <summary>
        /// Sheet customization function.
        /// </summary>
        public static readonly IProperty<Action<ColumnContext>> ConfigureColumn = new Property<Action<ColumnContext>>("ConfigureColumn");

        /// <summary>
        /// HeaderCell customization function.
        /// </summary>
        public static readonly IProperty<Action<CellContext>> ConfigureHeaderCell = new Property<Action<CellContext>>("ConfigureHeaderCell");
    }

    /// <summary>
    /// Excel Cell customizations.
    /// </summary>
    public class ExcelCellMetadata : MutablePropertyContainer, IExcelMetadata
    {
        /// <summary>
        /// Excel data type.
        /// </summary>
        public static readonly IProperty<CellValues> DataType = ExcelMetadata.DataType;

        /// <summary>
        /// Cell customization function.
        /// </summary>
        public static readonly IProperty<Action<CellContext>> ConfigureCell = new Property<Action<CellContext>>("ConfigureCell");
    }
}

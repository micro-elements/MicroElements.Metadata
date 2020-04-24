// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
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

    /// <summary>
    /// Contains excel customization properties and helper methods.
    /// </summary>
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

        /// <summary>
        /// Gets property value from the first source where value is defined.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to search.</param>
        /// <param name="source1">Source 1.</param>
        /// <param name="source2">Source 2.</param>
        /// <param name="source3">Source 3.</param>
        /// <param name="source4">Source 4.</param>
        /// <param name="defaultValue">Default value if all sources does not contain property value.</param>
        /// <returns>Property value or default value.</returns>
        public static T GetFirstDefinedValue<T>(
            IProperty<T> property,
            IPropertyContainer source1 = null,
            IPropertyContainer source2 = null,
            IPropertyContainer source3 = null,
            IPropertyContainer source4 = null,
            T defaultValue = default)
        {
            IPropertyValue<T> propertyValue;

            if (source1 != null)
            {
                propertyValue = source1.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source2 != null)
            {
                propertyValue = source2.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source3 != null)
            {
                propertyValue = source3.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
                if (propertyValue.HasValue())
                    return propertyValue.Value;
            }

            if (source4 != null)
            {
                propertyValue = source4.GetPropertyValue(property, searchInParent: false, calculateValue: true, useDefaultValue: false);
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
        public static readonly IProperty<Action<DocumentContext>> CustomizeDocument = new Property<Action<DocumentContext>>("CustomizeDocument");
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
        public static readonly IProperty<Action<SheetContext>> CustomizeSheet = new Property<Action<SheetContext>>("CustomizeSheet");
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
        public static readonly IProperty<Action<ColumnContext>> CustomizeColumn = new Property<Action<ColumnContext>>("CustomizeColumn");
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
        public static readonly IProperty<Action<Cell>> CustomizeCell = new Property<Action<Cell>>("CustomizeCell");
    }

    public class DocumentContext
    {
        public SpreadsheetDocument Document { get; }

        public IExcelMetadata DocumentMetadata { get; }

        public DocumentContext(SpreadsheetDocument document, IExcelMetadata documentMetadata)
        {
            Document = document;
            DocumentMetadata = documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));
        }
    }

    public class SheetContext
    {
        public DocumentContext DocumentContext { get; }

        public IExcelMetadata DocumentMetadata => DocumentContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata { get; }

        public IReportProvider ReportProvider { get; }

        public bool IsTransposed => ExcelMetadata.GetFirstDefinedValue(ExcelMetadata.Transpose, SheetMetadata, defaultValue: false);

        public bool IsNotTransposed => !IsTransposed;

        public IReadOnlyList<ColumnContext> Columns { get; internal set; }

        public SheetData SheetData { get; set; }

        public Sheet Sheet { get; set; }

        public SheetContext(
            DocumentContext documentContext,
            IExcelMetadata sheetMetadata,
            IReportProvider reportProvider)
        {
            DocumentContext = documentContext.AssertArgumentNotNull(nameof(documentContext));
            SheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
            ReportProvider = reportProvider.AssertArgumentNotNull(nameof(reportProvider));
        }
    }

    public class ColumnContext
    {
        public SheetContext SheetContext { get; }

        public IExcelMetadata DocumentMetadata => SheetContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata => SheetContext.SheetMetadata;

        public IExcelMetadata ColumnMetadata { get; }

        public IPropertyRenderer PropertyRenderer { get; }

        public Column Column { get; set; }

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

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
        public static readonly IProperty<CellValues> DataType = new Property<CellValues>("DataType").SetDefaultValue(CellValues.SharedString);

        /// <summary>
        /// Column width.
        /// </summary>
        public static readonly IProperty<int> ColumnWidth = new Property<int>("ColumnWidth").SetDefaultValue(14);

        /// <summary>
        /// Freeze top row.
        /// </summary>
        public static readonly IProperty<bool> FreezeTopRow = new Property<bool>("FreezeTopRow").SetDefaultValue(false);

        /// <summary>
        /// Transpose sheet.
        /// </summary>
        public static readonly IProperty<bool> Transpose = new Property<bool>("Transpose").SetDefaultValue(false);

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
        public static T GetFirstDefinedValue<T>(
            IProperty<T> property,
            IPropertyContainer source1 = null,
            IPropertyContainer source2 = null,
            IPropertyContainer source3 = null,
            IPropertyContainer source4 = null)
        {
            IPropertyValue<T> propertyValue;

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

            return (source1 ?? source2 ?? source3 ?? source4).GetPropertyValue(property, SearchOptions.Default).Value;
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

    public class DocumentContext
    {
        public SpreadsheetDocument Document { get; }

        public WorkbookPart WorkbookPart => Document.WorkbookPart;

        public IExcelMetadata DocumentMetadata { get; }

        public IDictionary<string, string> SharedStringTable { get; } = new Dictionary<string, string>();

        public DocumentContext(SpreadsheetDocument document, IExcelMetadata documentMetadata)
        {
            Document = document;
            DocumentMetadata = documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));
        }
    }

    public class SheetContext
    {
        public DocumentContext DocumentContext { get; }

        public WorksheetPart WorksheetPart { get; }

        public IExcelMetadata DocumentMetadata => DocumentContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata { get; }

        public IReportProvider ReportProvider { get; }

        public bool IsTransposed => ExcelMetadata.GetFirstDefinedValue(ExcelMetadata.Transpose, SheetMetadata);

        public bool IsNotTransposed => !IsTransposed;

        public IReadOnlyList<ColumnContext> Columns { get; internal set; }

        public SheetData SheetData { get; set; }

        public Sheet Sheet { get; set; }

        public SheetContext(
            DocumentContext documentContext,
            WorksheetPart worksheetPart,
            IExcelMetadata sheetMetadata,
            IReportProvider reportProvider)
        {
            DocumentContext = documentContext.AssertArgumentNotNull(nameof(documentContext));
            SheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
            ReportProvider = reportProvider.AssertArgumentNotNull(nameof(reportProvider));
            WorksheetPart = worksheetPart.AssertArgumentNotNull(nameof(worksheetPart));
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

        public IPropertyRenderer PropertyRenderer => ColumnContext.PropertyRenderer;

        public Cell Cell { get; }

        public CellContext(ColumnContext columnContext, IExcelMetadata cellMetadata, Cell cell)
        {
            ColumnContext = columnContext.AssertArgumentNotNull(nameof(columnContext));
            CellMetadata = cellMetadata.AssertArgumentNotNull(nameof(cellMetadata));
            Cell = cell;
        }
    }

    public static class ExcelMetadataExtensions
    {
        /// <summary>
        /// Takes configure action and combines action with new action.
        /// </summary>
        public static TContainer WithCombinedConfigure<TContainer, TContext>(this TContainer value, IProperty<Action<TContext>> property, Action<TContext> action)
            where TContainer : IMutablePropertyContainer
        {
            Action<TContext> existedAction = value.GetPropertyValue(property)?.Value;

            return value.WithValue(property, context => Combine(context, existedAction, action));

            static void Combine(TContext context, Action<TContext> action1, Action<TContext> action2)
            {
                action1?.Invoke(context);
                action2(context);
            }
        }
    }
}

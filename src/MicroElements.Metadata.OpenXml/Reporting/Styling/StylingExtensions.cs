// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;
using MicroElements.Reporting.Excel;

namespace MicroElements.Reporting.Styling
{
    /// <summary>
    /// Extensions for excel styling.
    /// </summary>
    public static class StylingExtensions
    {
        /// <summary>
        /// Configures cell.
        /// </summary>
        /// <typeparam name="TMetadataProvider">MetadataProvider.</typeparam>
        /// <param name="metadataProvider">Source metadata.</param>
        /// <param name="configureCell">Cell customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same metadata instance.</returns>
        public static TMetadataProvider ConfigureCell<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Action<CellContext> configureCell,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.ConfigureMetadata<TMetadataProvider, ExcelCellMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelCellMetadata.ConfigureCell, configureCell, combineMode));
        }

        /// <summary>
        /// Configures column.
        /// </summary>
        /// <typeparam name="TMetadataProvider">MetadataProvider.</typeparam>
        /// <param name="metadataProvider">Source metadata.</param>
        /// <param name="configureColumn">Column customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same metadata instance.</returns>
        public static TMetadataProvider ConfigureColumn<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Action<ColumnContext> configureColumn,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.ConfigureMetadata<TMetadataProvider, ExcelColumnMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelColumnMetadata.ConfigureColumn, configureColumn, combineMode));
        }

        /// <summary>
        /// Configures column HeaderCell.
        /// </summary>
        /// <typeparam name="TMetadataProvider">MetadataProvider.</typeparam>
        /// <param name="metadataProvider">Source metadata.</param>
        /// <param name="configureHeaderCell">HeaderCell customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same metadata instance.</returns>
        public static TMetadataProvider ConfigureHeaderCell<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Action<CellContext> configureHeaderCell,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.ConfigureMetadata<TMetadataProvider, ExcelColumnMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelColumnMetadata.ConfigureHeaderCell, configureHeaderCell, combineMode));
        }

        /// <summary>
        /// Configures Row.
        /// </summary>
        /// <typeparam name="TMetadataProvider">MetadataProvider.</typeparam>
        /// <param name="metadataProvider">Source metadata.</param>
        /// <param name="configureRow">Row customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same metadata instance.</returns>
        public static TMetadataProvider ConfigureRow<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Action<RowContext> configureRow,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TMetadataProvider : IMetadataProvider
        {
            return metadataProvider.ConfigureMetadata<TMetadataProvider, ExcelSheetMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelSheetMetadata.ConfigureRow, configureRow, combineMode));
        }

        /// <summary>
        /// Gets format name for <see cref="ICellFormatProvider"/>.
        /// This name used for registering format in stylesheet.
        /// </summary>
        /// <param name="formatProvider">Source format provider.</param>
        /// <returns>Format name.</returns>
        public static string GetFormatName(this ICellFormatProvider formatProvider)
        {
            return formatProvider.GetType().ToString();
        }

        /// <summary>
        /// Applies style to cell.
        /// Register style with <see cref="ExcelExtensions.AddCellFormat"/> and other Add methods..
        /// </summary>
        /// <param name="context">Cell context.</param>
        /// <param name="applyStyleName">Style name to apply.</param>
        /// <param name="mergeMode">Apply style.</param>
        public static void ApplyStyleToCell(this CellContext context, string applyStyleName, MergeMode mergeMode = MergeMode.Merge)
        {
            var documentContext = context.ColumnContext.SheetContext.DocumentContext;

            context.Cell.StyleIndex = GetMergedStyleIndex(documentContext, context.Cell.StyleIndex, applyStyleName, mergeMode);
        }

        /// <summary>
        /// Applies style to column.
        /// Register style with <see cref="ExcelExtensions.AddCellFormat"/> and other Add methods..
        /// </summary>
        /// <param name="context">Cell context.</param>
        /// <param name="applyStyleName">Style name to apply.</param>
        /// <param name="mergeMode">Apply style.</param>
        public static void ApplyStyleToColumn(this ColumnContext context, string applyStyleName, MergeMode mergeMode = MergeMode.Merge)
        {
            var documentContext = context.SheetContext.DocumentContext;

            context.Column.Style = GetMergedStyleIndex(documentContext, context.Column.Style, applyStyleName, mergeMode);
        }

        /// <summary>
        /// Gets merged style index.
        /// </summary>
        /// <param name="documentContext">Document context.</param>
        /// <param name="currentStyleIndex">Current style index.</param>
        /// <param name="applyStyleName">New style name.</param>
        /// <param name="mergeMode">Apply style.</param>
        /// <returns>Index of new style.</returns>
        public static uint GetMergedStyleIndex(
            this DocumentContext documentContext,
            UInt32Value? currentStyleIndex,
            string applyStyleName,
            MergeMode mergeMode = MergeMode.Merge)
        {
            int styleIndex = (int)(currentStyleIndex?.Value ?? 0);

            uint mergedStyleIndex;
            if (styleIndex != 0)
            {
                CellFormat currentStyle = documentContext.GetCellFormat(styleIndex);
                string currentStyleName = currentStyle.GetStyleSheetName();
                string combinedStyleName = mergeMode == MergeMode.Merge
                    ? (currentStyleName, applyStyleName).ToString()
                    : mergeMode == MergeMode.ReverseMerge
                        ? (applyStyleName, currentStyleName).ToString()
                        : applyStyleName;

                uint combinedStyleIndex = documentContext.GetCellFormatIndex(combinedStyleName);
                if (combinedStyleIndex > 0)
                {
                    // style already created
                    mergedStyleIndex = combinedStyleIndex;
                }
                else
                {
                    CellFormat applyStyle = documentContext.GetCellFormat(applyStyleName);
                    CellFormat combineStyle = mergeMode == MergeMode.Merge
                        ? CombineStyles(currentStyle, applyStyle)
                        : mergeMode == MergeMode.ReverseMerge
                            ? CombineStyles(applyStyle, currentStyle)
                            : applyStyle;

                    // register new style
                    documentContext.AddCellFormat(combineStyle, combinedStyleName);

                    mergedStyleIndex = documentContext.GetCellFormatIndex(combinedStyleName);
                }
            }
            else
            {
                mergedStyleIndex = documentContext.GetCellFormatIndex(applyStyleName);
            }

            return mergedStyleIndex;
        }

        /// <summary>
        /// Merges two styles.
        /// </summary>
        /// <param name="currentStyle">Current style.</param>
        /// <param name="applyStyle">Style to apply.</param>
        /// <returns>New style instance.</returns>
        public static CellFormat CombineStyles(CellFormat currentStyle, CellFormat applyStyle)
        {
            CellFormat newStyle = new CellFormat
            {
                NumberFormatId = currentStyle.NumberFormatId,
                FontId = currentStyle.FontId,
                FillId = currentStyle.FillId,
                BorderId = currentStyle.BorderId,

                ApplyNumberFormat = currentStyle.ApplyNumberFormat,
                ApplyFont = currentStyle.ApplyFont,
                ApplyFill = currentStyle.ApplyFill,
                ApplyBorder = currentStyle.ApplyBorder,
            };

            if (applyStyle.ApplyNumberFormat != null && applyStyle.ApplyNumberFormat)
            {
                newStyle.NumberFormatId = applyStyle.NumberFormatId;
                newStyle.ApplyNumberFormat = applyStyle.ApplyNumberFormat;
            }

            if (applyStyle.ApplyFont != null && applyStyle.ApplyFont)
            {
                newStyle.FontId = applyStyle.FontId;
                newStyle.ApplyFont = applyStyle.ApplyFont;
            }

            if (applyStyle.ApplyFill != null && applyStyle.ApplyFill)
            {
                newStyle.FillId = applyStyle.FillId;
                newStyle.ApplyFill = applyStyle.ApplyFill;
            }

            if (applyStyle.ApplyBorder != null && applyStyle.ApplyBorder)
            {
                newStyle.BorderId = applyStyle.BorderId;
                newStyle.ApplyBorder = applyStyle.ApplyBorder;
            }

            return newStyle;
        }

        /// <summary>
        /// Configures cell style.
        /// </summary>
        /// <typeparam name="TMetadataProvider">MetadataProvider.</typeparam>
        /// <param name="metadataProvider">Source metadata.</param>
        /// <param name="getCellStyle">Function. Input: CellValue, Result: StyleName.</param>
        /// <param name="mergeMode">StyleApply mode. Default: Merge.</param>
        /// <returns>The same metadata instance.</returns>
        public static TMetadataProvider ConfigureCellStyle<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Func<string?, string?> getCellStyle,
            MergeMode mergeMode = MergeMode.Merge)
            where TMetadataProvider : IMetadataProvider
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));
            getCellStyle.AssertArgumentNotNull(nameof(getCellStyle));

            if (mergeMode == MergeMode.Set)
            {
                metadataProvider.ConfigureMetadata<ExcelCellMetadata>(
                    metadata => metadata.SetValue(ExcelCellMetadata.ConfigureCell, context => ConfigureCell(context, getCellStyle, mergeMode)));
            }
            else
            {
                metadataProvider.ConfigureMetadata<ExcelCellMetadata>(metadata =>
                    metadata.WithCombinedConfigure(ExcelCellMetadata.ConfigureCell, context => ConfigureCell(context, getCellStyle, mergeMode)));
            }

            return metadataProvider;

            static void ConfigureCell(CellContext context, Func<string?, string?> getCellStyle, MergeMode styleApply)
            {
                string? cellValue = context.GetCellValue();
                string? cellStyle = getCellStyle(cellValue);

                if (cellStyle != null)
                    context.ApplyStyleToCell(cellStyle, styleApply);
            }
        }

        /// <summary>
        /// Configures (registers) document styles from list of <see cref="ICellFormatProvider"/>.
        /// </summary>
        /// <typeparam name="TExcelMetadata">Metadata container.</typeparam>
        /// <param name="excelMetadata">Excel metadata.</param>
        /// <param name="formatProviders"><see cref="ICellFormatProvider"/> list.</param>
        /// <returns>The same metadata.</returns>
        public static TExcelMetadata WithExcelDocumentStyles<TExcelMetadata>(this TExcelMetadata excelMetadata, params ICellFormatProvider[] formatProviders)
            where TExcelMetadata : IMutablePropertyContainer, IExcelMetadata
        {
            return excelMetadata.WithCombinedConfigure
            (
                ExcelDocumentMetadata.ConfigureDocument,
                context =>
                {
                    foreach (var formatProvider in formatProviders)
                    {
                        context.AddCellFormat(formatProvider.CreateFormat(context), formatProvider.GetFormatName());
                    }
                }
            );
        }

        /// <summary>
        /// Configures (registers) document styles from list of <see cref="ICellFormatProvider"/>.
        /// </summary>
        /// <typeparam name="TExcelMetadata">Metadata container.</typeparam>
        /// <param name="excelMetadata">Excel metadata.</param>
        /// <param name="modelType">Model type with properties. Properties can be marked with <see cref="FormatAttribute"/>.</param>
        /// <returns>The same metadata.</returns>
        public static TExcelMetadata WithExcelDocumentStyles<TExcelMetadata>(this TExcelMetadata excelMetadata, Type modelType)
            where TExcelMetadata : IMutablePropertyContainer, IExcelMetadata
        {
            Type[] cellFormatProviderTypes = modelType
                .GetProperties()
                .Select(item => item.GetCustomAttribute<FormatAttribute>()?.CellFormatProviderType)
                .Where(item => item != default)
                .Distinct()
                .ToArray()!;
            return cellFormatProviderTypes.Length > 0 ? excelMetadata.WithCombinedConfigure(
                    ExcelDocumentMetadata.ConfigureDocument,
                    context =>
                    {
                        foreach (Type cellFormatProviderType in cellFormatProviderTypes)
                        {
                            ICellFormatProvider instance = (ICellFormatProvider)Activator.CreateInstance(cellFormatProviderType);
                            context.AddCellFormat(instance.CreateFormat(context), instance.GetFormatName());
                        }
                    })
                : excelMetadata;
        }
    }
}

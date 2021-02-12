// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata.OpenXml.Excel.Styling;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// PropertyRenderer customizations.
    /// </summary>
    public static class StylingExtensions
    {
        /// <summary>
        /// Sets <see cref="ExcelMetadata.DataType"/> for column (<see cref="ExcelColumnMetadata"/>).
        /// If <paramref name="cellValues"/> is null then cell style is cleared to default.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="cellValues">Cell type.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetExcelType<TPropertyRenderer>(this TPropertyRenderer propertyRenderer, CellValues? cellValues)
            where TPropertyRenderer : IPropertyRenderer
        {
            if (cellValues.HasValue)
            {
                return propertyRenderer.ConfigureMetadata<TPropertyRenderer, ExcelColumnMetadata>(
                    metadata => metadata.SetValue(ExcelMetadata.DataType, cellValues.Value));
            }

            return propertyRenderer.ConfigureMetadata<TPropertyRenderer, ExcelColumnMetadata>(
                metadata => metadata.RemoveValue(ExcelMetadata.DataType));
        }

        /// <summary>
        /// Configures cell.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="configureCell">Cell customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ConfigureCell<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            Action<CellContext> configureCell,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer.ConfigureMetadata<TPropertyRenderer, ExcelCellMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelCellMetadata.ConfigureCell, configureCell, combineMode));
        }

        /// <summary>
        /// Configures column.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="configureColumn">Column customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ConfigureColumn<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            Action<ColumnContext> configureColumn,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer.ConfigureMetadata<TPropertyRenderer, ExcelColumnMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelColumnMetadata.ConfigureColumn, configureColumn, combineMode));
        }

        /// <summary>
        /// Configures column HeaderCell.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="configureHeaderCell">HeaderCell customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ConfigureHeaderCell<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            Action<CellContext> configureHeaderCell,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer.ConfigureMetadata<TPropertyRenderer, ExcelColumnMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelColumnMetadata.ConfigureHeaderCell, configureHeaderCell, combineMode));
        }

        /// <summary>
        /// Configures Row.
        /// </summary>
        /// <typeparam name="TMetadataProvider">Metadata provider type.</typeparam>
        /// <param name="metadataProvider">Metadata provider.</param>
        /// <param name="configureRow">Row customization action.</param>
        /// <param name="combineMode">Combine mode. Default: AppendToEnd.</param>
        /// <returns>The same renderer instance.</returns>
        public static TMetadataProvider ConfigureRow<TMetadataProvider>(
            this TMetadataProvider metadataProvider,
            Action<RowContext> configureRow,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TMetadataProvider : IReportRenderer
        {
            return metadataProvider.ConfigureMetadata<TMetadataProvider, ExcelSheetMetadata>(
                metadata => metadata.WithCombinedConfigure(ExcelSheetMetadata.ConfigureRow, configureRow, combineMode));
        }

        /// <summary>
        /// Configures cell style.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="getCellStyle">Function. Input: CellValue, Result: StyleName.</param>
        /// <param name="mergeMode">StyleApply mode. Default: Merge.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ConfigureCellStyle<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            Func<string?, string?> getCellStyle,
            MergeMode mergeMode = MergeMode.Merge)
            where TPropertyRenderer : IPropertyRenderer
        {
            propertyRenderer.AssertArgumentNotNull(nameof(propertyRenderer));
            getCellStyle.AssertArgumentNotNull(nameof(getCellStyle));

            if (mergeMode == MergeMode.Set)
            {
                propertyRenderer.ConfigureMetadata<ExcelCellMetadata>(
                    metadata => metadata.SetValue(ExcelCellMetadata.ConfigureCell, context => ConfigureCellStyleInternal(context, getCellStyle, mergeMode)));
            }
            else
            {
                propertyRenderer.ConfigureMetadata<ExcelCellMetadata>(metadata =>
                    metadata.WithCombinedConfigure(ExcelCellMetadata.ConfigureCell, context => ConfigureCellStyleInternal(context, getCellStyle, mergeMode)));
            }

            return propertyRenderer;

            static void ConfigureCellStyleInternal(CellContext context, Func<string?, string?> getCellStyle, MergeMode styleApply)
            {
                string? cellValue = context.GetCellValue();
                string? cellStyle = getCellStyle(cellValue);

                if (cellStyle != null)
                    context.ApplyStyleToCell(cellStyle, styleApply);
            }
        }

        /// <summary>
        /// Applies styling from <see cref="ICellFormatProvider"/>.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <param name="autoRegister">Use format registration.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ApplyStyle<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            ICellFormatProvider formatProvider,
            bool autoRegister = true)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer.ConfigureCell(context =>
            {
                var formatName = formatProvider.Name;

                if (autoRegister)
                {
                    CellFormat? cellFormat = context.DocumentContext.TryGetCellFormat(formatName);
                    if (cellFormat == null)
                    {
                        cellFormat = formatProvider.CreateFormat(context.DocumentContext);
                        context.DocumentContext.AddCellFormat(cellFormat, formatName);
                    }
                }

                context.ApplyStyleToCell(formatName);
            });
        }

        /// <summary>
        /// Applies styling from <see cref="INumberingFormatProvider"/>.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <param name="autoRegister">Use format registration.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer ApplyStyle<TPropertyRenderer>(
            this TPropertyRenderer propertyRenderer,
            INumberingFormatProvider formatProvider,
            bool autoRegister = true)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer.ConfigureCell(context =>
            {
                var numberingFormatName = formatProvider.Name;
                var cellFormatName = $"CellFormat.{numberingFormatName}";

                if (autoRegister)
                {
                    if (context.DocumentContext.GetNumberingFormatIndex(numberingFormatName) == 0)
                    {
                        // Register NumberingFormat
                        NumberingFormat numberingFormat = formatProvider.CreateFormat(context.DocumentContext);
                        context.DocumentContext.AddNumberingFormat(numberingFormat, numberingFormatName);

                        // Register CellFormat
                        CellFormat? cellFormat = context.DocumentContext.TryGetCellFormat(cellFormatName);
                        if (cellFormat == null)
                        {
                            cellFormat = new CellFormat
                            {
                                NumberFormatId = numberingFormat.NumberFormatId,
                                ApplyNumberFormat = true,
                            };
                            context.DocumentContext.AddCellFormat(cellFormat, cellFormatName);
                        }
                    }
                }

                context.ApplyStyleToCell(cellFormatName);
            });
        }

        /// <summary>
        /// Sets date format as Excel Serial Format, applies "Date" style.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <param name="registeredStyleName">One of date style formats: DateTime, Date, Time.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetExcelSerialDateFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer, string registeredStyleName)
            where TPropertyRenderer : IPropertyRenderer
        {
            propertyRenderer
                .SetExcelType(CellValues.Number)
                .ConfigureRenderer(options => options.CustomRender = (property, container) =>
                {
                    object? valueUntyped = container.GetValueUntyped(property, container.SearchOptions.UseDefaultValue(false).ReturnNull());
                    return valueUntyped.ToExcelSerialDateAsString();
                })
                .ConfigureCell(context => context.ApplyStyleToCell(registeredStyleName));

            return propertyRenderer;
        }

        /// <summary>
        /// Sets date format as Excel Serial Format, applies "Date" style.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetDateSerialFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer =>
            propertyRenderer.SetExcelSerialDateFormat("Date");

        /// <summary>
        /// Sets date format as Excel Serial Format, applies "Time" style.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetTimeSerialFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer =>
            propertyRenderer.SetExcelSerialDateFormat("Time");

        /// <summary>
        /// Sets date format as Excel Serial Format, applies "DateTime" style.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetDateTimeSerialFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer =>
            propertyRenderer.SetExcelSerialDateFormat("DateTime");

        /// <summary>
        /// Sets date format as ISO string yyyy-MM-dd.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetDateIsoFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer
                .SetExcelType(null)
                .ConfigureTyped(renderer => renderer.SetFormat("yyyy-MM-dd"));
        }

        /// <summary>
        /// Sets date format as ISO string HH:mm:ss.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetTimeIsoFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer
                .SetExcelType(null)
                .ConfigureTyped(renderer => renderer.SetFormat("HH:mm:ss"));
        }

        /// <summary>
        /// Sets date format as ISO string yyyy-MM-ddTHH:mm:ss.
        /// </summary>
        /// <typeparam name="TPropertyRenderer">Property renderer type.</typeparam>
        /// <param name="propertyRenderer">Property renderer.</param>
        /// <returns>The same renderer instance.</returns>
        public static TPropertyRenderer SetDateTimeIsoFormat<TPropertyRenderer>(this TPropertyRenderer propertyRenderer)
            where TPropertyRenderer : IPropertyRenderer
        {
            return propertyRenderer
                .SetExcelType(null)
                .ConfigureTyped(renderer => renderer.SetFormat("yyyy-MM-ddTHH:mm:ss"));
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
                        context.AddCellFormat(formatProvider.CreateFormat(context), formatProvider.Name);
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
                            context.AddCellFormat(instance.CreateFormat(context), instance.Name);
                        }
                    })
                : excelMetadata;
        }

        /// <summary>
        /// Applies style to cell.
        /// Register style with <see cref="ExcelBuilderExtensions.AddCellFormat"/> and other Add methods..
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
        /// Register style with <see cref="ExcelBuilderExtensions.AddCellFormat"/> and other Add methods..
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
        public static UInt32Value GetMergedStyleIndex(
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

            return documentContext.Cache.UInt32Value.GetOrAdd(mergedStyleIndex, value => new UInt32Value(value));
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
    }
}

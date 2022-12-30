// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Metadata.OpenXml.Excel.Parsing;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Extension methods for excel building.
    /// </summary>
    public static class ExcelBuilderExtensions
    {
        /// <summary>
        /// Gets or creates OpenXml SheetViews.
        /// </summary>
        /// <param name="workSheet">Source workSheet.</param>
        /// <returns><see cref="SheetViews"/> instance.</returns>
        public static SheetViews GetOrCreateSheetViews(this Worksheet workSheet)
        {
            SheetViews sheetViews = workSheet.GetFirstChild<SheetViews>();
            return sheetViews ?? new SheetViews();
        }

        /// <summary>
        /// Gets sheet count.
        /// </summary>
        /// <param name="workbookPart">Source workbookPart.</param>
        /// <returns>Sheet count.</returns>
        public static uint GetSheetCount(this WorkbookPart workbookPart) =>
            (uint)workbookPart.Workbook.Sheets.ChildElements.Count;

        /// <summary>
        /// Gets OpenXml <see cref="SharedStringTablePart"/> from document.
        /// </summary>
        /// <param name="documentContext">Source document context.</param>
        /// <param name="autoCreate">Create if not exists.</param>
        /// <returns><see cref="SharedStringTablePart"/> instance.</returns>
        public static SharedStringTablePart GetWorkbookSharedStringsPart(this DocumentContext documentContext, bool autoCreate = true)
        {
            WorkbookPart workbookPart = documentContext.Document.WorkbookPart;
            SharedStringTablePart workbookSharedStringsPart = workbookPart.SharedStringTablePart;

            if (autoCreate && workbookSharedStringsPart == null)
            {
                workbookSharedStringsPart = workbookPart.AddNewPart<SharedStringTablePart>("stringsPart");
            }

            return workbookSharedStringsPart;
        }

        /// <summary>
        /// Gets OpenXml <see cref="SharedStringTable"/> from document.
        /// </summary>
        /// <param name="documentContext">Source document context.</param>
        /// <param name="autoCreate">Create if not exists.</param>
        /// <returns><see cref="SharedStringTable"/> instance.</returns>
        public static SharedStringTable GetSharedStringTable(this DocumentContext documentContext, bool autoCreate = true)
        {
            SharedStringTablePart workbookSharedStringsPart = documentContext.GetWorkbookSharedStringsPart(autoCreate);

            if (autoCreate && workbookSharedStringsPart.SharedStringTable == null)
            {
                SharedStringTable sharedStringTable = new SharedStringTable();
                sharedStringTable.Count = UInt32Value.FromUInt32(0);
                sharedStringTable.UniqueCount = UInt32Value.FromUInt32(0);
                workbookSharedStringsPart.SharedStringTable = sharedStringTable;
            }

            return workbookSharedStringsPart.SharedStringTable;
        }

        /// <summary>
        /// Freezes top rows.
        /// </summary>
        /// <param name="workSheet">Source workSheet.</param>
        /// <param name="rowNum">Number or rows to freeze.</param>
        /// <returns>The same <paramref name="workSheet"/> instance.</returns>
        public static Worksheet FreezeTopRow(this Worksheet workSheet, int rowNum = 1)
        {
            SheetViews sheetViews = workSheet.GetOrCreateSheetViews();

            SheetView sheetView = sheetViews.GetFirstChild<SheetView>();

            // the freeze pane
            Pane pane = new Pane
            {
                VerticalSplit = rowNum,
                TopLeftCell = $"A{rowNum + 1}",
                ActivePane = PaneValues.BottomLeft,
                State = PaneStateValues.Frozen,
            };

            Selection selection = new Selection
            {
                Pane = PaneValues.BottomLeft,
                ActiveCell = pane.TopLeftCell,
                SequenceOfReferences = new ListValue<StringValue>() { InnerText = pane.TopLeftCell },
            };

            sheetView.AppendChild(pane);
            sheetView.AppendChild(selection);

            return workSheet;
        }

        /// <summary>
        /// Gets or adds shared string and returns its index.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="text">Text to add.</param>
        /// <returns>Index in excel SharedStringTable.</returns>
        public static string? GetOrAddSharedString(this DocumentContext documentContext, string? text)
        {
            if (text == null)
                return null;

            if (!documentContext.Cache.SharedStringTable.TryGetValue(text, out string stringIndex))
            {
                SharedStringTable sharedStringTable = documentContext.GetSharedStringTable();

                SharedStringItem sharedStringItem = new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text { Text = text });
                sharedStringTable.AppendChild(sharedStringItem);
                sharedStringTable.Count += 1;
                sharedStringTable.UniqueCount += 1;

                stringIndex = (sharedStringTable.Count - 1).ToString();
                documentContext.Cache.SharedStringTable.TryAdd(text, stringIndex);
            }

            return stringIndex;
        }

        /// <summary>
        /// Gets <see cref="WorkbookStylesPart"/> for document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="autoCreate">Create if not exists.</param>
        /// <returns><see cref="WorkbookStylesPart"/> instance.</returns>
        public static WorkbookStylesPart GetWorkbookStylesPart(this DocumentContext documentContext, bool autoCreate = true)
        {
            WorkbookPart workbookPart = documentContext.Document.WorkbookPart;
            WorkbookStylesPart workbookStylesPart = workbookPart.WorkbookStylesPart;

            if (autoCreate && workbookStylesPart == null)
            {
                workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("stylesPart");
            }

            return workbookStylesPart;
        }

        /// <summary>
        /// Gets <see cref="Stylesheet"/> for document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="autoCreate">Create if not exists.</param>
        /// <returns><see cref="Stylesheet"/> instance.</returns>
        public static Stylesheet GetStylesheet(this DocumentContext documentContext, bool autoCreate = true)
        {
            WorkbookStylesPart workbookStylesPart = documentContext.GetWorkbookStylesPart(autoCreate);

            if (autoCreate && workbookStylesPart.Stylesheet == null)
            {
                workbookStylesPart.Stylesheet = new Stylesheet();
            }

            return workbookStylesPart.Stylesheet;
        }

        /// <summary>
        /// Sets attached name to element.
        /// </summary>
        /// <typeparam name="TOpenXmlElement">OpenXmlElement.</typeparam>
        /// <param name="element">Element.</param>
        /// <param name="name">Name to attach.</param>
        /// <returns>The same instance.</returns>
        public static TOpenXmlElement SetStyleSheetName<TOpenXmlElement>(this TOpenXmlElement element, string name)
            where TOpenXmlElement : OpenXmlElement
        {
            element.AsMetadataProvider().SetMetadata("StyleSheetName", name);
            return element;
        }

        /// <summary>
        /// Gets attached name.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns>Name attached to element.</returns>
        public static string GetStyleSheetName(this OpenXmlElement element)
        {
            return element.AsMetadataProvider().GetMetadata<string>("StyleSheetName") ?? $"StyleSheetName_{element.GetHashCode()}";
        }

        /// <summary>
        /// Adds OpenXml <see cref="Font"/> to document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="font">Font to add.</param>
        /// <param name="name">Name attached to font.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddFont(this DocumentContext documentContext, Font font, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Fonts == null)
                stylesheet.Fonts = new Fonts();

            stylesheet.Fonts.AppendChild(font.SetStyleSheetName(name));
            stylesheet.Fonts.Count = (uint)stylesheet.Fonts.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="Fill"/> to document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="fill">Fill to add.</param>
        /// <param name="name">Name attached to fill.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddFill(this DocumentContext documentContext, Fill fill, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Fills == null)
                stylesheet.Fills = new Fills();

            stylesheet.Fills.AppendChild(fill.SetStyleSheetName(name));
            stylesheet.Fills.Count = (uint)stylesheet.Fills.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="Border"/> to document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="border">Border to add.</param>
        /// <param name="name">Name attached to border.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddBorder(this DocumentContext documentContext, Border border, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Borders == null)
                stylesheet.Borders = new Borders();

            stylesheet.Borders.AppendChild(border.SetStyleSheetName(name));
            stylesheet.Borders.Count = (uint)stylesheet.Borders.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="CellFormat"/> to document stylesheet CellStyleFormats.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="cellFormat">CellFormat to add.</param>
        /// <param name="name">Name attached to cellFormat.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddCellStyleFormat(this DocumentContext documentContext, CellFormat cellFormat, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellStyleFormats == null)
                stylesheet.CellStyleFormats = new CellStyleFormats();

            stylesheet.CellStyleFormats.AppendChild(cellFormat.SetStyleSheetName(name));
            stylesheet.CellStyleFormats.Count = (uint)stylesheet.CellStyleFormats.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="CellFormat"/> to document stylesheet CellFormats.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="cellFormat">CellFormat to add.</param>
        /// <param name="name">Name attached to cellFormat.</param>
        /// <param name="replaceOldIfExists">Replace old if exists.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddCellFormat(this DocumentContext documentContext, CellFormat cellFormat, string name, bool replaceOldIfExists = true)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellFormats == null)
                stylesheet.CellFormats = new CellFormats();

            if (replaceOldIfExists)
            {
                var existent = documentContext.TryGetCellFormat(name);
                if (existent != null)
                    stylesheet.CellFormats.RemoveChild(existent);
            }

            stylesheet.CellFormats.AppendChild(cellFormat.SetStyleSheetName(name));
            stylesheet.CellFormats.Count = (uint)stylesheet.CellFormats.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="CellStyle"/> to document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="cellStyle">CellStyle to add.</param>
        /// <param name="name">Name attached to cellStyle.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddCellStyle(this DocumentContext documentContext, CellStyle cellStyle, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellStyles == null)
                stylesheet.CellStyles = new CellStyles();

            stylesheet.CellStyles.AppendChild(cellStyle.SetStyleSheetName(name));
            stylesheet.CellStyles.Count = (uint)stylesheet.CellStyles.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Adds OpenXml <see cref="NumberingFormat"/> to document.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="numberingFormat">NumberingFormat to add.</param>
        /// <param name="name">Name attached to numberingFormat.</param>
        /// <returns>The same document context.</returns>
        public static DocumentContext AddNumberingFormat(this DocumentContext documentContext, NumberingFormat numberingFormat, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.NumberingFormats == null)
                stylesheet.NumberingFormats = new NumberingFormats();

            stylesheet.NumberingFormats.AppendChild(numberingFormat.SetStyleSheetName(name));
            stylesheet.NumberingFormats.Count = (uint)stylesheet.NumberingFormats.ChildElements.Count;

            return documentContext;
        }

        /// <summary>
        /// Gets <see cref="Font"/> index by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetFontIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.Fonts == null)
                return 0;

            return stylesheet.Fonts.GetChildren<Font>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
        }

        /// <summary>
        /// Gets <see cref="Fill"/> index by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetFillIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.Fills == null)
                return 0;

            return stylesheet.Fills.GetChildren<Fill>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
        }

        /// <summary>
        /// Gets <see cref="Border"/> index by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetBorderIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.Borders == null)
                return 0;

            return stylesheet.Borders.GetChildren<Border>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
        }

        /// <summary>
        /// Gets <see cref="NumberingFormat"/> index by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetNumberingFormatIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.NumberingFormats == null)
                return 0;

            return stylesheet.NumberingFormats.GetChildren<NumberingFormat>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
        }

        /// <summary>
        /// Gets <see cref="NumberingFormat.NumberFormatId"/> by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetNumberingFormatId(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.NumberingFormats == null)
                return 0;

            return stylesheet
                .NumberingFormats
                .ChildElements
                .OfType<NumberingFormat>()
                .FirstOrDefault(item => item.GetStyleSheetName() == name)
                ?.NumberFormatId;
        }

        /// <summary>
        /// Gets <see cref="CellFormat"/> index from <see cref="Stylesheet.CellStyleFormats"/> by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetCellStyleFormatIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.CellStyleFormats == null)
                return 0;

            var cellStyleFormatIndex = stylesheet.CellStyleFormats.GetChildren<CellFormat>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
            return cellStyleFormatIndex;
        }

        /// <summary>
        /// Gets <see cref="CellFormat"/> index from <see cref="Stylesheet.CellFormats"/> by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetCellFormatIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.CellFormats == null)
                return 0;

            var cellFormatIndex = stylesheet.CellFormats.GetChildren<CellFormat>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
            return cellFormatIndex;
        }

        /// <summary>
        /// Gets <see cref="CellFormat"/> index from <see cref="Stylesheet.CellStyles"/> by attached name.
        /// </summary>
        /// <param name="documentContext">Source document.</param>
        /// <param name="name">Name to search.</param>
        /// <returns>Index or 0 if not found.</returns>
        public static uint GetCellStyleIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.CellStyles == null)
                return 0;

            var cellStyleIndex = stylesheet.CellStyles.GetChildren<CellFormat>().UintIndexOrZero(item => item.GetStyleSheetName() == name);
            return cellStyleIndex;
        }

        /// <summary>
        /// Gets Optional <see cref="CellFormat"/> from document stylesheet by attached name.
        /// </summary>
        /// <param name="documentContext">Source document context.</param>
        /// <param name="name">CellFormat attached name.</param>
        /// <returns>Optional <see cref="CellFormat"/> instance.</returns>
        public static CellFormat? TryGetCellFormat(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            if (stylesheet.CellFormats == null)
                return null;

            CellFormat? cellFormat = stylesheet.CellFormats.GetChildren<CellFormat>().FirstOrDefault(item => item.GetStyleSheetName() == name);
            return cellFormat;
        }

        /// <summary>
        /// Gets <see cref="CellFormat"/> from document stylesheet by attached name.
        /// </summary>
        /// <param name="documentContext">Source document context.</param>
        /// <param name="name">CellFormat attached name.</param>
        /// <returns><see cref="CellFormat"/> instance or throws <see cref="ExceptionWithError{ExcelError}"/>.</returns>
        public static CellFormat GetCellFormat(this DocumentContext documentContext, string name)
        {
            CellFormat? cellFormat = TryGetCellFormat(documentContext, name);
            if (cellFormat == null)
                ExcelErrors.CellFormatNotFound(name).Throw();
            return cellFormat;
        }

        /// <summary>
        /// Gets <see cref="CellFormat"/> from document stylesheet by index.
        /// </summary>
        /// <param name="documentContext">Source document context.</param>
        /// <param name="index">Zero base index.</param>
        /// <returns><see cref="CellFormat"/> instance.</returns>
        public static CellFormat GetCellFormat(this DocumentContext documentContext, int index)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            CellFormat? cellFormat = stylesheet.CellFormats.TryGetElementAt<CellFormat>(index);

            if (index == 0 && cellFormat != null)
                ExcelErrors.CellFormatsIsNotRegistered.Throw();

            return cellFormat ?? documentContext.GetCellFormat(0);
        }

        internal static TElement? TryGetElementAt<TElement>(this OpenXmlCompositeElement element, int index)
            where TElement : OpenXmlElement
        {
            return element.GetChildren<TElement>().Skip(index).FirstOrDefault();
        }

        internal static uint UintIndexOrZero<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            uint i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return i;
                i++;
            }

            return 0;
        }
    }
}

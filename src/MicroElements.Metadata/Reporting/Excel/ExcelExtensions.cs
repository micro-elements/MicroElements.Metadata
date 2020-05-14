// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Extension methods for excel building.
    /// </summary>
    public static class ExcelExtensions
    {
        public static SheetViews GetOrCreateSheetViews(this Worksheet workSheet)
        {
            SheetViews sheetViews = workSheet.GetFirstChild<SheetViews>();
            return sheetViews ?? new SheetViews();
        }

        public static uint GetSheetCount(this WorkbookPart workbookPart) =>
            (uint)workbookPart.Workbook.Sheets.ChildElements.Count;

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

            sheetView.Append(pane);
            sheetView.Append(selection);

            return workSheet;
        }

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

        public static SharedStringTable GetSharedStringTable(this DocumentContext documentContext, bool autoCreate = true)
        {
            SharedStringTablePart workbookSharedStringsPart = documentContext.GetWorkbookSharedStringsPart(autoCreate);

            if (autoCreate && workbookSharedStringsPart.SharedStringTable == null)
            {
                SharedStringTable sharedStringTable = new SharedStringTable();
                workbookSharedStringsPart.SharedStringTable = sharedStringTable;
            }

            return workbookSharedStringsPart.SharedStringTable;
        }

        public static string GetOrAddSharedString(this DocumentContext documentContext, string text)
        {
            if (text == null)
                return null;

            if (!documentContext.SharedStringTable.TryGetValue(text, out string stringIndex))
            {
                SharedStringTable sharedStringTable = documentContext.GetSharedStringTable();

                SharedStringItem sharedStringItem = new SharedStringItem();
                Text text1 = new Text { Text = text };
                sharedStringItem.Append(text1);

                sharedStringTable.AppendChild(sharedStringItem);

                uint itemCount = (uint)sharedStringTable.ChildElements.Count;
                sharedStringTable.Count = itemCount;
                sharedStringTable.UniqueCount = itemCount;

                stringIndex = (itemCount-1).ToString();
                documentContext.SharedStringTable.Add(text, stringIndex);
            }

            return stringIndex;
        }

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

        public static Stylesheet GetStylesheet(this DocumentContext documentContext, bool autoCreate = true)
        {
            WorkbookStylesPart workbookStylesPart = documentContext.GetWorkbookStylesPart(autoCreate);

            if (autoCreate && workbookStylesPart.Stylesheet == null)
            {
                workbookStylesPart.Stylesheet = new Stylesheet();
            }

            return workbookStylesPart.Stylesheet;
        }

        internal static void SetAttachedName(this object target, string name)
        {
            target.GetInstanceMetadata().SetMetadata("StyleSheetName", name);
        }

        internal static string GetAttachedName(this object target)
        {
            return target.GetInstanceMetadata().GetMetadata<string>("StyleSheetName");
        }

        internal static uint IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
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

        public static DocumentContext AddFont(this DocumentContext documentContext, Font font, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Fonts == null)
                stylesheet.Fonts = new Fonts();

            font.SetAttachedName(name);

            stylesheet.Fonts.AppendChild(font);
            stylesheet.Fonts.Count = (uint)stylesheet.Fonts.ChildElements.Count;

            return documentContext;
        }

        public static DocumentContext AddFill(this DocumentContext documentContext, Fill fill, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Fills == null)
                stylesheet.Fills = new Fills();

            fill.SetAttachedName(name);

            stylesheet.Fills.AppendChild(fill);
            stylesheet.Fills.Count = (uint)stylesheet.Fills.ChildElements.Count;

            return documentContext;
        }

        public static DocumentContext AddBorder(this DocumentContext documentContext, Border border, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.Borders == null)
                stylesheet.Borders = new Borders();

            border.SetAttachedName(name);

            stylesheet.Borders.AppendChild(border);
            stylesheet.Borders.Count = (uint)stylesheet.Borders.ChildElements.Count;

            return documentContext;
        }

        public static DocumentContext AddCellStyleFormat(this DocumentContext documentContext, CellFormat cellFormat, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellStyleFormats == null)
                stylesheet.CellStyleFormats = new CellStyleFormats();

            cellFormat.SetAttachedName(name);

            stylesheet.CellStyleFormats.AppendChild(cellFormat);
            stylesheet.CellStyleFormats.Count = (uint)stylesheet.CellStyleFormats.ChildElements.Count;

            return documentContext;
        }

        public static DocumentContext AddCellFormat(this DocumentContext documentContext, CellFormat cellFormat, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellFormats == null)
                stylesheet.CellFormats = new CellFormats();

            cellFormat.SetAttachedName(name);

            stylesheet.CellFormats.AppendChild(cellFormat);
            stylesheet.CellFormats.Count = (uint)stylesheet.CellFormats.ChildElements.Count;

            return documentContext;
        }

        public static DocumentContext AddCellStyle(this DocumentContext documentContext, CellStyle cellStyle, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();

            if (stylesheet.CellStyles == null)
                stylesheet.CellStyles = new CellStyles();

            cellStyle.SetAttachedName(name);

            stylesheet.CellStyles.AppendChild(cellStyle);
            stylesheet.CellStyles.Count = (uint)stylesheet.CellStyles.ChildElements.Count;

            return documentContext;
        }

        public static uint GetFontIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.Fonts.ChildElements.OfType<Font>().IndexOf(item => item.GetAttachedName() == name);
        }

        public static uint GetFillIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.Fills.ChildElements.OfType<Fill>().IndexOf(item => item.GetAttachedName() == name);
        }

        public static uint GetBorderIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.Borders.ChildElements.OfType<Border>().IndexOf(item => item.GetAttachedName() == name);
        }

        public static uint GetCellStyleFormatIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.CellStyleFormats.ChildElements.OfType<CellFormat>().IndexOf(item => item.GetAttachedName() == name);
        }

        public static uint GetCellFormatIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.CellFormats.ChildElements.OfType<CellFormat>().IndexOf(item => item.GetAttachedName() == name);
        }

        public static uint GetCellStyleIndex(this DocumentContext documentContext, string name)
        {
            Stylesheet stylesheet = documentContext.GetStylesheet();
            return stylesheet.CellStyles.ChildElements.OfType<CellFormat>().IndexOf(item => item.GetAttachedName() == name);
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.CodeContracts;
using MicroElements.Metadata.OpenXml.Excel.Parsing;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Represents OpenXml sheet context.
    /// </summary>
    public class SheetContext
    {
        /// <summary>
        /// Gets OpenXml document context.
        /// </summary>
        public DocumentContext DocumentContext { get; }

        /// <summary>
        /// Gets <see cref="WorksheetPart"/>.
        /// </summary>
        public WorksheetPart WorksheetPart { get; }

        /// <summary>
        /// Gets sheet configuration metadata.
        /// </summary>
        public IExcelMetadata SheetMetadata { get; }

        /// <summary>
        /// Gets renderer for sheet.
        /// </summary>
        public IReportRenderer ReportRenderer { get; }

        /// <summary>
        /// Gets document metadata.
        /// </summary>
        public IExcelMetadata DocumentMetadata => DocumentContext.DocumentMetadata;

        /// <summary>
        /// Gets a value indicating whether sheet should be transposed.
        /// </summary>
        public bool IsTransposed => ExcelMetadata.GetFirstDefinedValue(ExcelMetadata.Transpose, SheetMetadata);

        /// <summary>
        /// Gets a value indicating whether sheet should not be transposed.
        /// </summary>
        public bool IsNotTransposed => !IsTransposed;

        /// <summary>
        /// Gets columns for the sheet.
        /// </summary>
        public IReadOnlyList<ColumnContext> Columns { get; internal set; }

        /// <summary>
        /// Gets <see cref="DocumentFormat.OpenXml.Spreadsheet.SheetData"/> part.
        /// </summary>
        public SheetData SheetData { get; internal set; }

        /// <summary>
        /// Gets <see cref="DocumentFormat.OpenXml.Spreadsheet.Sheet"/> part.
        /// </summary>
        public Sheet Sheet { get; internal set; }

        /// <summary>
        /// Gets <see cref="Sheet"/> as <see cref="ExcelElement{TOpenXmlElement}"/>.
        /// </summary>
        public ExcelElement<Sheet> SheetElement => new ExcelElement<Sheet>(DocumentContext.Document, Sheet);

        /// <summary>
        /// Initializes a new instance of the <see cref="SheetContext"/> class.
        /// </summary>
        /// <param name="documentContext">Document context.</param>
        /// <param name="worksheetPart">WorksheetPart.</param>
        /// <param name="sheetMetadata">Sheet configuration metadata.</param>
        /// <param name="reportRenderer">Renderer for sheet.</param>
        public SheetContext(
            DocumentContext documentContext,
            WorksheetPart worksheetPart,
            IExcelMetadata sheetMetadata,
            IReportRenderer reportRenderer)
        {
            DocumentContext = documentContext.AssertArgumentNotNull(nameof(documentContext));
            WorksheetPart = worksheetPart.AssertArgumentNotNull(nameof(worksheetPart));
            SheetMetadata = sheetMetadata.AssertArgumentNotNull(nameof(sheetMetadata));
            ReportRenderer = reportRenderer.AssertArgumentNotNull(nameof(reportRenderer));
        }
    }
}

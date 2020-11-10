// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Context for column rendering.
    /// </summary>
    public class ColumnContext
    {
        /// <summary>
        /// Gets owner sheet context.
        /// </summary>
        public SheetContext SheetContext { get; }

        /// <summary>
        /// Gets column metadata.
        /// </summary>
        public IExcelMetadata ColumnMetadata { get; }

        /// <summary>
        /// Gets <see cref="IPropertyRenderer"/> to render column cells.
        /// </summary>
        public IPropertyRenderer PropertyRenderer { get; }

        /// <summary>
        /// Gets OpenXml column.
        /// </summary>
        public Column Column { get; internal set; }

        /// <summary>
        /// Gets document metadata.
        /// </summary>
        public IExcelMetadata DocumentMetadata => SheetContext.DocumentMetadata;

        /// <summary>
        /// Gets sheet metadata.
        /// </summary>
        public IExcelMetadata SheetMetadata => SheetContext.SheetMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnContext"/> class.
        /// </summary>
        /// <param name="sheetContext">Owner sheet context.</param>
        /// <param name="columnMetadata">Column metadata.</param>
        /// <param name="propertyRenderer"><see cref="IPropertyRenderer"/> to render column cells.</param>
        public ColumnContext(
            SheetContext sheetContext,
            IExcelMetadata columnMetadata,
            IPropertyRenderer propertyRenderer)
        {
            SheetContext = sheetContext.AssertArgumentNotNull(nameof(sheetContext));
            ColumnMetadata = columnMetadata.AssertArgumentNotNull(nameof(columnMetadata));
            PropertyRenderer = propertyRenderer.AssertArgumentNotNull(nameof(propertyRenderer));
        }
    }
}

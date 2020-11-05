// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    public class ColumnContext
    {
        public SheetContext SheetContext { get; }

        public IExcelMetadata DocumentMetadata => SheetContext.DocumentMetadata;

        public IExcelMetadata SheetMetadata => SheetContext.SheetMetadata;

        public IExcelMetadata ColumnMetadata { get; }

        public IPropertyRenderer PropertyRenderer { get; }

        public Column Column { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnContext"/> class.
        /// </summary>
        /// <param name="sheetContext"></param>
        /// <param name="columnMetadata"></param>
        /// <param name="propertyRenderer"></param>
        public ColumnContext(SheetContext sheetContext, IExcelMetadata columnMetadata, IPropertyRenderer propertyRenderer)
        {
            SheetContext = sheetContext.AssertArgumentNotNull(nameof(sheetContext));
            ColumnMetadata = columnMetadata.AssertArgumentNotNull(nameof(columnMetadata));
            PropertyRenderer = propertyRenderer.AssertArgumentNotNull(nameof(propertyRenderer));
        }
    }
}

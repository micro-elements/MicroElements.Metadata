// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// <see cref="CellFormat"/> provider.
    /// </summary>
    public interface ICellFormatProvider
    {
        /// <summary>
        /// Gets format name.
        /// This name is used for registering format in stylesheet.
        /// Use <see cref="ExcelBuilderExtensions.GetCellFormat(DocumentContext,string)"/> to het by name.
        /// </summary>
        string Name => GetType().ToString();

        /// <summary>
        /// Creates format.
        /// </summary>
        /// <param name="context">Document context.</param>
        /// <returns><see cref="CellFormat"/> instance.</returns>
        CellFormat CreateFormat(DocumentContext context);
    }
}

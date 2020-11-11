// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;
using MicroElements.Reporting.Excel;

namespace MicroElements.Reporting.Styling
{
    /// <summary>
    /// <see cref="CellFormat"/> provider.
    /// </summary>
    public interface ICellFormatProvider
    {
        /// <summary>
        /// Creates format.
        /// </summary>
        /// <param name="context">Document context.</param>
        /// <returns><see cref="CellFormat"/> instance.</returns>
        CellFormat CreateFormat(DocumentContext context);
    }
}

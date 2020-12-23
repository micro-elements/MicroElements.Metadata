// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// <see cref="NumberingFormat"/> provider.
    /// </summary>
    public interface INumberingFormatProvider
    {
        /// <summary>
        /// Gets format name.
        /// This name is used for registering format in stylesheet.
        /// Use <see cref="ExcelBuilderExtensions.GetNumberingFormatId(DocumentContext,string)"/> to get by name.
        /// </summary>
        string Name => GetType().ToString();

        /// <summary>
        /// Creates format.
        /// </summary>
        /// <param name="context">Document context.</param>
        /// <returns><see cref="NumberingFormat"/> instance.</returns>
        NumberingFormat CreateFormat(DocumentContext context);
    }
}

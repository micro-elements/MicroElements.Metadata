// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Excel export options.
    /// </summary>
    public interface IExportOptions
    {
        /// <summary>
        /// Output file path.
        /// </summary>
        string OutFilePath { get; }
    }

    /// <summary>
    /// Excel export options.
    /// </summary>
    public class ExportOptions : IExportOptions
    {
        /// <summary>
        /// Output file path.
        /// </summary>
        public string OutFilePath { get; }
    }
}

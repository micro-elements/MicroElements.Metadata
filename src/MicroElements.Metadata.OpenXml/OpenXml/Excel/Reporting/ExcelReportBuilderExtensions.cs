// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Extensions for ExcelReportBuilder.
    /// </summary>
    public static class ExcelReportBuilderExtensions
    {
        /// <summary>
        /// Adds report sheet if has any rows.
        /// </summary>
        /// <param name="reportBuilder">Source report builder.</param>
        /// <param name="reportProvider">Report provider for sheet.</param>
        /// <param name="rows">Rows for sheet.</param>
        /// <returns>The same report builder.</returns>
        public static ExcelReportBuilder AddReportSheetIfHasRows(
            this ExcelReportBuilder reportBuilder,
            IReportProvider reportProvider,
            IReadOnlyCollection<IPropertyContainer>? rows = null)
        {
            rows ??= reportProvider.GetReportRows().ToArray();
            return rows?.Count > 0 ? reportBuilder.AddReportSheet(reportProvider, rows) : reportBuilder;
        }
    }
}

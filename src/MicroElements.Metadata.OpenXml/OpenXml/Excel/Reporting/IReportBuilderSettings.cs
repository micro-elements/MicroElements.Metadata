// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Report builder settings.
    /// </summary>
    public interface IReportBuilderSettings
    {
        /// <summary>
        /// Gets string provider to support string interning or caching for performance needs.
        /// </summary>
        IStringProvider StringProvider { get; }

        /// <summary>
        /// Cell factory.
        /// </summary>
        IOpenXmlCellFactory CellFactory { get; }
    }

    /// <summary>
    /// Report builder settings.
    /// </summary>
    public sealed class ReportBuilderSettings : IReportBuilderSettings
    {
        /// <inheritdoc />
        public IStringProvider StringProvider { get; }

        /// <inheritdoc />
        public IOpenXmlCellFactory CellFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBuilderSettings"/> class.
        /// </summary>
        /// <param name="stringProvider">Optional String provider to support string interning or caching for performance needs.</param>
        /// <param name="cellFactory">Cell factory.</param>
        public ReportBuilderSettings(
            IStringProvider? stringProvider = null, 
            IOpenXmlCellFactory? cellFactory = null)
        {
            StringProvider = stringProvider ?? new DefaultStringProvider();
            CellFactory = cellFactory ?? new DefaultOpenXmlCellFactory();
        }
    }
}

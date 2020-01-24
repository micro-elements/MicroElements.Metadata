// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property renderers.
    /// </summary>
    public interface IReportProvider
    {
        /// <summary>
        /// Gets report name.
        /// </summary>
        string ReportName { get; }

        /// <summary>
        /// Gets property renderers.
        /// </summary>
        IReadOnlyList<IPropertyRenderer> Renderers { get; }
    }

    /// <summary>
    /// Simple implementation for <see cref="IReportProvider"/>.
    /// </summary>
    public class ReportProvider : IReportProvider
    {
        /// <inheritdoc />
        public string ReportName { get; }

        /// <inheritdoc />
        IReadOnlyList<IPropertyRenderer> IReportProvider.Renderers => Renderers;

        /// <summary>
        /// Gets property renderers.
        /// </summary>
        protected List<IPropertyRenderer> Renderers { get; } = new List<IPropertyRenderer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportProvider"/> class.
        /// </summary>
        /// <param name="reportName">Report name.</param>
        public ReportProvider(string reportName = null)
        {
            ReportName = reportName ?? GetType().Name;
        }

        /// <summary>
        /// Adds new property renderer and returns reference for it.
        /// </summary>
        /// <typeparam name="T">Property name.</typeparam>
        /// <param name="property">Property to render.</param>
        /// <param name="targetName">Target name.</param>
        /// <returns>Reference for added renderer.</returns>
        protected PropertyRenderer<T> Add<T>(IProperty<T> property, string targetName = null)
        {
            var renderer = new PropertyRenderer<T>(property, targetName);
            Renderers.Add(renderer);
            return renderer;
        }
    }
}

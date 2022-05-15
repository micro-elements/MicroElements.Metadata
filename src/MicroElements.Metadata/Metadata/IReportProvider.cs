// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property renderers and optional report data.
    /// </summary>
    public interface IReportProvider : IReportRenderer
    {
        /// <summary>
        /// Gets data rows for report rendering.
        /// </summary>
        /// <returns><see cref="IPropertyContainer"/> enumeration.</returns>
        IEnumerable<IPropertyContainer> GetReportRows();
    }

    /// <summary>
    /// Simple implementation for <see cref="IReportProvider"/>.
    /// </summary>
    [DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
    public class ReportProvider : IReportProvider
    {
        private readonly List<IPropertyRenderer> _renderers = new List<IPropertyRenderer>();
        private readonly List<IPropertyContainer> _rows = new List<IPropertyContainer>();

        /// <inheritdoc />
        public string ReportName { get; }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyRenderer> Renderers => _renderers;

        /// <inheritdoc />
        public IEnumerable<IPropertyContainer> GetReportRows()
        {
            return _rows;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportProvider"/> class.
        /// </summary>
        /// <param name="reportName">Report name.</param>
        /// <param name="propertyRenderers">Optional property renderers to add.</param>
        /// <param name="rows">Optional report data.</param>
        public ReportProvider(
            string? reportName = null,
            IEnumerable<IPropertyRenderer>? propertyRenderers = null,
            IEnumerable<IPropertyContainer>? rows = null)
        {
            ReportName = reportName ?? GetType().Name;

            if (propertyRenderers != null)
                _renderers.AddRange(propertyRenderers);

            if (rows != null)
                _rows.AddRange(rows);
        }

        /// <summary>
        /// Adds new property renderer and returns reference for it.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to render.</param>
        /// <param name="targetName">Target name.</param>
        /// <returns>Reference for added renderer.</returns>
        public PropertyRenderer<T> Add<T>(IProperty<T> property, string? targetName = null)
        {
            var renderer = new PropertyRenderer<T>(property, targetName);
            _renderers.Add(renderer);
            return renderer;
        }

        /// <summary>
        /// Adds property renderer.
        /// </summary>
        /// <param name="renderer">Renderer to add.</param>
        /// <returns>The same provider instance for chaining.</returns>
        public ReportProvider AddRenderer(IPropertyRenderer renderer)
        {
            _renderers.Add(renderer.AssertArgumentNotNull(nameof(renderer)));
            return this;
        }

        /// <summary>
        /// Adds property renderers.
        /// </summary>
        /// <param name="renderers">Renderer to add.</param>
        /// <returns>The same provider instance for chaining.</returns>
        public ReportProvider AddRenderers(IEnumerable<IPropertyRenderer> renderers)
        {
            _renderers.AddRange(renderers.AssertArgumentNotNull(nameof(renderers)));
            return this;
        }

        /// <summary>
        /// Adds data rows.
        /// </summary>
        /// <param name="rows">Data rows.</param>
        /// <returns>The same provider instance for chaining.</returns>
        public ReportProvider AddRows(IEnumerable<IPropertyContainer> rows)
        {
            _rows.AddRange(rows.AssertArgumentNotNull(nameof(rows)));
            return this;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property renderers.
    /// </summary>
    public interface IReportProvider : IMetadataProvider
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
        private readonly List<IPropertyRenderer> _renderers = new List<IPropertyRenderer>();

        /// <inheritdoc />
        public string ReportName { get; }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyRenderer> Renderers => _renderers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportProvider"/> class.
        /// </summary>
        /// <param name="reportName">Report name.</param>
        /// <param name="propertyRenderers">Optional property renderers to add.</param>
        public ReportProvider(string reportName = null, IEnumerable<IPropertyRenderer> propertyRenderers = null)
        {
            ReportName = reportName ?? GetType().Name;

            if (propertyRenderers != null)
                _renderers.AddRange(propertyRenderers);
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
            _renderers.Add(renderer);
            return renderer;
        }
    }

    /// <summary>
    /// Report provider that can be easily implemented with yield return.
    /// </summary>
    public abstract class EnumerableReportProvider : IReportProvider
    {
        /// <inheritdoc />
        public string ReportName { get; }

        /// <inheritdoc />
        public IReadOnlyList<IPropertyRenderer> Renderers => GetRenderers().ToList();

        /// <summary>
        /// Gets property renderers.
        /// </summary>
        /// <returns>Enumeration of property renderers.</returns>
        public abstract IEnumerable<IPropertyRenderer> GetRenderers();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableReportProvider"/> class.
        /// </summary>
        /// <param name="reportName">Report name.</param>
        protected EnumerableReportProvider(string reportName)
        {
            ReportName = reportName;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides <see cref="IValueFormatter"/> list to perform formatting.
    /// </summary>
    public interface IValueFormatterProvider
    {
        /// <summary>
        /// Gets <see cref="IValueFormatter"/> enumeration.
        /// </summary>
        /// <returns><see cref="IValueFormatter"/> enumeration.</returns>
        IEnumerable<IValueFormatter> GetFormatters();
    }

    /// <summary>
    /// <see cref="IValueFormatterProvider"/> implemented with internal list.
    /// </summary>
    public class ValueFormatterProvider : IValueFormatterProvider
    {
        private readonly List<IValueFormatter> _formatters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFormatterProvider"/> class.
        /// </summary>
        /// <param name="formatters">Formatters enumeration.</param>
        public ValueFormatterProvider(IEnumerable<IValueFormatter> formatters)
        {
            formatters.AssertArgumentNotNull(nameof(formatters));

            _formatters = new List<IValueFormatter>(formatters);
        }

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            return _formatters;
        }
    }

    /// <summary>
    /// Extensions fro <see cref="IValueFormatterProvider"/>.
    /// </summary>
    public static class ValueFormatterProviderExtensions
    {
        /// <summary>
        /// Converts enumeration to <see cref="IValueFormatterProvider"/>.
        /// </summary>
        /// <param name="formatters">Formatters enumeration.</param>
        /// <returns><see cref="IValueFormatterProvider"/> instance.</returns>
        public static IValueFormatterProvider ToValueFormatterProvider(this IEnumerable<IValueFormatter>? formatters)
        {
            return new ValueFormatterProvider(formatters ?? Array.Empty<IValueFormatter>());
        }
    }
}

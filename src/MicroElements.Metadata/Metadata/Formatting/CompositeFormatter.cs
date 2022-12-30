// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Composite formatter with internal formatter cache by type.
    /// </summary>
    public class CompositeFormatter : IValueFormatter, IValueFormatterProvider
    {
        private readonly IReadOnlyCollection<IValueFormatter> _formatters;
        private readonly ConcurrentDictionary<Type, IValueFormatter> _formattersCache = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFormatter"/> class.
        /// </summary>
        /// <param name="formatters">Optional formatters.</param>
        public CompositeFormatter(IEnumerable<IValueFormatter>? formatters = null)
        {
            _formatters = formatters?.ToArray() ?? Array.Empty<IValueFormatter>();
        }

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters() => _formatters;

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return _formatters.Any(formatter => formatter.CanFormat(valueType));
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            valueType = ValueFormatter.GetTargetType(value, valueType);

            IValueFormatter valueFormatter = _formattersCache.GetOrAdd(valueType, GetFormatter);
            return valueFormatter.Format(value, valueType);
        }

        private IValueFormatter GetFormatter(Type valueType)
        {
            IValueFormatter? valueFormatter = _formatters.FirstOrDefault(formatter => formatter.CanFormat(valueType));

            if (valueFormatter is null && Nullable.GetUnderlyingType(valueType) is { } underlyingType)
            {
                valueFormatter = _formatters.FirstOrDefault(formatter => formatter.CanFormat(underlyingType));

                if (valueFormatter != null)
                {
                    return new NullableFormatter(valueFormatter);
                }
            }

            if (valueFormatter != null)
            {
                return valueFormatter;
            }

            return DefaultToStringFormatter.Instance;
        }
    }

    /// <summary>
    /// CompositeFormatter extensions.
    /// </summary>
    public static class CompositeFormatterExtensions
    {
        /// <summary>
        /// Creates new <see cref="CompositeFormatter"/> with formatters added from <paramref name="formatters"/>.
        /// </summary>
        /// <param name="formatter">Source formatter.</param>
        /// <param name="formatters">Formatters to add.</param>
        /// <returns>New instance of <see cref="CompositeFormatter"/>.</returns>
        public static CompositeFormatter With(this CompositeFormatter formatter, IEnumerable<IValueFormatter> formatters)
        {
            formatter.AssertArgumentNotNull(nameof(formatter));
            formatters.AssertArgumentNotNull(nameof(formatters));

            return new CompositeFormatter(formatter.GetFormatters().Concat(formatters));
        }

        /// <summary>
        /// Creates new <see cref="CompositeFormatter"/> with formatters added from <paramref name="formatterProvider"/>.
        /// </summary>
        /// <param name="formatter">Source formatter.</param>
        /// <param name="formatterProvider">Formatters provider to add.</param>
        /// <returns>New instance of <see cref="CompositeFormatter"/>.</returns>
        public static CompositeFormatter With(this CompositeFormatter formatter, IValueFormatterProvider formatterProvider)
        {
            return formatter.With(formatterProvider.GetFormatters());
        }

        /// <summary>
        /// Creates new <see cref="CompositeFormatter"/> with formatters added from <paramref name="formatters"/>.
        /// </summary>
        /// <param name="formatter">Source formatter.</param>
        /// <param name="formatters">Formatters to add.</param>
        /// <returns>New instance of <see cref="CompositeFormatter"/>.</returns>
        public static CompositeFormatter With(this CompositeFormatter formatter, params IValueFormatter[] formatters)
        {
            return formatter.With((IEnumerable<IValueFormatter>)formatters);
        }
    }
}

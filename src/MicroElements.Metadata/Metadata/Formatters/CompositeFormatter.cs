// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Composite formatter with internal formatter cache by type.
    /// </summary>
    public class CompositeFormatter : IValueFormatter, IValueFormatterProvider
    {
        private readonly IReadOnlyCollection<IValueFormatter> _formatters;
        private readonly ConcurrentDictionary<Type, IValueFormatter> _formattersCache = new ConcurrentDictionary<Type, IValueFormatter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeFormatter"/> class.
        /// </summary>
        /// <param name="formatterProvider">Formatters.</param>
        public CompositeFormatter(IValueFormatterProvider formatterProvider)
        {
            formatterProvider.AssertArgumentNotNull(nameof(formatterProvider));

            _formatters = formatterProvider.GetFormatters().ToArray();
        }

        public CompositeFormatter(IEnumerable<IValueFormatter> formatters)
        {
            formatters.AssertArgumentNotNull(nameof(formatters));

            _formatters = formatters.ToArray();
        }

        public CompositeFormatter(params IValueFormatter[] formatters)
        {
            formatters.AssertArgumentNotNull(nameof(formatters));

            _formatters = formatters.ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<IValueFormatter> GetFormatters()
        {
            return _formatters;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType) => true;

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            // typeof(object) treats as type unknown
            if (valueType == typeof(object))
                valueType = value?.GetType() ?? typeof(object);

            IValueFormatter valueFormatter = _formattersCache.GetOrAdd(valueType, GetFormatter);
            return valueFormatter.Format(value, valueType);
        }

        private IValueFormatter GetFormatter(Type valueType)
        {
            IValueFormatter? valueFormatter = _formatters
                .FirstOrDefault(formatter => formatter.CanFormat(valueType));
            return valueFormatter ?? DefaultToStringFormatter.Instance;
        }


    }
}

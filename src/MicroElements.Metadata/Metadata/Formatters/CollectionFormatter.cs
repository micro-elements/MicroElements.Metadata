// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using MicroElements.Functional;

namespace MicroElements.Metadata.Formatters
{
    /// <summary>
    /// Formatter for collection types.
    /// </summary>
    public class CollectionFormatter : IValueFormatter
    {
        private readonly IValueFormatter _valueFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionFormatter"/> class.
        /// </summary>
        /// <param name="valueFormatter">Collection values formatter.</param>
        public CollectionFormatter(IValueFormatter valueFormatter)
        {
            valueFormatter.AssertArgumentNotNull(nameof(valueFormatter));

            _valueFormatter = valueFormatter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return valueType.IsAssignableTo(typeof(ICollection));
        }

        /// <inheritdoc />
        public string? Format(object? value, Type valueType)
        {
            if (value is ICollection collection)
            {
                return collection.FormatAsTuple(
                    startSymbol: "[",
                    endSymbol: "]",
                    formatValue: item => _valueFormatter.Format(item, typeof(object)));
            }

            return null;
        }
    }
}

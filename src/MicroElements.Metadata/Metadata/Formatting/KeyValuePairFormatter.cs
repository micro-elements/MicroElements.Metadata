// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.CodeContracts;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatter for <see cref="KeyValuePair{TKey,TValue}"/> with string key.
    /// </summary>
    public class KeyValuePairFormatter : IValueFormatter
    {
        private readonly IValueFormatter _valueFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePairFormatter"/> class.
        /// </summary>
        /// <param name="valueFormatter">Value formatter.</param>
        public KeyValuePairFormatter(IValueFormatter valueFormatter)
        {
            valueFormatter.AssertArgumentNotNull(nameof(valueFormatter));

            _valueFormatter = valueFormatter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return valueType.IsAssignableTo(typeof(KeyValuePair<string, object>));
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            if (value is KeyValuePair<string, object?> keyValuePair)
                return $"({keyValuePair.Key}: {_valueFormatter.TryFormat(keyValuePair.Value)})";

            return null;
        }
    }
}

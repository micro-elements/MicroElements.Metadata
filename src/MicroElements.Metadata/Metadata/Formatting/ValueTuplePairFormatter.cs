// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.CodeContracts;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Formatter for <see cref="ValueTuple{TKey,TValue}"/> with string key.
    /// </summary>
    public class ValueTuplePairFormatter : IValueFormatter
    {
        private readonly IValueFormatter _valueFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueTuplePairFormatter"/> class.
        /// </summary>
        /// <param name="valueFormatter">Value formatter.</param>
        public ValueTuplePairFormatter(IValueFormatter valueFormatter)
        {
            valueFormatter.AssertArgumentNotNull(nameof(valueFormatter));

            _valueFormatter = valueFormatter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return valueType.IsAssignableTo(typeof(ValueTuple<string, object>));
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            if (value is ValueTuple<string, object?> nameValueTuple)
                return $"({nameValueTuple.Item1}: {_valueFormatter.TryFormat(nameValueTuple.Item2)})";

            return null;
        }
    }
}

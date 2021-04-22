// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using MicroElements.CodeContracts;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Formatting
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
            return valueType.IsAssignableTo(typeof(ICollection)) || valueType.Name.StartsWith("IReadOnlyCollection");
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
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

    public class RuntimeFormatter : IValueFormatter
    {
        private Func<IValueFormatter> _formatterGetter;

        public RuntimeFormatter(Func<IValueFormatter> formatterGetter)
        {
            _formatterGetter = formatterGetter;
        }

        /// <inheritdoc />
        public bool CanFormat(Type valueType)
        {
            return _formatterGetter().CanFormat(valueType);
        }

        /// <inheritdoc />
        public string? Format(object? value, Type? valueType)
        {
            return _formatterGetter().Format(value, valueType);
        }
    }
}

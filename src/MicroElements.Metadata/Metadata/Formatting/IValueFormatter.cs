// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Formatting
{
    /// <summary>
    /// Represents value formatter.
    /// Formats value to string.
    /// </summary>
    public interface IValueFormatter
    {
        /// <summary>
        /// Determines whether this instance can format the specified object type.
        /// </summary>
        /// <param name="valueType">Value type.</param>
        /// <returns>
        /// <c>true</c> if this instance can format the specified object type; otherwise, <c>false</c>.
        /// </returns>
        bool CanFormat(Type valueType);

        /// <summary>
        /// Formats <paramref name="value"/> to string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="valueType">Value type.</param>
        /// <returns>Formatted string.</returns>
        string? Format(object? value, Type? valueType = null);
    }

    /// <summary>
    /// Represents strong typed value formatter.
    /// Implements <see cref="IValueFormatter"/> methods.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IValueFormatter<T> : IValueFormatter
    {
        /// <summary>
        /// Formats not null <paramref name="value"/> to string.
        /// If you want to process <c>null</c> value override <see cref="FormatNull"/>.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Formatted string.</returns>
        string? Format(T value);

        /// <summary>
        /// Formats null value to string. By default returns <c>null</c>.
        /// </summary>
        /// <returns>Formatted string in case of null value.</returns>
        string? FormatNull() => null;

        /// <inheritdoc />
        bool IValueFormatter.CanFormat(Type valueType)
        {
            return typeof(T).IsAssignableFrom(valueType);
        }

        /// <inheritdoc />
        string? IValueFormatter.Format(object? value, Type? valueType)
        {
            if (value is T typedValue)
                return Format(typedValue);

            return FormatNull();
        }
    }

    internal static class ValueFormatter
    {
        /// <summary>
        /// Gets target type.
        /// Treats null type and typeof(object) as type unknown (so it should be retrived from <paramref name="value"/>).
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static Type GetTargetType(object? value, Type? valueType)
        {
            // treats null type and typeof(object) as type unknown
            if (valueType == null || valueType == typeof(object))
                valueType = value?.GetType() ?? typeof(object);
            return valueType;
        }
    }
}

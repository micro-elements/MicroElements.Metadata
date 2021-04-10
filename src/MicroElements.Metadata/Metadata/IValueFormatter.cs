// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Reflection;

namespace MicroElements.Metadata
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
        string? Format(object? value, Type valueType);
    }

    /// <summary>
    /// Represents strong typed value formatter.
    /// Implements <see cref="IValueFormatter"/> methods.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IValueFormatter<T> : IValueFormatter
    {
        /// <summary>
        /// Formats <paramref name="value"/> to string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Formatted string.</returns>
        string? Format(T? value);

        /// <inheritdoc />
        bool IValueFormatter.CanFormat(Type valueType)
        {
            return typeof(T).IsAssignableFrom(valueType);
        }

        /// <inheritdoc />
        string? IValueFormatter.Format(object? value, Type valueType)
        {
            if (value is T valueTyped)
                return Format(valueTyped);

            if (value is null && ValueFormatterTypeCache<T>.CanAcceptNull)
                return Format(default);

            return null;
        }
    }

    internal static class ValueFormatterTypeCache<T>
    {
        internal static bool CanAcceptNull { get; } = typeof(T).CanAcceptNull();
    }
}

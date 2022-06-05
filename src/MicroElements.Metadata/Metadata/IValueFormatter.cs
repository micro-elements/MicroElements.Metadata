// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents a formatter that formats values to string.
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
        /// <returns>Formatted string or <see langword="null"/>.</returns>
        string? Format(object? value, Type valueType);
    }

    /// <summary>
    /// Represents a strong typed value formatter.
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
        bool IValueFormatter.CanFormat(Type valueType) => valueType.IsAssignableTo<T>();

        /// <inheritdoc />
        string? IValueFormatter.Format(object? value, Type valueType)
        {
            if (value is T valueTyped)
                return Format(valueTyped);

            // If type can accept null let formatter to format it too.
            if (value is null && ValueFormatterTypeCache<T>.CanAcceptNull)
                return Format(default);

            return null;
        }
    }

    /// <summary>
    /// The same as <see cref="IValueFormatter{T}"/> but that formats only not null values.
    /// For null values will return null string.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface INotNullValueFormatter<T> : IValueFormatter
    {
        /// <summary>
        /// Formats <paramref name="value"/> to string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Formatted string.</returns>
        string? Format(T value);

        /// <inheritdoc />
        bool IValueFormatter.CanFormat(Type valueType) => typeof(T).IsAssignableFrom(valueType);

        /// <inheritdoc />
        string? IValueFormatter.Format(object? value, Type valueType)
        {
            if (value is T valueTyped and not null)
                return Format(valueTyped);

            return null;
        }
    }

    internal static class ValueFormatterTypeCache<T>
    {
        internal static bool CanAcceptNull { get; } = typeof(T).CanAcceptNull();
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.Formatting;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Formatting extensions.
    /// </summary>
    public static class StringFormatter
    {
        /// <summary>
        /// Default string formatting for most used types.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="nullResultValue">Value to return if input value is null or formatted result is null.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatValue(this object? value, string nullResultValue = "null")
        {
            if (value == null)
                return nullResultValue;

            return Formatter.FullRecursiveFormatter.TryFormat(value) ?? nullResultValue;
        }

        /// <summary>
        /// Tries to format value with provided formatter.
        /// In case of error default formatting will be used.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="formatter">Formatter.</param>
        /// <param name="value">Value to format.</param>
        /// <param name="valueType">Optional value type.</param>
        /// <returns>Formatted string.</returns>
        public static string? TryFormat<T>(this IValueFormatter formatter, T? value, Type? valueType = null)
        {
            try
            {
                return formatter.Format(value, valueType ?? typeof(T));
            }
            catch
            {
                return DefaultToStringFormatter.Instance.Format(value, valueType ?? typeof(T));
            }
        }
    }
}

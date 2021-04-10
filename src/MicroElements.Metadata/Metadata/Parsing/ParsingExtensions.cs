// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// TryParse delegate.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Source value.</param>
    /// <param name="result">Output result.</param>
    /// <returns>True if parse was successful.</returns>
    public delegate bool TryParseFunc<T>(string? value, out T result);

    /// <summary>
    /// TryParse delegate with extended params for numeric parse.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Source value.</param>
    /// <param name="numberStyles">NumberStyles.</param>
    /// <param name="provider">IFormatProvider.</param>
    /// <param name="result">Output result.</param>
    /// <returns>True if parse was successful.</returns>
    public delegate bool TryParseNumericFunc<T>(string? value, NumberStyles numberStyles, IFormatProvider provider, out T result);

    /// <summary>
    /// Parsing extensions.
    /// </summary>
    public static partial class ParsingExtensions
    {
        /// <summary>
        /// Parses text value with provided <paramref name="tryParse"/> function.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Source text value.</param>
        /// <param name="tryParse">TryParse function.</param>
        /// <returns>Parse result.</returns>
        [Pure]
        public static ParseResult<T> Parse<T>(this string? value, TryParseFunc<T> tryParse) =>
            tryParse(value, out T result)
                ? ParseResult.Success(result)
                : ParseResult.Failed<T>();

        /// <summary>
        /// Parses text value with provided <paramref name="tryParse"/> function.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Source text value.</param>
        /// <param name="tryParse">TryParse function.</param>
        /// <returns>Parse result.</returns>
        [Pure]
        public static ParseResult<T?> ParseNullable<T>(this string? value, TryParseFunc<T> tryParse)
            where T : struct
        {
            if (value == null)
                return ParseResult.Default<T?>();

            return tryParse(value, out T result)
                ? ParseResult.Success<T?>(result)
                : ParseResult.Failed<T?>();
        }

        /// <summary>
        /// Parses text value with provided <paramref name="tryParse"/> function.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Source text value.</param>
        /// <param name="tryParse">TryParse function.</param>
        /// <param name="numberStyles">NumberStyles.</param>
        /// <param name="provider">IFormatProvider.</param>
        /// <returns>Parse result.</returns>
        [Pure]
        public static ParseResult<T> ParseNumeric<T>(this string? value, TryParseNumericFunc<T> tryParse, NumberStyles numberStyles, IFormatProvider provider) =>
            tryParse(value, numberStyles, provider, out T result)
                ? ParseResult.Success(result)
                : ParseResult.Failed<T>();

        /// <summary>
        /// Parses text value with provided <paramref name="tryParse"/> function.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Source text value.</param>
        /// <param name="tryParse">TryParse function.</param>
        /// <param name="numberStyles">NumberStyles.</param>
        /// <param name="provider">IFormatProvider.</param>
        /// <returns>Parse result.</returns>
        [Pure]
        public static ParseResult<T?> ParseNullableNumeric<T>(this string? value, TryParseNumericFunc<T> tryParse, NumberStyles numberStyles, IFormatProvider provider)
            where T : struct
        {
            if (value == null)
                return ParseResult.Default<T?>();

            return tryParse(value, numberStyles, provider, out T result)
                ? ParseResult.Success<T?>(result)
                : ParseResult.Failed<T?>();
        }
    }
}

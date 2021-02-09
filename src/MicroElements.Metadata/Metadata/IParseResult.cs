// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents parse result.
    /// </summary>
    public interface IParseResult
    {
        /// <summary>
        /// Gets value type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets a value indicating whether the result is in success state.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets optional value.
        /// </summary>
        object? ValueUntyped { get; }

        /// <summary>
        /// Gets optional error. Can be filled if result is failed.
        /// </summary>
        Message? Error { get; }
    }

    /// <summary>
    /// Represents parse result.
    /// </summary>
    /// <typeparam name="T">Result value type.</typeparam>
    public readonly struct ParseResult<T> : IParseResult
    {
        /// <summary>
        /// Gets Empty Success result for type.
        /// </summary>
        public static ParseResult<T> Empty { get; } = ParseResult.Success<T>(default);

        /// <summary>
        /// Gets default Failed result for type.
        /// </summary>
        public static ParseResult<T> Failed { get; } = ParseResult.Failed<T>();

        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public bool IsSuccess { get; }

        /// <inheritdoc />
        public object? ValueUntyped => Value;

        /// <inheritdoc />
        public Message? Error { get; }

        /// <summary>
        /// Gets result value.
        /// </summary>
        [AllowNull]
        [MaybeNull]
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseResult{T}"/> struct.
        /// </summary>
        /// <param name="isSuccess">Is success.</param>
        /// <param name="value">Parse result.</param>
        /// <param name="error">Error.</param>
        internal ParseResult(bool isSuccess, [AllowNull] T value, Message? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }
    }

    /// <summary>
    /// ParseResult extensions.
    /// </summary>
    public static class ParseResult
    {
        /// <summary>
        /// Creates <see cref="ParseResult{T}"/> is Success state.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="value">Value.</param>
        /// <returns>Success <see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> Success<T>([AllowNull] T value)
        {
            return new ParseResult<T>(isSuccess: true, value: value, error: null);
        }

        /// <summary>
        /// Creates <see cref="ParseResult{T}"/> is Failed state.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="error">Optional error message.</param>
        /// <returns>Failed <see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> Failed<T>(Message? error = null)
        {
            return new ParseResult<T>(isSuccess: false, value: default, error: error);
        }

        /// <summary>
        /// Converts <see cref="Option{A}"/> to <see cref="ParseResult{T}"/>.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="option">Source option.</param>
        /// <returns><see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> ToParseResult<T>(this in Option<T> option)
        {
            if (option.IsSome)
                return Success((T)option);

            return ParseResult<T>.Failed;
        }

        /// <summary>
        /// Maps result to other result type with <paramref name="map"/> func.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Target type.</typeparam>
        /// <param name="source">Source result.</param>
        /// <param name="map">Map function.</param>
        /// <returns><see cref="ParseResult{T}"/> of type <typeparamref name="B"/>.</returns>
        public static ParseResult<B> Map<A, B>(this in ParseResult<A> source, Func<A, B> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess)
            {
                B valueB = map(source.Value!);
                return Success(valueB);
            }

            return ParseResult<B>.Failed;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;
using MicroElements.Validation;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents parse result.
    /// 1. Result can be in two states: Success | Failed;
    /// 2. Result knows value type.
    /// 3. Result can hold null value as Success; (Option can not)
    /// 4. Failed result should have error message.
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
        public T? Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseResult{T}"/> struct.
        /// </summary>
        /// <param name="isSuccess">Is success.</param>
        /// <param name="value">Parse result.</param>
        /// <param name="error">Error.</param>
        internal ParseResult(bool isSuccess, T? value, Message? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Implicit conversion of value to success ParseResult.
        /// </summary>
        /// <param name="value">Value.</param>
        public static implicit operator ParseResult<T>(T? value) => ParseResult.Success(value);

        /// <summary>
        /// Implicit conversion of Message to failed ParseResult.
        /// </summary>
        /// <param name="message">Value.</param>
        public static implicit operator ParseResult<T>(Message message) => ParseResult.Failed<T>(message);

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsSuccess)
                return $"Success<{Type.GetFriendlyName()}>({Value.FormatValue()})";
            return $"Failed<{Type.GetFriendlyName()}>({Error?.FormattedMessage})";
        }
    }

    /// <summary>
    /// ParseResult extensions.
    /// </summary>
    public static class ParseResult
    {
        /// <summary>
        /// Gets Success result with default value for type.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Success result with default value for type.</returns>
        public static ParseResult<T> Default<T>() => ParseResult<T>.Empty;

        /// <summary>
        /// Creates <see cref="ParseResult{T}"/> is Success state.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="value">Value.</param>
        /// <returns>Success <see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> Success<T>(T? value)
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
        public static ParseResult<B> Map<A, B>(this in ParseResult<A> source, Func<A?, B?> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess)
            {
                B? valueB = map(source.Value!);
                return Success(valueB);
            }

            return Failed<B>(source.Error);
        }

        /// <summary>
        /// Binds result to other result type with <paramref name="bind"/> func.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Target type.</typeparam>
        /// <param name="source">Source result.</param>
        /// <param name="bind">Bind function.</param>
        /// <returns><see cref="ParseResult{T}"/> of type <typeparamref name="B"/>.</returns>
        public static ParseResult<B> Bind<A, B>(this in ParseResult<A> source, Func<A?, ParseResult<B>> bind)
        {
            bind.AssertArgumentNotNull(nameof(bind));

            if (source.IsSuccess)
            {
                ParseResult<B> resultB = bind(source.Value!);
                return resultB;
            }

            return Failed<B>(source.Error);
        }

        public static ParseResult<A> WrapError<A>(this in ParseResult<A> source, Func<Message?, Message?> mapError)
        {
            mapError.AssertArgumentNotNull(nameof(mapError));

            if (source.IsSuccess)
            {
                return source;
            }

            Message? newError = mapError(source.Error);
            return Failed<A>(newError);
        }
    }
}

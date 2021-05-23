// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using MicroElements.CodeContracts;
using MicroElements.Diagnostics;
using MicroElements.Diagnostics.ErrorModel;
using MicroElements.Reflection;

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
    /// Strong typed parse result.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IParseResult<T> : IParseResult
    {
        /// <summary>
        /// Gets result value.
        /// </summary>
        public T? Value { get; }
    }

    /// <summary>
    /// Represents parse result.
    /// ParseResult is a class because most use cases uses it as boxed <see cref="IParseResult"/>.
    /// </summary>
    /// <typeparam name="T">Result value type.</typeparam>
    [ImmutableObject(true)]
    public class ParseResult<T> : IParseResult<T>
    {
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
        /// Initializes a new instance of the <see cref="ParseResult{T}"/> class.
        /// </summary>
        /// <param name="isSuccess">Is success.</param>
        /// <param name="value">Parse result.</param>
        /// <param name="error">Error.</param>
        internal ParseResult(bool isSuccess, T? value, Message? error)
        {
            if (isSuccess && error != null)
                throw new ArgumentException("Success ParseResult should not have error.");

            if (!isSuccess && error == null)
                throw new ArgumentException("Failed ParseResult should have error.");

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
        /// Most used parse results.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        public static class Cache<T>
        {
            /// <summary>
            /// Gets Empty Success result for type.
            /// </summary>
            public static ParseResult<T> SuccessDefault { get; } = new ParseResult<T>(isSuccess: true, value: default, error: null);

            /// <summary>
            /// Gets default Failed result for type.
            /// </summary>
            public static ParseResult<T> Failed { get; } = new ParseResult<T>(isSuccess: false, value: default, error: new Message("Failed result."));

            /// <summary>
            /// Gets None result.
            /// </summary>
            public static ParseResult<T> None { get; } = new ParseResult<T>(isSuccess: false, value: default, error: new Message("None result."));

            /// <summary>
            /// Gets default Failed result for type.
            /// </summary>
            public static ParseResult<T> FailedNullNotAllowed { get; } = ParseResult.Failed<T>(new Message("Null value is not allowed.", severity: MessageSeverity.Error));
        }

        /// <summary>
        /// Gets default NullNotAllowed result for type.
        /// </summary>
        public static ParseResult<T> NullNotAllowed<T>() => Cache<T>.FailedNullNotAllowed;

        /// <summary>
        /// Gets Success result with default value for type.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <returns>Success result with default value for type.</returns>
        public static ParseResult<T> Default<T>() => Cache<T>.SuccessDefault;

        /// <summary>
        /// Creates <see cref="ParseResult{T}"/> is Success state.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="value">Value.</param>
        /// <returns>Success <see cref="ParseResult{T}"/> instance.</returns>
        public static ParseResult<T> Success<T>(T? value)
        {
            if (value.IsDefault())
                return Cache<T>.SuccessDefault;

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
            if (error == null)
                return Cache<T>.Failed;

            return new ParseResult<T>(isSuccess: false, value: default, error: error);
        }

        //TODO: Migrate
        ///// <summary>
        ///// Converts <see cref="Option{A}"/> to <see cref="ParseResult{T}"/>.
        ///// </summary>
        ///// <typeparam name="T">Result type.</typeparam>
        ///// <param name="option">Source option.</param>
        ///// <returns><see cref="ParseResult{T}"/> instance.</returns>
        //public static ParseResult<T> ToParseResult<T>(this in Option<T> option)
        //{
        //    if (option.IsSome)
        //        return Success((T)option);

        //    return ParseResult.Cache<T>.Failed;
        //}

        public static ParseResult<T> ToParseResult<T>(this T value, bool allowNull = false)
        {
            if (value.IsNull())
            {
                if (!allowNull)
                    return Cache<T>.FailedNullNotAllowed;
                return Cache<T>.SuccessDefault;
            }

            return ParseResult.Success(value);
        }

        public static ParseResult<T> ParseNotNull<T>(this T? value)
        {
            if (value.IsNull())
            {
                return Cache<T>.FailedNullNotAllowed;
            }

            return ParseResult.Success(value);
        }

        public static ParseResult<T> Parse<T>(this T value)
        {
            if (value.IsNull())
            {
                return Cache<T>.SuccessDefault;
            }

            return ParseResult.Success(value);
        }

        /// <summary>
        /// Maps result to other result type with <paramref name="map"/> func.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Target type.</typeparam>
        /// <param name="source">Source result.</param>
        /// <param name="map">Map function.</param>
        /// <returns><see cref="ParseResult{T}"/> of type <typeparamref name="B"/>.</returns>
        public static ParseResult<B> Map<A, B>(this IParseResult<A> source, Func<A, B> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess)
            {
                B valueB = map(source.Value!);
                return Success(valueB);
            }

            return ParseResult.Failed<B>(source.Error);
        }

        public static IParseResult<T> MapNotNull<T>(this IParseResult<T?> source)
        {
            if (source.IsSuccess && source.Value.IsNotNull())
            {
                return source;
            }

            return ParseResult.Cache<T>.FailedNullNotAllowed;
        }

        public static IParseResult<B> MapNotNull<A, B>(this IParseResult<A?> source, Func<A, B> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess && source.Value.IsNotNull())
            {
                B valueB = map(source.Value!);
                return Success(valueB);
            }

            return ParseResult.Failed<B>(source.Error);
        }

        public static IParseResult<B> Bind<A, B>(this IParseResult<A> source, Func<A, IParseResult<B>> bind)
        {
            bind.AssertArgumentNotNull(nameof(bind));

            if (source.IsSuccess)
            {
                return bind(source.Value!);
            }

            return ParseResult.Failed<B>(source.Error);
        }

        public static T? GetValueOrThrow<T>(this IParseResult<T> source, bool allowNullResult = true)
        {
            if (source.IsSuccess)
            {
                T? value = source.Value;
                if (!allowNullResult && value.IsNull())
                    throw new Exception();

                return value;
            }

            Message error = source.Error ?? new Message("ParseResult dos not contain detailed error", severity: MessageSeverity.Error);
            throw error.ToException();
        }

        public static object? GetValueOrThrow(this IParseResult source, bool allowNullResult = true)
        {
            if (source.IsSuccess)
            {
                var value = source.ValueUntyped;
                if (!allowNullResult && value.IsNull())
                    throw new Exception();

                return value;
            }

            Message error = source.Error ?? new Message("ParseResult dos not contain detailed error", severity: MessageSeverity.Error);
            throw error.ToException();
        }

        public static Exception ToException(this Message message)
        {
            Error<string> error = new Error<string>(message.EventName ?? "ERROR", message.FormattedMessage);
            return new ExceptionWithError<string>(error);
        }
    }
}

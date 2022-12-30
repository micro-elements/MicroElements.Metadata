// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Formatting;
using MicroElements.Reflection.FriendlyName;
using Message = MicroElements.Diagnostics.Message;
using MessageSeverity = MicroElements.Diagnostics.MessageSeverity;
using ObjectExtensions = MicroElements.Reflection.ObjectExtensions.ObjectExtensions;

namespace MicroElements.Metadata.Parsing
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
    /// ParseResult is a class because most use cases uses it as boxed <see cref="IParseResult"/> also it can be cached.
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
            if (ObjectExtensions.IsDefault(value))
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
            if (ObjectExtensions.IsNull(value))
            {
                if (!allowNull)
                    return Cache<T>.FailedNullNotAllowed;
                return Cache<T>.SuccessDefault;
            }

            return ParseResult.Success(value);
        }

        public static ParseResult<T> ParseNotNull<T>(this T? value)
        {
            if (ObjectExtensions.IsNull(value))
            {
                return Cache<T>.FailedNullNotAllowed;
            }

            return ParseResult.Success(value);
        }

        public static ParseResult<T> Parse<T>(this T value)
        {
            if (ObjectExtensions.IsNull(value))
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
        public static ParseResult<B> Map<A, B>(this IParseResult<A> source, Func<A?, B?> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess)
            {
                B? valueB = map(source.Value);
                return Success(valueB);
            }

            return ParseResult.Failed<B>(source.Error);
        }

        public static IParseResult<T> MapNotNull<T>(this IParseResult<T?> source)
        {
            if (source.IsSuccess && ObjectExtensions.IsNotNull(source.Value))
            {
                return source;
            }

            return ParseResult.Cache<T>.FailedNullNotAllowed;
        }

        public static IParseResult<B> MapNotNull<A, B>(this IParseResult<A?> source, Func<A, B> map)
        {
            map.AssertArgumentNotNull(nameof(map));

            if (source.IsSuccess && ObjectExtensions.IsNotNull(source.Value))
            {
                B valueB = map(source.Value!);
                return Success(valueB);
            }

            return ParseResult.Failed<B>(source.Error);
        }

        /// <summary>
        /// Binds result to other result type with <paramref name="bind"/> func.
        /// </summary>
        /// <typeparam name="A">Source type.</typeparam>
        /// <typeparam name="B">Target type.</typeparam>
        /// <param name="source">Source result.</param>
        /// <param name="bind">Bind function.</param>
        /// <returns><see cref="ParseResult{T}"/> of type <typeparamref name="B"/>.</returns>
        public static IParseResult<B> Bind<A, B>(this IParseResult<A> source, Func<A?, IParseResult<B>> bind)
        {
            bind.AssertArgumentNotNull(nameof(bind));

            if (source.IsSuccess)
            {
                IParseResult<B> resultB = bind(source.Value);
                return resultB;
            }

            return ParseResult.Failed<B>(source.Error);
        }

        [Pure]
        public static B Match<A, B>(
            this IParseResult<A> source,
            Func<A?, B> mapResult,
            Func<Message, B> mapError)
        {
            source.AssertArgumentNotNull(nameof(source));
            mapResult.AssertArgumentNotNull(nameof(mapResult));
            mapError.AssertArgumentNotNull(nameof(mapError));

            if (source.IsSuccess)
                return mapResult(source.Value);

            return mapError(source.Error!);
        }

        public static void Match<A>(
            this IParseResult<A> source,
            Action<A?> onResult,
            Action<Message> onError)
        {
            source.AssertArgumentNotNull(nameof(source));
            onResult.AssertArgumentNotNull(nameof(onResult));
            onError.AssertArgumentNotNull(nameof(onError));

            if (source.IsSuccess)
                onResult(source.Value);
            else
                onError(source.Error!);
        }

        public static T? GetValueOrDefault<T>(this IParseResult<T> source, T? defaultValue = default)
        {
            if (source.IsSuccess)
                return source.Value;
            return defaultValue;
        }

        public static T? GetValueOrDefault<T>(this IParseResult<T> source, Func<T?> getDefaultValue)
        {
            if (source.IsSuccess)
                return source.Value;
            return getDefaultValue();
        }

        public static T? GetValueOrThrow<T>(this IParseResult<T> source, bool allowNullResult = true)
        {
            if (source.IsSuccess)
            {
                T? value = source.Value;
                if (!allowNullResult && ObjectExtensions.IsNull(value))
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
                if (!allowNullResult && ObjectExtensions.IsNull(value))
                    throw new Exception();

                return value;
            }

            Message error = source.Error ?? new Message("ParseResult dos not contain detailed error", severity: MessageSeverity.Error);
            throw error.ToException();
        }

        public static Exception ToException(this Message message)
        {
            Diagnostics.ErrorModel.Error<string> error = new Diagnostics.ErrorModel.Error<string>(message.EventName ?? "ERROR", message.FormattedMessage);
            return new Diagnostics.ErrorModel.ExceptionWithError<string>(error);
        }

        public static ParseResult<A> WrapError<A>(this ParseResult<A> source, Func<Message?, Message?> mapError)
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

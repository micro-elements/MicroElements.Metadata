// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Diagnostics;
using MicroElements.Metadata.Formatting;
using MicroElements.Metadata.Parsing;
using MicroElements.Reflection.CodeCompiler;
using MicroElements.Reflection.FriendlyName;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions for <see cref="IPropertyParser"/>.
    /// </summary>
    public static class PropertyParserExtensions
    {
        /// <summary>
        /// Searches value to parse in <paramref name="sourceRow"/> then parses and returns optional parse result.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="sourceRow">Value to parse.</param>
        /// <returns>Parse result.</returns>
        public static ParseResult<IPropertyValue> ParseRowUntyped(
            this IPropertyParser propertyParser,
            IReadOnlyDictionary<string, string?> sourceRow)
        {
            bool isValuePresent = sourceRow.TryGetValue(propertyParser.SourceName, out string? valueToParse);
            return ParseValueUntyped(propertyParser, valueToParse, isValuePresent);
        }

        /// <summary>
        /// Parses text value according <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Property parser.</param>
        /// <param name="valueToParse">Value to parse. Can be null.</param>
        /// <param name="isValuePresent">valueToParse was provided whether it is not null or null.</param>
        /// <returns>Parse result.</returns>
        public static ParseResult<IPropertyValue> ParseValueUntyped(
            this IPropertyParser propertyParser,
            string? valueToParse,
            bool isValuePresent = true)
        {
            bool isValueAbsent = !isValuePresent;
            bool isValueNull = valueToParse is null;

            if ((isValueAbsent || isValueNull) && propertyParser.DefaultSourceValue is { Value: { } defaultSourceValue })
            {
                // Use provided value as source value.
                valueToParse = defaultSourceValue;

                isValueNull = valueToParse is null;
                isValuePresent = true;
                isValueAbsent = false;
            }

            if (isValueAbsent && propertyParser.DefaultValueUntyped != null)
            {
                return propertyParser.GetDefaultValueUntyped();
            }

            if (isValueNull && propertyParser.DefaultValueUntyped != null)
            {
                return propertyParser.GetDefaultValueUntyped();
            }

            if (isValuePresent)
            {
                ParseResult<IPropertyValue> parseResult = ParseUntyped(propertyParser, valueToParse);
                parseResult = parseResult.WrapError(message =>
                {
                    string errorCause = message != null ? $" ParseError: '{message.FormattedMessage}'" : string.Empty;
                    return new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed from source '{valueToParse.FormatValue()}'{errorCause}", MessageSeverity.Error);
                });
                return parseResult;
            }

            // Return failed result.
            return ParseResult.Failed<IPropertyValue>(new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed because source '{propertyParser.SourceName}' is absent.", MessageSeverity.Error));
        }

        /// <summary>
        /// Parses <paramref name="textValue"/> with <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="textValue">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static ParseResult<IPropertyValue> ParseUntyped(this IPropertyParser propertyParser, string? textValue)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, string?, ParseResult<IPropertyValue>>(propertyParser.TargetType, "Parse", Parse<object>);
            return func(propertyParser, textValue);

            typeof(PropertyParserExtensions)
                .GetCompiledCachedMethod<IPropertyParser, string?, ParseResult<IPropertyValue>>("Parse",
                    propertyParser.TargetType);

            static ParseResult<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string? textValue)
            {
                var propertyParserTyped = (IPropertyParser<T>)propertyParserUntyped;

                return propertyParserTyped
                    .ValueParser
                    .Parse(textValue)
                    .Map(value => (IPropertyValue)new PropertyValue<T>(propertyParserTyped.TargetProperty, value, ValueSource.Defined));
            }
        }

        /// <summary>
        /// Gets <see cref="IPropertyParser{T}.DefaultValue"/> for untyped <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <returns>Returns optional <see cref="IPropertyValue"/> if default value evaluated for <paramref name="propertyParser"/>.</returns>
        public static ParseResult<IPropertyValue> GetDefaultValueUntyped(this IPropertyParser propertyParser)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, ParseResult<IPropertyValue>>(propertyParser.TargetType, "GetDefaultValue", GetDefaultValue<CodeCompiler.GenericType>);
            return func(propertyParser);

            static ParseResult<IPropertyValue> GetDefaultValue<T>(IPropertyParser propertyParserUntyped)
            {
                IPropertyValue? propertyValue = null;
                IPropertyParser<T> propertyParser = (IPropertyParser<T>)propertyParserUntyped;
                if (propertyParser.DefaultValue != null)
                {
                    var defaultValue = propertyParser.DefaultValue.Value;
                    propertyValue = new PropertyValue<T>(propertyParser.TargetProperty, defaultValue, ValueSource.DefaultValue);
                    return ParseResult.Success(propertyValue);
                }

                propertyValue = PropertyValue.Default(propertyParser.TargetProperty);
                return ParseResult.Success(propertyValue);
            }
        }
    }

    [Obsolete("Use shared")]
    internal static class Invoker
    {
        private static class FuncCache<TMethodArg1, TResult>
        {
            internal static ConcurrentDictionary<(Type MethodOwner, string MethodName, Type? GenericArg1, Type? GenericArg2), Func<TMethodArg1, TResult>> Cache = new();
        }

        private static class FuncCache<TMethodArg1, TMethodArg2, TResult>
        {
            internal static ConcurrentDictionary<(Type MethodOwner, string MethodName, Type? GenericArg1, Type? GenericArg2), Func<TMethodArg1, TMethodArg2, TResult>> Cache = new();
        }

        private static class FuncCache<TMethodArg1, TMethodArg2, TMethodArg3, TResult>
        {
            internal static ConcurrentDictionary<(Type MethodOwner, string MethodName, Type? GenericArg1, Type? GenericArg2), Func<TMethodArg1, TMethodArg2, TMethodArg3, TResult>> Cache = new();
        }

        public static Func<TInstance, TMethodArg1, TResult> GetCompiledCachedMethod<TInstance, TMethodArg1, TResult>(
            string methodName, Type? genericArg1 = null, Type? genericArg2 = null)
        {
            var cacheKey = (typeof(TInstance), name: methodName, arg1: genericArg1, arg2: genericArg2);
            return FuncCache<TInstance, TMethodArg1, TResult>.Cache.GetOrAdd(cacheKey,
                a => CompileMethod<TInstance, TMethodArg1, TResult>(a.MethodOwner, a.MethodName, a.GenericArg1, a.GenericArg2));
        }

        public static Func<TInstance, TMethodArg1, TMethodArg2, TResult> GetCompiledCachedMethod<TInstance, TMethodArg1, TMethodArg2, TResult>(
            string methodName, Type? genericArg1 = null, Type? genericArg2 = null)
        {
            var cacheKey = (typeof(TInstance), name: methodName, arg1: genericArg1, arg2: genericArg2);
            return FuncCache<TInstance, TMethodArg1, TMethodArg2, TResult>.Cache.GetOrAdd(cacheKey,
                a => CompileMethod<TInstance, TMethodArg1, TMethodArg2, TResult>(a.MethodOwner, a.MethodName, a.GenericArg1, a.GenericArg2));
        }

        public static Func<TMethodArg1, TResult> GetCompiledCachedMethod<TMethodArg1, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1 = null, Type? genericArg2 = null)
        {
            var cacheKey = (methodOwnerType: methodOwner, name: methodName, arg1: genericArg1, arg2: genericArg2);
            return FuncCache<TMethodArg1, TResult>.Cache.GetOrAdd(cacheKey,
                a => CompileMethod<TMethodArg1, TResult>(a.MethodOwner, a.MethodName, a.GenericArg1, a.GenericArg2));
        }

        public static Func<TMethodArg1, TMethodArg2, TResult> GetCompiledCachedMethod<TMethodArg1, TMethodArg2, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1 = null, Type? genericArg2 = null)
        {
            var cacheKey = (methodOwnerType: methodOwner, name: methodName, arg1: genericArg1, arg2: genericArg2);
            return FuncCache<TMethodArg1, TMethodArg2, TResult>.Cache.GetOrAdd(cacheKey,
                a => CompileMethod<TMethodArg1, TMethodArg2, TResult>(a.MethodOwner, a.MethodName, a.GenericArg1, a.GenericArg2));
        }

        public static Func<TMethodArg1, TMethodArg2, TMethodArg3, TResult> GetCompiledCachedMethod<TMethodArg1, TMethodArg2, TMethodArg3, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1 = null, Type? genericArg2 = null)
        {
            var cacheKey = (methodOwnerType: methodOwner, name: methodName, arg1: genericArg1, arg2: genericArg2);
            return FuncCache<TMethodArg1, TMethodArg2, TMethodArg3, TResult>.Cache.GetOrAdd(cacheKey,
                a => CompileMethod<TMethodArg1, TMethodArg2, TMethodArg3, TResult>(a.MethodOwner, a.MethodName, a.GenericArg1, a.GenericArg2));
        }

        private static MethodInfo GetMethod(Type methodOwner, string methodName, Type? genericArg1, Type? genericArg2)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var methodInfo = methodOwner.GetMethod(methodName, bindingFlags);
            if (methodInfo is null)
                throw new InvalidOperationException($"Type {methodOwner.GetFriendlyName()} has no method {methodName}");

            if (methodInfo.IsGenericMethodDefinition)
            {
                Type[] methodGenericArguments = GetArgs(genericArg1, genericArg2).ToArray();
                methodInfo = methodInfo.MakeGenericMethod(methodGenericArguments);
            }

            return methodInfo;

            static IEnumerable<Type> GetArgs(Type? genericArg1, Type? genericArg2)
            {
                if (genericArg1 != null) yield return genericArg1;
                if (genericArg2 != null) yield return genericArg2;
            }
        }

        public static Func<TMethodArg1, TResult> CompileMethod<TMethodArg1, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1, Type? genericArg2)
        {
            var genericMethod = GetMethod(methodOwner, methodName, genericArg1, genericArg2);
            return CompileMethod<TMethodArg1, TResult>(genericMethod);
        }

        public static Func<TMethodArg1, TMethodArg2, TResult> CompileMethod<TMethodArg1, TMethodArg2, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1, Type? genericArg2)
        {
            var genericMethod = GetMethod(methodOwner, methodName, genericArg1, genericArg2);
            return CompileMethod<TMethodArg1, TMethodArg2, TResult>(genericMethod);
        }

        public static Func<TMethodArg1, TMethodArg2, TMethodArg3, TResult> CompileMethod<TMethodArg1, TMethodArg2, TMethodArg3, TResult>(
            this Type methodOwner, string methodName, Type? genericArg1, Type? genericArg2)
        {
            var genericMethod = GetMethod(methodOwner, methodName, genericArg1, genericArg2);
            return CompileMethod<TMethodArg1, TMethodArg2, TMethodArg3, TResult>(genericMethod);
        }

        public static Func<TMethodArg1, TResult> CompileMethod<TMethodArg1, TResult>(MethodInfo method)
        {
            var arg1 = Expression.Parameter(typeof(TMethodArg1), "arg1");

            MethodCallExpression callExpression = method.IsStatic ?
                Expression.Call(null, method, arg1) :
                Expression.Call(arg1, method);

            return Expression
                .Lambda<Func<TMethodArg1, TResult>>(callExpression, arg1)
                .Compile();
        }

        public static Func<TMethodArg1, TMethodArg2, TResult> CompileMethod<TMethodArg1, TMethodArg2, TResult>(MethodInfo method)
        {
            var arg1 = Expression.Parameter(typeof(TMethodArg1), "arg1");
            var arg2 = Expression.Parameter(typeof(TMethodArg2), "arg2");

            MethodCallExpression callExpression = method.IsStatic ?
                Expression.Call(null, method, arg1, arg2) :
                Expression.Call(arg1, method, arg2);

            return Expression
                .Lambda<Func<TMethodArg1, TMethodArg2, TResult>>(callExpression, arg1, arg2)
                .Compile();
        }

        public static Func<TMethodArg1, TMethodArg2, TMethodArg3, TResult> CompileMethod<TMethodArg1, TMethodArg2, TMethodArg3, TResult>(MethodInfo method)
        {
            var arg1 = Expression.Parameter(typeof(TMethodArg1), "arg1");
            var arg2 = Expression.Parameter(typeof(TMethodArg2), "arg2");
            var arg3 = Expression.Parameter(typeof(TMethodArg3), "arg3");

            MethodCallExpression callExpression = method.IsStatic ?
                Expression.Call(null, method, arg1, arg2, arg3) :
                Expression.Call(arg1, method, arg2, arg3);

            return Expression
                .Lambda<Func<TMethodArg1, TMethodArg2, TMethodArg3, TResult>>(callExpression, arg1, arg2, arg3)
                .Compile();
        }
    }
}



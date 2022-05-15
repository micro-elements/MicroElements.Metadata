// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Factory methods for <see cref="IDefaultValue"/>.
    /// </summary>
    public static partial class DefaultValue
    {
        private static readonly ConcurrentDictionary<Type, IDefaultValue> DefaultValueForTypeCache = new ();

        /// <summary>
        /// Gets <see cref="DefaultValue{T}.Default"/> instance for specified type.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns><see cref="DefaultValue{T}.Default"/> instance for specified type.</returns>
        public static IDefaultValue GetDefaultForType(Type type)
        {
            return DefaultValueForTypeCache.GetOrAdd(type, GetDefaultForTypeInternal);
        }

        /// <summary>
        /// Gets or creates <see cref="IDefaultValue"/>.
        /// Creates <see cref="DefaultValue{T}"/> if <paramref name="value"/> is not default.
        /// Returns <see cref="DefaultValue{T}.Default"/> instance if value is default.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="value">Value that will be used to create <see cref="IDefaultValue"/>.</param>
        /// <returns><see cref="IDefaultValue"/>.</returns>
        /// <exception cref="ArgumentException">Value can not be used as default value for type.</exception>
        public static IDefaultValue GetOrCreateDefaultValue(Type type, object? value)
        {
            value = value.ThrowIfValueCanNotBeAssignedToType(type);

            Func<object?, IDefaultValue> cachedCompiledFunc = CodeCompiler.CachedCompiledFunc<object?, IDefaultValue>(type, "CreateDefaultValue", CreateDefaultValueInternal<CodeCompiler.GenericType>);
            return cachedCompiledFunc(value);

            static IDefaultValue CreateDefaultValueInternal<T>(object? value) => ((T?)value).IsDefault() ? DefaultValue<T>.Default : new DefaultValue<T>((T?)value);
        }

        private static IDefaultValue GetDefaultForTypeInternal(Type type)
        {
            Type genericType = typeof(DefaultValue<>).MakeGenericType(type);
            PropertyInfo? propertyDefault = genericType.GetProperty(nameof(DefaultValue<object>.Default), BindingFlags.Public | BindingFlags.Static);
            return (IDefaultValue)propertyDefault!.GetValue(null);
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Core;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides default value for type, schema.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IDefaultValue : IMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the default value is allowed for the property value.
        /// </summary>
        bool IsDefaultValueAllowed { get; }

        /// <summary>
        /// Gets default value for type, schema.
        /// </summary>
        /// <returns>Default value.</returns>
        object? GetDefaultValue();
    }

    /// <summary>
    /// Provides strong typed default value for type, schema.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IDefaultValue<T> : IDefaultValue
    {
        /// <summary>
        /// Gets strong typed default value.
        /// </summary>
        T? Value { get; }

        /// <inheritdoc />
        object? IDefaultValue.GetDefaultValue() => Value;
    }

    /// <summary>
    /// Strong typed default value metadata.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public sealed class DefaultValue<T> : IDefaultValue<T>, IImmutable
    {
        /// <summary>
        /// Gets default value for type.
        /// </summary>
        public static IDefaultValue<T> Default { get; } = new DefaultValue<T>(defaultValue: default, isDefaultValueAllowed: true);

        /// <summary>
        /// Gets <see cref="IDefaultValue"/> for type that prohibits default value.
        /// </summary>
        public static IDefaultValue<T> DefaultNotAllowed { get; } = new DefaultValue<T>(defaultValue: default, isDefaultValueAllowed: false);

        /// <inheritdoc />
        public T? Value { get; }

        /// <inheritdoc />
        public bool IsDefaultValueAllowed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValue{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="isDefaultValueAllowed">Is default value is allowed for the property value.</param>
        public DefaultValue(T? defaultValue = default, bool isDefaultValueAllowed = true)
        {
            Value = defaultValue;
            IsDefaultValueAllowed = isDefaultValueAllowed;
        }
    }

    /// <summary>
    /// Default value metadata.
    /// </summary>
    public class DefaultValueUntyped : IDefaultValue, IImmutable
    {
        private readonly object? _defaultValue;

        /// <inheritdoc />
        public bool IsDefaultValueAllowed { get; }

        /// <inheritdoc />
        public object? GetDefaultValue() => _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueUntyped"/> class.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="isDefaultValueAllowed">Is default value is allowed for the property value.</param>
        public DefaultValueUntyped(object? defaultValue, bool isDefaultValueAllowed = true)
        {
            _defaultValue = defaultValue;
            IsDefaultValueAllowed = isDefaultValueAllowed;
        }
    }

    /// <summary>
    /// Strong typed default value that evaluates in runtime with provided function.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class DefaultValueLazy<T> : IDefaultValue<T>
    {
        private readonly Func<T> _defaultValue;

        /// <inheritdoc />
        public T? Value => _defaultValue();

        /// <inheritdoc />
        public bool IsDefaultValueAllowed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueLazy{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="isDefaultValueAllowed">Is default value is allowed for the property value.</param>
        public DefaultValueLazy(Func<T> defaultValue, bool isDefaultValueAllowed = true)
        {
            _defaultValue = defaultValue;
            IsDefaultValueAllowed = isDefaultValueAllowed;
        }
    }

    /// <summary>
    /// DefaultValue statics.
    /// </summary>
    public static class DefaultValue
    {
        /// <summary>
        /// Gets <see cref="IDefaultValue"/> that treats as "Default value is not allowed".
        /// </summary>
        public static IDefaultValue NotAllowed { get; } = new DefaultValueUntyped(null, isDefaultValueAllowed: false);

        /// <summary>
        /// Gets <see cref="IDefaultValue"/> for type.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="isDefaultValueAllowed">IsDefaultValueAllowed or not.</param>
        /// <returns><seealso cref="IDefaultValue"/> instance.</returns>
        public static IDefaultValue<T> ForType<T>(bool isDefaultValueAllowed = true) => isDefaultValueAllowed ? DefaultValue<T>.Default : DefaultValue<T>.DefaultNotAllowed;

        /// <summary>
        /// Gets <see cref="IDefaultValue"/> for type.
        /// </summary>
        /// <param name="type">Type to get default value.</param>
        /// <param name="isDefaultValueAllowed">IsDefaultValueAllowed or not.</param>
        /// <returns><seealso cref="IDefaultValue"/> instance.</returns>
        public static IDefaultValue ForType(Type type, bool isDefaultValueAllowed = true) => new DefaultValueUntyped(GetDefaultValue(type), isDefaultValueAllowed);

        /// <summary>
        /// Gets default value for type.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>Default value.</returns>
        public static object? GetDefaultValue(Type type)
        {
            type.AssertArgumentNotNull(nameof(type));

            if (type.IsValueType)
                return Activator.CreateInstance(type);

            // For reference types always returns null.
            return null;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IDefaultValue"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="defaultValue">DefaultValue metadata for schema.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetDefaultValueMetadata<TSchema>(this TSchema schema, IDefaultValue defaultValue)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            defaultValue.AssertArgumentNotNull(nameof(defaultValue));

            return schema.SetMetadata(defaultValue);
        }

        /// <summary>
        /// Gets optional <see cref="IDefaultValue"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IDefaultValue"/> metadata.</returns>
        [Pure]
        public static IDefaultValue? GetDefaultValue(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetSchemaMetadata<IDefaultValue>();
        }

        /// <summary>
        /// Sets <see cref="IDefaultValue"/> metadata for the schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="allowDefaultValue">Value indicating that property can contain null value.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetAllowDefault<TSchema>(this TSchema schema, bool allowDefaultValue = true)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));

            IDefaultValue defaultValue = DefaultValue.ForType(schema.Type, allowDefaultValue);
            return schema.SetDefaultValueMetadata(defaultValue);
        }

        /// <summary>
        /// Sets <see cref="IDefaultValue"/> metadata for the schema.
        /// Checks that <paramref name="defaultValue"/> can be set as default value fo schema type.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="defaultValue">Default value for the schema, property.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetDefaultValueUntyped<TSchema>(this TSchema schema, object? defaultValue)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));

            if (defaultValue != null)
            {
                bool isAssignableType = defaultValue.IsAssignableTo(schema.Type);
                if (!isAssignableType)
                    throw new ArgumentException($"Value {defaultValue.FormatValue()} can not be set as default value for type {schema.Type}");

                return schema.SetDefaultValueMetadata(new DefaultValueUntyped(defaultValue));
            }

            if (!schema.Type.CanAcceptNull())
                throw new ArgumentException($"null value can not be set as default value for type {schema.Type}");

            return schema.SetDefaultValueMetadata(DefaultValue.ForType(schema.Type));
        }

        /// <summary>
        /// Sets <see cref="IDefaultValue"/> metadata for the schema.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="defaultValue">Default value for the schema, property.</param>
        /// <returns>The same schema.</returns>
        public static ISchema<T> SetDefaultValue<T>(this ISchema<T> schema, T? defaultValue)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.SetDefaultValueMetadata(new DefaultValue<T>(defaultValue));
        }
    }
}

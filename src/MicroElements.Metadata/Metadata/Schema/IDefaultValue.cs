// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Exceptions;
using MicroElements.Reflection.TypeExtensions;
using MicroElements.Validation;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Provides default value for type, schema.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property | MetadataTargets.SimpleSchema)]
    public interface IDefaultValue : IMetadata
    {
        /// <summary>
        /// Gets default value for type, schema.
        /// </summary>
        /// <returns>Default value.</returns>
        object? Value { get; }
    }

    /// <summary>
    /// Provides strong typed default value for type, schema.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    [MetadataUsage(ValidOn = MetadataTargets.Property | MetadataTargets.SimpleSchema)]
    public interface IDefaultValue<out T> : IDefaultValue
    {
        /// <inheritdoc />
        object? IDefaultValue.Value => Value;

        /// <summary>
        /// Gets strong typed default value.
        /// </summary>
        new T? Value { get; }
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
        public static IDefaultValue<T> Default { get; } = new DefaultValue<T>(defaultValue: default);

        /// <inheritdoc />
        public T? Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValue{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        public DefaultValue(T? defaultValue = default)
        {
            Value = defaultValue;
        }

        /// <inheritdoc />
        public override string ToString() => $"DefaultValue: {Value.FormatValue("null")}, Type: {typeof(T).GetFriendlyName()}";
    }

    /// <summary>
    /// Strong typed default value that evaluates in runtime with provided function.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class DefaultValueLazy<T> : IDefaultValue<T>
    {
        private readonly Func<T?> _defaultValue;

        /// <inheritdoc />
        public T? Value => _defaultValue();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueLazy{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">Default value.</param>
        public DefaultValueLazy(Func<T?> defaultValue)
        {
            _defaultValue = defaultValue;
        }
    }

    /// <summary>
    /// DefaultValue statics.
    /// </summary>
    public static partial class DefaultValue
    {
        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if <paramref name="value"/> can not be used as default value for <paramref name="type"/>.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="type">Type to check.</param>
        /// <returns>The same value if it can be used as default value for type.</returns>
        /// <exception cref="ArgumentException">Value can not be used as default value for type.</exception>
        public static object? ThrowIfValueCanNotBeAssignedToType(this object? value, Type type)
        {
            type.AssertArgumentNotNull(nameof(type));

            if (value != null)
            {
                bool isAssignableType = value.IsAssignableTo(type);
                if (!isAssignableType)
                    throw new ArgumentException($"Value '{value.FormatValue()}' can not be set as default value for type '{type}'");

                return value;
            }

            if (!type.CanAcceptNull())
                throw new ArgumentException($"Value 'null' can not be set as default value for type '{type}'");

            return value;
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
        public static IDefaultValue? GetDefaultValueMetadata(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetComponent<IDefaultValue>();
        }

        public static object? GetDefaultValueUntyped(this ISchema schema, object? defaultValue = null)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            IDefaultValue? defaultValueMetadata = GetDefaultValueMetadata(schema);
            return defaultValueMetadata != null ? defaultValueMetadata.Value : defaultValue;
        }

        public static T? GetDefaultValue<T>(this ISchema<T> schema, T? defaultValue = default)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            IDefaultValue? defaultValueMetadata = GetDefaultValueMetadata(schema);
            return defaultValueMetadata != null ? (T?)defaultValueMetadata.Value : defaultValue;
        }

        public static T? GetDefaultValueOrThrow<T>(this ISchema<T> schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            IDefaultValue defaultValueMetadata = GetDefaultValueMetadata(schema) ?? throw new NotFoundException($"Metadata of type '{typeof(T).GetFriendlyName()}' was not found in '{schema}'");
            return (T?)defaultValueMetadata.Value;
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

            IDefaultValue defaultValueUntyped = DefaultValue.GetOrCreateDefaultValue(schema.Type, defaultValue);

            return schema.SetDefaultValueMetadata(defaultValueUntyped);
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

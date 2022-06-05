// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema builder extensions.
    /// </summary>
    public static partial class SchemaBuilder
    {
        /// <summary>
        /// Creates a new copy of the source with new default value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithDefaultValue<TSchema, T>(this TSchema schema, IDefaultValue<T> defaultValue)
            where TSchema : ISchemaBuilder<IDefaultValue<T>>, ISchema<T>
        {
            return schema.WithComponent(defaultValue);
        }

        /// <summary>
        /// Creates a new copy of the source with new default value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithDefaultValueUntyped<TSchema>(this TSchema schema, IDefaultValue defaultValue)
            where TSchema : ISchemaBuilder<IDefaultValue>, ISchema
        {
            return schema.WithComponent(defaultValue);
        }

        /// <summary>
        /// Creates a new copy of the source with new default value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithDefaultValue<TSchema, T>(this TSchema schema, T? defaultValue)
            where TSchema : ISchemaBuilder<IDefaultValue<T>>, ISchema<T>
        {
            return schema.WithDefaultValue(new DefaultValue<T>(defaultValue));
        }

        /// <summary>
        /// Creates a new copy of the source with new default value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="schema">The source schema.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>A new copy of the source schema.</returns>
        [Pure]
        public static TSchema WithDefaultValue<TSchema, T>(this TSchema schema, Func<T?> defaultValue)
            where TSchema : ISchemaBuilder<IDefaultValue<T>>, ISchema<T>
        {
            return schema.WithDefaultValue(new DefaultValueLazy<T>(defaultValue));
        }
    }
}

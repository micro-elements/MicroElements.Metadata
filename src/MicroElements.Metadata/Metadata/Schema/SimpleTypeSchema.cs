// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents simple type schema.
    /// Simple type does not have own properties but has some restrictions for vase type.
    /// Example: Currency (string type with maxLength: 3).
    /// </summary>
    [DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
    public class SimpleTypeSchema : ISchema, IManualMetadataProvider, ISchemaBuilder<SimpleTypeSchema, ISchemaDescription>
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <inheritdoc />
        public IPropertyContainer Metadata { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypeSchema"/> class.
        /// </summary>
        /// <param name="name">Type name.</param>
        /// <param name="type">Base type for schema.</param>
        /// <param name="description">Optional schema description.</param>
        /// <param name="metadata">Optional metadata for schema.</param>
        public SimpleTypeSchema(string name, Type type, string? description = null, IPropertyContainer? metadata = null)
        {
            name.AssertArgumentNotNull(nameof(name));
            type.AssertArgumentNotNull(nameof(type));

            Name = name;
            Type = type;
            Description = description;

            Metadata = new ConcurrentMutablePropertyContainer(metadata, searchOptions: MetadataProvider.DefaultSearchOptions);
        }

        /// <inheritdoc />
        public SimpleTypeSchema With(ISchemaDescription schemaDescription)
        {
            var source = this;
            return new SimpleTypeSchema(source.Name, source.Type, schemaDescription.Description, source.Metadata);
        }
    }

    /// <summary>
    /// Represents simple type schema.
    /// Simple type does not have own properties but has some restrictions for vase type.
    /// Example: Currency (string type with maxLength: 3).
    /// </summary>
    /// <typeparam name="T">Schema value type.</typeparam>
    [DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
    public partial class SimpleTypeSchema<T> : ISchema<T>
    {
        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypeSchema{T}"/> class.
        /// </summary>
        /// <param name="name">Type name.</param>
        /// <param name="description">Optional schema description.</param>
        public SimpleTypeSchema(string name, string? description = null)
        {
            name.AssertArgumentNotNull(nameof(name));

            Name = name;
            Description = description;
        }
    }

    /// <content>
    /// SimpleTypeSchema builder methods.
    /// </content>
    public partial class SimpleTypeSchema<T> : ISchemaBuilder<SimpleTypeSchema<T>, ISchemaDescription>
    {
        /// <inheritdoc />
        public SimpleTypeSchema<T> With(ISchemaDescription schemaDescription)
        {
            var source = this;
            return new SimpleTypeSchema<T>(source.Name, schemaDescription.Description).SetMetadataFrom(source);
        }
    }
}

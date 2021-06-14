// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using MicroElements.CodeContracts;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Allows to attach schema to <see cref="IProperty"/> or <see cref="IPropertyContainer"/>.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.PropertyContainer | MetadataTargets.Property)]
    public interface IHasSchema : IMetadata
    {
        /// <summary>
        /// Gets schema for <see cref="IProperty"/> or <see cref="IPropertyContainer"/>.
        /// </summary>
        ISchema Schema { get; }

        /// <summary>
        /// Creates new instance of <see cref="ISchema"/>.
        /// </summary>
        /// <returns>New instance of <see cref="ISchema"/>.</returns>
        ISchema Create();
    }

    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    public class HasSchema : IHasSchema
    {
        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create() => Schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchema"/> class.
        /// </summary>
        /// <param name="schema">Schema for <see cref="IPropertyContainer"/>.</param>
        public HasSchema(ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            Schema = schema;
        }
    }

    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    public class HasSchemaProvider : IHasSchema
    {
        private readonly ISchemaProvider _schemaProvider;

        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create() => _schemaProvider.GetSchema();

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchemaProvider"/> class.
        /// </summary>
        /// <param name="schemaProvider">Schema for <see cref="IPropertyContainer"/>.</param>
        public HasSchemaProvider(ISchemaProvider schemaProvider)
        {
            schemaProvider.AssertArgumentNotNull(nameof(schemaProvider));

            _schemaProvider = schemaProvider;

            Schema = Create();
        }
    }

    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    public class HasSchema<TSchema> : IHasSchema
        where TSchema : ISchema, new()
    {
        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create() => new TSchema();

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchema{T}"/> class.
        /// </summary>
        public HasSchema()
        {
            Schema = Create();
        }
    }

    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    public class HasSchemaByType : IHasSchema
    {
        /// <summary>
        /// Gets schema type.
        /// </summary>
        public Type SchemaType { get; }

        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create()
        {
            ISchema schema = (ISchema)Activator.CreateInstance(SchemaType);
            return schema;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchemaByType"/> class.
        /// </summary>
        /// <param name="schemaType">Schema type.</param>
        public HasSchemaByType(Type schemaType)
        {
            schemaType.AssertArgumentNotNull(nameof(schemaType));

            if (!schemaType.IsAssignableTo(typeof(ISchema)))
                throw new ArgumentException($"Type {schemaType} should be {nameof(ISchema)}");

            SchemaType = schemaType;
            Schema = Create();
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        private static readonly ConditionalWeakTable<ISchema, IHasSchema> _schemasCache = new ();

        private static IMetadataProvider SetSchemaInternal(this IMetadataProvider metadataProvider, ISchema schema)
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));
            schema.AssertArgumentNotNull(nameof(schema));

            // Caches IHasSchema by each schema instance.
            IHasSchema hasSchema = _schemasCache.GetValue(schema, sch => new HasSchema(sch));
            return metadataProvider.SetMetadata(hasSchema);
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <param name="schema"><see cref="IHasSchema"/> metadata.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, IHasSchema schema)
            where TProperty : IProperty
        {
            property.AssertArgumentNotNull(nameof(property));
            schema.AssertArgumentNotNull(nameof(schema));

            return property.SetMetadata(schema);
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <param name="schemaProvider">Schema provider.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, ISchemaProvider schemaProvider)
            where TProperty : IProperty
        {
            property.AssertArgumentNotNull(nameof(property));
            schemaProvider.AssertArgumentNotNull(nameof(schemaProvider));

            return property.SetSchema(new HasSchemaProvider(schemaProvider));
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TSchemaProvider">Schema provider.</typeparam>
        /// <param name="property">Target property.</param>
        /// <returns>The same instance.</returns>
        public static IProperty<IPropertyContainer> SetSchema<TSchemaProvider>(this IProperty<IPropertyContainer> property)
            where TSchemaProvider : ISchemaProvider, new()
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetSchema(new TSchemaProvider());
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <param name="schema">Schema.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, ISchema schema)
            where TProperty : IProperty
        {
            property.AssertArgumentNotNull(nameof(property));
            schema.AssertArgumentNotNull(nameof(schema));

            return (TProperty)property.SetSchemaInternal(schema);
        }

        /// <summary>
        /// Sets schema for <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Target property container.</param>
        /// <param name="schema">Schema.</param>
        /// <returns>The same instance.</returns>
        public static IPropertyContainer SetSchema(this IPropertyContainer propertyContainer, ISchema schema)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            schema.AssertArgumentNotNull(nameof(schema));

            return (IPropertyContainer)propertyContainer.SetSchemaInternal(schema);
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <param name="schemaType">Schema type.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, Type schemaType)
            where TProperty : ISchema
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetMetadata((IHasSchema)new HasSchemaByType(schemaType));
        }

        /// <summary>
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="metadataProvider">Source property container.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetHasSchema(this IPropertyContainer metadataProvider)
        {
            metadataProvider.AssertArgumentNotNull(nameof(metadataProvider));

            return metadataProvider.GetMetadata<IHasSchema>(searchInSchema: false);
        }

        /// <summary>
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetHasSchema(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IHasSchema>();
        }

        /// <summary>
        /// Gets optional <see cref="ISchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="ISchema"/>.</returns>
        [Pure]
        public static ISchema? GetSchema(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetHasSchema()?.Schema;
        }

        /// <summary>
        /// Creates new schema instance for property if it has schema (has <see cref="IHasSchema"/> metadata).
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="ISchema"/>.</returns>
        public static ISchema? GetNewSchema(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetHasSchema()?.Create();
        }
    }
}

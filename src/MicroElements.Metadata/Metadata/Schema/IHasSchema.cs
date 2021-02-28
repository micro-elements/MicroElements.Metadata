// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.PropertyContainer | MetadataTargets.Property)]
    public interface IHasSchema : IMetadata
    {
        /// <summary>
        /// Gets schema for <see cref="IPropertyContainer"/>.
        /// </summary>
        ISchema Schema { get; }

        /// <summary>
        /// Creates new instance of <see cref="IMutableObjectSchema"/>.
        /// </summary>
        /// <returns>New instance of <see cref="IMutableObjectSchema"/>.</returns>
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
    public class HasSchema2 : IHasSchema
    {
        private readonly ISchemaProvider _schemaProvider;

        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create() => Schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchema"/> class.
        /// </summary>
        /// <param name="schemaProvider">Schema for <see cref="IPropertyContainer"/>.</param>
        public HasSchema2(ISchemaProvider schemaProvider)
        {
            schemaProvider.AssertArgumentNotNull(nameof(schemaProvider));

            _schemaProvider = schemaProvider;
            Schema = _schemaProvider.GetSchema();
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
        public HasSchema() => Schema = Create();
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

            if (!schemaType.IsAssignableTo<ISchema>())
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

            return metadataProvider.SetMetadata(_schemasCache.GetValue(schema, sch => new HasSchema(sch)));
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
        /// Sets schema for <paramref name="propertyContainer"/>.
        /// </summary>
        /// <param name="propertyContainer">Target property container.</param>
        /// <param name="schema">Schema.</param>
        /// <returns>The same instance.</returns>
        public static IPropertyContainer SetSchema(this IPropertyContainer propertyContainer, IPropertySet schema)
        {
            return propertyContainer.SetSchema(schema.ToSchema());
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
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <returns>The same instance.</returns>
        public static IProperty<IPropertyContainer> SetSchema<TSchema>(this IProperty<IPropertyContainer> property)
            where TSchema : ISchema, new()
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetMetadata((IHasSchema)new HasSchema<TSchema>());
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <param name="schemaType">Schema type.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, Type schemaType)
            where TProperty : IProperty
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetMetadata((IHasSchema)new HasSchemaByType(schemaType));
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
        /// <param name="schema"><see cref="IHasSchema"/> metadata.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, ISchemaProvider schema)
            where TProperty : IProperty
        {
            property.AssertArgumentNotNull(nameof(property));
            schema.AssertArgumentNotNull(nameof(schema));

            return property.SetMetadata<TProperty, IHasSchema>(new HasSchema(schema.GetSchema()));
        }

        /// <summary>
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetHasSchema(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return propertyContainer.GetMetadata<IHasSchema>();
        }

        /// <summary>
        /// Gets optional <see cref="IMutableObjectSchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Optional <see cref="IMutableObjectSchema"/>.</returns>
        [Pure]
        public static IObjectSchema? GetSchema(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            ISchema? schema = propertyContainer.GetHasSchema()?.Schema;

            return schema?.ToObjectSchema();
        }

        /// <summary>
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetHasSchema(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IHasSchema>();
        }

        /// <summary>
        /// Gets optional <see cref="IMutableObjectSchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IMutableObjectSchema"/>.</returns>
        [Pure]
        public static ISchema? GetSchema(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetHasSchema()?.Schema;
        }

        /// <summary>
        /// Gets or adds schema to property.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <param name="factory">Schema factory.</param>
        /// <returns>Schema attached to property.</returns>
        public static ISchema GetOrAddSchema(this IProperty property, Func<ISchema>? factory = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            IHasSchema? hasSchema = property.GetMetadata<IHasSchema>();

            ISchema? schema = hasSchema?.Create();
            if (schema == null)
            {
                schema = factory?.Invoke() ?? new MutableObjectSchema(name: property.Name);
                property.SetSchema(schema);
            }

            return schema;
        }
    }
}

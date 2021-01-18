﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
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
    /// <typeparam name="TSchema">Schema type.</typeparam>
    public class HasSchema<TSchema> : IHasSchema
        where TSchema : IPropertySet, new()
    {
        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public ISchema Create() => new TSchema().ToSchema();

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
            IPropertySet? propertySet = Activator.CreateInstance(SchemaType) as IPropertySet;
            return propertySet!.ToSchema();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasSchemaByType"/> class.
        /// </summary>
        /// <param name="schemaType">Schema type.</param>
        public HasSchemaByType(Type schemaType)
        {
            schemaType.AssertArgumentNotNull(nameof(schemaType));

            if (!schemaType.IsAssignableTo<IPropertySet>())
                throw new ArgumentException($"Type {schemaType} should be {nameof(IPropertySet)} or {nameof(ISchema)}");

            SchemaType = schemaType;
            Schema = Create();
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
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

            return propertyContainer.SetMetadata((IHasSchema)new HasSchema(schema));
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

            return property.SetMetadata((IHasSchema)new HasSchema(schema));
        }

        /// <summary>
        /// Sets schema for <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="property">Target property.</param>
        /// <returns>The same instance.</returns>
        public static IProperty<IPropertyContainer> SetSchema<TSchema>(this IProperty<IPropertyContainer> property)
            where TSchema : IPropertySet, new()
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
        /// <param name="schema">Schema.</param>
        /// <returns>The same instance.</returns>
        public static TProperty SetSchema<TProperty>(this TProperty property, IPropertySet schema)
            where TProperty : IProperty
        {
            return property.SetSchema(schema.ToSchema());
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
        /// Gets optional <see cref="ISchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Optional <see cref="ISchema"/>.</returns>
        [Pure]
        public static ISchema? GetSchema(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return propertyContainer.GetHasSchema()?.Schema;
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
        /// Gets optional <see cref="ISchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="ISchema"/>.</returns>
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
                schema = factory?.Invoke() ?? new MutableSchema();
                property.SetSchema(schema);
            }

            return schema;
        }
    }
}

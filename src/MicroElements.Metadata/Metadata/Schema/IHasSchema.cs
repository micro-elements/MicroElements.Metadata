// Copyright (c) MicroElements. All rights reserved.
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
    }

    /// <summary>
    /// Allows to attach schema to <see cref="IPropertyContainer"/>.
    /// </summary>
    public class HasSchema : IHasSchema
    {
        /// <inheritdoc />
        public ISchema Schema { get; }

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
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetSchema(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            return propertyContainer.GetMetadata<IHasSchema>();
        }

        /// <summary>
        /// Gets optional <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IHasSchema"/>.</returns>
        [Pure]
        public static IHasSchema? GetSchema(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IHasSchema>();
        }

        public static ISchema GetOrAddSchema(this IProperty property, Func<ISchema>? factory = null)
        {
            property.AssertArgumentNotNull(nameof(property));

            IHasSchema? hasSchema = property.GetMetadata<IHasSchema>();

            ISchema? schema = hasSchema?.Schema;
            if (schema == null)
            {
                schema = factory?.Invoke() ?? new Schema();
                property.SetSchema(schema);
            }

            return schema;
        }
    }
}

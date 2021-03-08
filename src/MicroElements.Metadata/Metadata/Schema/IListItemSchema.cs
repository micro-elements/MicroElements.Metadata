// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using MicroElements.Core;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// List schema metadata to provide schema to validate list item.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IListItemSchema : IMetadata
    {
        /// <summary>
        /// Gets schema to validate list item.
        /// </summary>
        ISchema ItemSchema { get; }
    }

    /// <inheritdoc cref="IListItemSchema"/>
    public class ListItemSchema : IListItemSchema
    {
        /// <inheritdoc />
        public ISchema ItemSchema { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListItemSchema"/> class.
        /// </summary>
        /// <param name="itemSchema">Schema to validate list item.</param>
        public ListItemSchema(ISchema itemSchema)
        {
            itemSchema.AssertArgumentNotNull(nameof(itemSchema));

            ItemSchema = itemSchema;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IListItemSchema"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="itemSchema">Schema to validate list item.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetLisItemsSchema<TSchema>(this TSchema schema, ISchema itemSchema)
            where TSchema : ISchema
        {
            return SetLisItemsSchema(schema, new ListItemSchema(itemSchema));
        }

        /// <summary>
        /// Sets <see cref="IListItemSchema"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="itemSchema">Schema to validate list item.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetLisItemsSchema<TSchema>(this TSchema schema, IListItemSchema itemSchema)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            itemSchema.AssertArgumentNotNull(nameof(itemSchema));

            return schema.SetMetadata(itemSchema);
        }

        /// <summary>
        /// Gets optional <see cref="IListItemSchema"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IListItemSchema"/> metadata.</returns>
        [Pure]
        public static IListItemSchema? GetLisItemsSchema(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IListItemSchema>();
        }
    }
}

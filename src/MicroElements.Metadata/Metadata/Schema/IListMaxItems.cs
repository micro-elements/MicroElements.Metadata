// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.Core;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional list property metadata.
    /// An array instance is valid against "maxItems" if its size is less than, or equal to, the value of this keyword.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IListMaxItems : IMetadata
    {
        /// <summary>
        /// Gets maximum items allowed for list.
        /// </summary>
        int MaxItems { get; }
    }

    /// <inheritdoc cref="IListMaxItems"/>
    public class ListMaxItems : IListMaxItems, IImmutable
    {
        /// <inheritdoc />
        public int MaxItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMaxItems"/> class.
        /// </summary>
        /// <param name="maxItems">Minimum items allowed for list property.</param>
        public ListMaxItems(int maxItems)
        {
            if (maxItems < 0)
                throw new ArgumentException($"MaxItems should be a non-negative integer but was {maxItems}", nameof(maxItems));

            MaxItems = maxItems;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IListMaxItems"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minItems">Maximum items allowed for list.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetListMaxItems<TSchema>(this TSchema schema, int minItems)
            where TSchema : ISchema
        {
            return SetListMaxItems(schema, new ListMaxItems(minItems));
        }

        /// <summary>
        /// Sets <see cref="IListMaxItems"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minItems">Maximum items allowed for list.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetListMaxItems<TSchema>(this TSchema schema, IListMaxItems minItems)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            minItems.AssertArgumentNotNull(nameof(minItems));

            return schema.SetMetadata(minItems);
        }

        /// <summary>
        /// Gets optional <see cref="IListMaxItems"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IListMaxItems"/> metadata.</returns>
        [Pure]
        public static IListMaxItems? GetListMaxItems(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IListMaxItems>();
        }
    }
}

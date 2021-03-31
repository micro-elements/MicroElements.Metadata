// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Core;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional list property metadata.
    /// An array instance is valid against "minItems" if its size is greater than, or equal to, the value of this keyword.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IListMinItems : IMetadata
    {
        /// <summary>
        /// Gets minimum items allowed for list.
        /// </summary>
        int MinItems { get; }
    }

    /// <inheritdoc cref="IListMinItems"/>
    public class ListMinItems : IListMinItems, IImmutable
    {
        /// <inheritdoc />
        public int MinItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMinItems"/> class.
        /// </summary>
        /// <param name="minItems">Minimum items allowed for list.</param>
        public ListMinItems(int minItems)
        {
            if (minItems < 0)
                throw new ArgumentException($"MinItems should be a non-negative integer but was {minItems}", nameof(minItems));

            MinItems = minItems;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IListMinItems"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minItems">Minimum items allowed for list.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetListMinItems<TSchema>(this TSchema schema, int minItems)
            where TSchema : ISchema
        {
            return SetListMinItems(schema, new ListMinItems(minItems));
        }

        /// <summary>
        /// Sets <see cref="IListMinItems"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minItems">Minimum items allowed for list.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetListMinItems<TSchema>(this TSchema schema, IListMinItems minItems)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            minItems.AssertArgumentNotNull(nameof(minItems));

            return schema.SetMetadata(minItems);
        }

        /// <summary>
        /// Gets optional <see cref="IListMinItems"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IListMinItems"/> metadata.</returns>
        [Pure]
        public static IListMinItems? GetListMinItems(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IListMinItems>();
        }
    }
}

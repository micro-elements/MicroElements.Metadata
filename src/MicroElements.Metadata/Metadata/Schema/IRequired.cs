// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Object is valid if required property is exists.
    /// It's an equivalent of JsonSchema required.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IRequired : IMetadata
    {
    }

    /// <summary>
    /// Property required metadata.
    /// </summary>
    public class Required : IRequired
    {
        /// <summary>
        /// Gets the global instance of <see cref="Required"/> metadata.
        /// </summary>
        public static IRequired Instance { get; } = new Required();
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IRequired"/> metadata for property.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="isRequired">Is property required or not.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetRequired<TSchema>(this TSchema schema, bool isRequired = true)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));

            if (isRequired)
                schema.SetMetadata(Required.Instance);
            else
                schema.SetMetadata<TSchema, IRequired>(null, ValueSource.NotDefined);

            return schema;
        }

        /// <summary>
        /// Gets optional <see cref="IRequired"/> metadata.
        /// </summary>
        /// <param name="schema">Source property.</param>
        /// <returns>Optional <see cref="IRequired"/>.</returns>
        public static IRequired? GetRequired(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IRequired>();
        }
    }
}

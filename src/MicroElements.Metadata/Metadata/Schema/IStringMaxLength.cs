// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents maximum allowed length for string values.
    /// It's an equivalent of JsonSchema maxLength.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringMaxLength : IMetadata
    {
        /// <summary>
        /// Gets maximum allowed length for string values.
        /// </summary>
        int MaxLength { get; }
    }

    /// <inheritdoc/>
    public sealed class StringMaxLength : IStringMaxLength
    {
        /// <inheritdoc/>
        public int MaxLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMaxLength"/> class.
        /// </summary>
        /// <param name="maxLength">Maximum allowed length for string values.</param>
        /// <exception cref="ArgumentException">MaxLength should be a non-negative integer.</exception>
        public StringMaxLength(int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentException($"MaxLength should be a non-negative integer but was {maxLength}", nameof(maxLength));
            MaxLength = maxLength;
        }

        /// <inheritdoc />
        public override string ToString() => $"StringMaxLength({MaxLength})";
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IStringMaxLength"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="maxLength">Maximum allowed length for string value.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringMaxLength<TSchema>(this TSchema schema, int maxLength)
            where TSchema : ISchema
        {
            return SetStringMaxLength(schema, new StringMaxLength(maxLength));
        }

        /// <summary>
        /// Sets <see cref="IStringMaxLength"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="maxLength">Maximum allowed length for string value.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringMaxLength<TSchema>(this TSchema schema, IStringMaxLength maxLength)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            maxLength.AssertArgumentNotNull(nameof(maxLength));

            return schema.SetMetadata(maxLength);
        }

        /// <summary>
        /// Gets optional <see cref="IStringMaxLength"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IStringMaxLength"/> metadata.</returns>
        [Pure]
        public static IStringMaxLength? GetStringMaxLength(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IStringMaxLength>();
        }
    }
}

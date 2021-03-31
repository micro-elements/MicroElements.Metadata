// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents minimum allowed length for string values.
    /// It's an equivalent of JsonSchema minLength.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringMinLength : IMetadata
    {
        /// <summary>
        /// Gets minimum allowed length for string values.
        /// </summary>
        int MinLength { get; }
    }

    /// <inheritdoc />
    public class StringMinLength : IStringMinLength
    {
        /// <inheritdoc />
        public int MinLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMinLength"/> class.
        /// </summary>
        /// <param name="minLength">Minimum allowed length for string values.</param>
        /// <exception cref="ArgumentException">MinLength should be a non-negative integer.</exception>
        public StringMinLength(int minLength)
        {
            if (minLength < 0)
                throw new ArgumentException($"MinLength should be a non-negative integer but was {minLength}", nameof(minLength));
            MinLength = minLength;
        }

        /// <inheritdoc />
        public override string ToString() => $"StringMinLength({MinLength})";
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IStringMinLength"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minLength">Minimum allowed length for string value.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringMinLength<TSchema>(this TSchema schema, int minLength)
            where TSchema : ISchema
        {
            return SetStringMinLength(schema, new StringMinLength(minLength));
        }

        /// <summary>
        /// Sets <see cref="IStringMinLength"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="minLength">Minimum allowed length for string value.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringMinLength<TSchema>(this TSchema schema, IStringMinLength minLength)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            minLength.AssertArgumentNotNull(nameof(minLength));

            return schema.SetMetadata(minLength);
        }

        /// <summary>
        /// Gets optional <see cref="IStringMinLength"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IStringMinLength"/> metadata.</returns>
        [Pure]
        public static IStringMinLength? GetStringMinLength(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<IStringMinLength>();
        }
    }
}

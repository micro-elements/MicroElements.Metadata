// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using MicroElements.Core;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="ISchema"/> metadata.
    /// Represents format for string values.
    /// It's an equivalent of JsonSchema format.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringFormat : IMetadata
    {
        /// <summary>
        /// Gets regex pattern for string values.
        /// </summary>
        string Format { get; }
    }

    /// <inheritdoc cref="IStringFormat"/>
    public class StringFormat : IStringFormat
    {
        /// <inheritdoc />
        public string Format { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFormat"/> class.
        /// </summary>
        /// <param name="format">Format.</param>
        public StringFormat(string format)
        {
            format.AssertArgumentNotNull(nameof(format));

            Format = format;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IStringFormat"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="format">Format metadata.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringFormat<TSchema>(this TSchema schema, IStringFormat format)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            format.AssertArgumentNotNull(nameof(format));

            return schema.SetMetadata(format);
        }

        /// <summary>
        /// Gets optional <see cref="IStringFormat"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IStringFormat"/> metadata.</returns>
        [Pure]
        public static IStringFormat? GetStringFormat(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetSchemaMetadata<IStringFormat>();
        }

        /// <summary>
        /// Sets <see cref="IStringFormat"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="format">Format metadata.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringFormat<TSchema>(this TSchema schema, string format)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            format.AssertArgumentNotNull(nameof(format));

            return schema.SetStringFormat(new StringFormat(format));
        }
    }
}

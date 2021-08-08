// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Core;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents regex pattern for string values.
    /// It's an equivalent of JsonSchema pattern.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringPattern : ISchemaComponent
    {
        /// <summary>
        /// Gets regex pattern for string values.
        /// </summary>
        string Expression { get; }
    }

    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents regex pattern for string values.
    /// It's an equivalent of JsonSchema pattern.
    /// </summary>
    public class StringPattern : IStringPattern, IImmutable
    {
        /// <inheritdoc />
        public string Expression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringPattern"/> class.
        /// </summary>
        /// <param name="expression">RegEx expression.</param>
        public StringPattern(string expression)
        {
            expression.AssertArgumentNotNull(nameof(expression));

            Expression = expression;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IStringPattern"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="pattern">Pattern metadata.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringPattern<TSchema>(this TSchema schema, IStringPattern pattern)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            pattern.AssertArgumentNotNull(nameof(pattern));

            return schema.SetMetadata(pattern);
        }

        /// <summary>
        /// Gets optional <see cref="IStringPattern"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="IStringPattern"/> metadata.</returns>
        [Pure]
        public static IStringPattern? GetStringPattern(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetSchemaMetadata<IStringPattern>();
        }

        /// <summary>
        /// Sets <see cref="IStringPattern"/> metadata to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="pattern">Pattern metadata.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetStringPattern<TSchema>(this TSchema schema, string pattern)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            pattern.AssertArgumentNotNull(nameof(pattern));

            return schema.SetStringPattern(new StringPattern(pattern));
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema combination.
    /// JsonSchema: https://json-schema.org/understanding-json-schema/reference/combining.html.
    /// JsonSchema: https://www.w3.org/2019/wot/json-schema.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.AnySchema)]
    public interface ISchemaCombinator : IMetadata
    {
        /// <summary>
        /// Gets combinator type.
        /// </summary>
        CombinatorType CombinatorType { get; }

        /// <summary>
        /// Gets schemas to combine.
        /// </summary>
        IReadOnlyCollection<ISchema> Schemas { get; }
    }

    /// <summary>
    /// Combinator type.
    /// </summary>
    public enum CombinatorType
    {
        /// <summary>
        /// Used to ensure that the data is valid against all of the specified schemas in the array.
        /// </summary>
        AllOf = 1,

        /// <summary>
        /// Used to ensure that the data is valid against any of the specified schemas in the array.
        /// </summary>
        AnyOf = 2,

        /// <summary>
        /// Used to ensure that the data is valid against exactly one of the specified schemas in the array.
        /// </summary>
        OneOf = 3,

        /// <summary>
        /// An instance is valid against this keyword if it fails to validate successfully against the schema defined by this keyword.
        /// </summary>
        Not = 4,

        /*

        Unsupported = 0,

        And = AllOf,
        Or = AnyOf,
        Xor = OneOf,

        Xsd_All = AllOf,
        Xsd_Choice = OneOf,

        // Json does not support property order
        Xsd_Sequence = AllOf,

        */
    }

    /// <inheritdoc cref="ISchemaCombinator"/>
    public class SchemaCombinator : ISchemaCombinator
    {
        /// <inheritdoc />
        public CombinatorType CombinatorType { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<ISchema> Schemas { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaCombinator"/> class.
        /// </summary>
        /// <param name="combinatorType">Combinator type.</param>
        /// <param name="schemas">Schemas to combine.</param>
        public SchemaCombinator(CombinatorType combinatorType, IReadOnlyCollection<ISchema> schemas)
        {
            CombinatorType = combinatorType;
            Schemas = schemas;
        }
    }

    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets schema combinator to schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="schemaCombinator">Schema combinator.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetSchemaCombinator<TSchema>(this TSchema schema, ISchemaCombinator schemaCombinator)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schemaCombinator.AssertArgumentNotNull(nameof(schemaCombinator));

            string combinatorName = schemaCombinator.CombinatorType.ToString();

            // Only one combinator of one type.
            return schema.SetMetadata(combinatorName, schemaCombinator);
        }

        /// <summary>
        /// Gets optional <see cref="ISchemaCombinator"/> metadata.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <param name="combinatorType">Combinator type to retrieve.</param>
        /// <returns>Optional <see cref="ISchemaCombinator"/> metadata.</returns>
        [Pure]
        public static ISchemaCombinator? GetSchemaCombinator(this ISchema schema, CombinatorType combinatorType)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            string combinatorName = combinatorType.ToString();

            return schema.GetMetadata<ISchemaCombinator>(combinatorName);
        }

        /// <summary>
        /// Sets AllOf combinator for schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="schemas">Schemas to combine.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetAllOf<TSchema>(this TSchema schema, params ISchema[] schemas)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schemas.AssertArgumentNotNull(nameof(schemas));

            return schema.SetSchemaCombinator(new SchemaCombinator(CombinatorType.AllOf, schemas));
        }

        /// <summary>
        /// Sets OneOf combinator for schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="schemas">Schemas to combine.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetOneOf<TSchema>(this TSchema schema, params ISchema[] schemas)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schemas.AssertArgumentNotNull(nameof(schemas));

            return schema.SetSchemaCombinator(new SchemaCombinator(CombinatorType.OneOf, schemas));
        }

        /// <summary>
        /// Sets OneOf combinator for schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="schemas">Schemas to combine.</param>
        /// <returns>The same schema.</returns>
        public static TSchema SetAnyOf<TSchema>(this TSchema schema, params ISchema[] schemas)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));
            schemas.AssertArgumentNotNull(nameof(schemas));

            return schema.SetSchemaCombinator(new SchemaCombinator(CombinatorType.AnyOf, schemas));
        }
    }
}

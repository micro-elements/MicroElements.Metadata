// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema properties for complex schemas.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.ObjectSchema | MetadataTargets.RootSchema)]
    public interface ISchemaProperties : IMetadata
    {
        /// <summary>
        /// Gets schema properties.
        /// </summary>
        IReadOnlyCollection<IProperty> Properties { get; }
    }

    /// <summary>
    /// Represents schema properties for complex schemas.
    /// </summary>
    public sealed class SchemaProperties : ISchemaProperties, IImmutable
    {
        /// <inheritdoc />
        public IReadOnlyCollection<IProperty> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaProperties"/> class.
        /// </summary>
        /// <param name="properties">Properties to add.</param>
        public SchemaProperties(IReadOnlyCollection<IProperty> properties)
        {
            properties.AssertArgumentNotNull(nameof(properties));

            Properties = properties;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="ISchemaProperties"/> metadata to schema.
        /// </summary>
        /// <param name="schema">Target schema.</param>
        /// <param name="schemaProperties">Schema properties to add to the schema..</param>
        /// <returns>The same schema.</returns>
        public static ISchema SetProperties(this ISchema schema, ISchemaProperties schemaProperties)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.SetMetadata(schemaProperties);
        }

        /// <summary>
        /// Gets <see cref="ISchemaProperties"/> for <see cref="ISchema"/>.
        /// </summary>
        /// <param name="schema">Source schema.</param>
        /// <returns>Optional <see cref="ISchemaProperties"/> metadata.</returns>
        public static ISchemaProperties? GetSchemaProperties(this ISchema schema)
        {
            schema.AssertArgumentNotNull(nameof(schema));

            return schema.GetMetadata<ISchemaProperties>();
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        public static IEnumerable<IMetadata> GetSchemaMetadata(this ISchema schema)
        {
            var metadataProperties = schema.GetMetadataContainer(autoCreate: false).Properties;

            foreach (var metadataProperty in metadataProperties)
            {
                if (metadataProperty.ValueUntyped is IMetadata metadata)
                {
                    if (metadata is IHasSchema hasSchema)
                    {
                        var referencedMetadataObjects = GetSchemaMetadata(hasSchema.Schema);
                        foreach (IMetadata referencedMetadataObject in referencedMetadataObjects)
                        {
                            yield return referencedMetadataObject;
                        }
                    }
                    else
                    {
                        yield return metadata;
                    }
                }
            }
        }

        /// <summary>
        /// Gets optional <see cref="IObjectSchema"/> from <see cref="IHasSchema"/> metadata.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Optional <see cref="IMutableObjectSchema"/>.</returns>
        public static IObjectSchema? GetSchema(this IPropertyContainer propertyContainer, bool autoCreateByProperties = false)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            if (propertyContainer.GetHasSchema() is { Schema: { } schema })
            {
                return schema.ToObjectSchema();
            }

            if (propertyContainer is IKnownPropertySet knownPropertySet)
            {
                if (knownPropertySet.SchemaType.IsAssignableTo<IObjectSchemaProvider>() && knownPropertySet.SchemaType.IsConcreteType())
                {
                    IObjectSchemaProvider schemaProvider = (IObjectSchemaProvider)Activator.CreateInstance(knownPropertySet.SchemaType);
                    return schemaProvider.GetObjectSchema();
                }

                IPropertySet? propertySet = PropertySetEvaluator.GetPropertySet(knownPropertySet.SchemaType);
                return propertySet?.ToSchema();
            }

            if (autoCreateByProperties)
            {
                var properties = propertyContainer.Properties.Select(value => value.PropertyUntyped);
                var objectSchema = new MutableObjectSchema(properties: properties);
                return objectSchema;
            }

            return null;
        }

        public static IObjectSchema GetOrCreateSchema(this IPropertyContainer propertyContainer)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            var schema = propertyContainer.GetSchema(autoCreateByProperties: false);

            if (schema == null)
            {
                var properties = propertyContainer.Properties.Select(value => value.PropertyUntyped);
                schema = new MutableObjectSchema(properties: properties, name: $"{GenerateRandomCode(8)}");
            }

            return schema;
        }

        private static string Symbols = "abcdefghijklmnopqrstuvwxyz";

        public static string GenerateRandomCode(int length = 8)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return Enumerable
                .Range(0, length)
                .Select(i => random.Next(0, Symbols.Length - 1))
                .Aggregate(new StringBuilder(), (stringBuilder, digit) => stringBuilder.Append(Symbols[digit].ToString()))
                .ToString();
        }
    }
}

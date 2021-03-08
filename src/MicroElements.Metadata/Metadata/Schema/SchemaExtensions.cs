// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

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
    }
}

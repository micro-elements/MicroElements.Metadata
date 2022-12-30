// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroElements.Text.Base58;
using MicroElements.Text.Hashing;

namespace MicroElements.Metadata.Schema
{
    public class SchemaDigest
    {
        public static string GetSchemaDigest(IEnumerable<IProperty> properties)
        {
            string digest = properties
                .OrderBy(property => property.Name)
                .Aggregate(
                    new StringBuilder(),
                    (builder, property) => builder.AppendFormat("{0}@{1};", property.Name, property.Type))
                .ToString();
            return digest;
        }
    }

    public interface ISchemaDigest
    {
        string GetSchemaDigest(IEnumerable<IProperty> properties);
    }

    public static class SchemaDigestExtensions
    {
        public static string GetSchemaDigest(this IEnumerable<IProperty> properties)
        {
            return SchemaDigest.GetSchemaDigest(properties);
        }

        public static string GetSchemaDigest(this IObjectSchema objectSchema)
        {
            return objectSchema.Properties.GetSchemaDigest();
        }

        public static string GetSchemaDigestHash(this IObjectSchema objectSchema)
        {
            return objectSchema
                .GetSchemaDigest()
                .Md5HashBytes()
                .AsHexText();
        }

        public static string GetSchemaDigestHashInBase58(this IObjectSchema objectSchema)
        {
            return objectSchema
                .GetSchemaDigest()
                .Md5HashBytes()
                .EncodeBase58();
        }
    }
}

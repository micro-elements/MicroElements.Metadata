// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Schema
{
    public class DataSchema :
        ISchema,
        IManualMetadataProvider,
        ISchemaBuilder<DataSchema, ISchemaDescription>,
        ISchemaBuilder<DataSchema, IOneOf>,
        ISchemaBuilder<DataSchema, IProperties>
    {
        /// <inheritdoc />
        public IPropertyContainer Metadata => this.GetOrCreateInstanceMetadata(() => MetadataProvider.CreateMutableContainer());

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        public DataSchema(string name)
        {
            Name = name;
            Type = typeof(object);
        }

        public DataSchema With(ISchemaDescription schemaDescription)
        {
            this.AsMetadataProvider().SetMetadata(schemaDescription);
            return this;
        }

        /// <inheritdoc />
        public DataSchema With(IOneOf oneOf)
        {
            this.AsMetadataProvider().SetMetadata(oneOf);
            return this;
        }

        /// <inheritdoc />
        public DataSchema With(IProperties schemaProperties)
        {
            this.AsMetadataProvider().SetMetadata(schemaProperties);
            return this;
        }
    }

    public static class aaa
    {
        public static DataSchema Test()
        {
            return new DataSchema("Schema")
                .WithDescription("Description")
                .OneOf(new DataSchema(""));
        }
    }
}

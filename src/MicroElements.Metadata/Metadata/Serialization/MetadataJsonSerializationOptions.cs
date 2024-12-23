﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Metadata json serializer options.
    /// Common for NewtonsoftJson and SystemTextJson.
    /// </summary>
    public class MetadataJsonSerializationOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the serialization should fail on serialization error.
        /// Default: true.
        /// </summary>
        public bool DoNotFail { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether arrays should be written in one row.
        /// Default: true.
        /// </summary>
        public bool WriteArraysInOneRow { get; set; } = true;

        /// <summary>
        /// Writes Json Schema (experimental).
        /// </summary>
        public bool UseJsonSchema { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the serializer will write compact schema definition in the start of the serialized metadata properties.
        /// TagName: "$metadata.schema.compact".
        /// Default: true.
        /// </summary>
        public bool WriteSchemaCompact { get; set; } = true;

        //public Func<string, string>? GetJsonPropertyName { get; set; }

        /// <summary>
        /// Gets or sets schema generator.
        /// </summary>
        public IJsonSchemaGenerator? SchemaGenerator { get; set; }

        /// <summary>
        /// WriteSchemaOnceForKnownTypes.
        /// </summary>
        public bool WriteSchemaOnceForKnownTypes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether schema info will be injected in a property name.
        /// It's the most compact presentation with schema but can lead problems with json processing by standard tools.
        /// </summary>
        public bool WriteSchemaToPropertyName { get; set; } = false;

        /// <summary>
        /// Gets or sets symbol that separates property name and property information.
        /// </summary>
        public string Separator { get; set; } = "@";

        /// <summary>
        /// Gets or sets an alternative separator in addition to <see cref="Separator"/>.
        /// </summary>
        public string? AltSeparator { get; set; } = null;

        /// <summary>
        /// Gets or sets a TypeMapper to map types in schema.
        /// </summary>
        public ITypeMapper TypeMapper { get; set; } = DefaultTypeMapper.Instance;

        /// <summary>
        /// Gets or sets a value indicating whether schema reference will be added to property container.
        /// For properties not from schema <see cref="IPropertyParseInfo.IsNotFromSchema"/> will be set to true.
        /// </summary>
        public bool AddSchemaInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether reader should read schema first and that read data.
        /// This impacts performance - use when schema is not the first node in json.
        /// </summary>
        public bool ReadSchemaFirst { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether serialization should use common schema catalog in json.
        /// This impacts performance but reduces json size.
        /// </summary>
        public bool UseSchemasRoot { get; set; } = false;

        /// <summary>
        /// Gets or sets schemas root name in json.
        /// </summary>
        public string SchemasRootName { get; set; } = "$defs";
    }

    public enum MetadataSchemaType
    {
        Compact,
        JsonSchema
    }

    /// <summary>
    /// <see cref="MetadataJsonSerializationOptions"/> extensions.
    /// </summary>
    public static class MetadataJsonSerializationOptionsExtensions
    {
        private static readonly MetadataJsonSerializationOptions _default = new MetadataJsonSerializationOptions();

        /// <summary>
        /// Creates new <see cref="MetadataJsonSerializationOptions"/> and optionally fills broken default values
        /// because there is no setter validation (for simplicity).
        /// </summary>
        /// <param name="source">Source options.</param>
        /// <returns>New <see cref="MetadataJsonSerializationOptions"/> instance.</returns>
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Ok.")]
        public static MetadataJsonSerializationOptions Copy(this MetadataJsonSerializationOptions? source)
        {
            source ??= new MetadataJsonSerializationOptions();
            return new MetadataJsonSerializationOptions()
            {
                DoNotFail = source.DoNotFail,
                WriteArraysInOneRow = source.WriteArraysInOneRow,
                WriteSchemaCompact = source.WriteSchemaCompact,
                SchemaGenerator = source.SchemaGenerator,
                WriteSchemaToPropertyName = source.WriteSchemaToPropertyName,
                Separator = source.Separator ?? _default.Separator,
                AltSeparator = source.AltSeparator ?? _default.AltSeparator,
                TypeMapper = source.TypeMapper,
                AddSchemaInfo = source.AddSchemaInfo,
                ReadSchemaFirst = source.ReadSchemaFirst,
                SchemasRootName = source.SchemasRootName,
                UseSchemasRoot = source.UseSchemasRoot,
                WriteSchemaOnceForKnownTypes = source.WriteSchemaOnceForKnownTypes,
                UseJsonSchema = source.UseJsonSchema,
            };
        }
    }
}

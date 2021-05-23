// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

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
        /// Gets or sets a value indicating whether the serializer will write compact schema definition in the start of the serialized metadata properties.
        /// TagName: "$metadata.schema.compact".
        /// Default: true.
        /// </summary>
        public bool WriteSchemaCompact { get; set; } = true;

        /// <summary>
        /// Gets or sets symbol that separates property name and property information.
        /// Default: "@".
        /// </summary>
        public string Separator { get; set; } = "@";

        /// <summary>
        /// Gets or sets an alternative separator in addition to <see cref="Separator"/>.
        /// </summary>
        public string? AltSeparator { get; set; } = null;
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
                Separator = source.Separator ?? _default.Separator,
                AltSeparator = source.AltSeparator ?? _default.AltSeparator,
            };
        }
    }
}

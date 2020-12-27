// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroElements.Metadata.SystemTextJson
{
    /// <summary>
    /// Helps to copy <see cref="Utf8JsonWriter"/> with other <see cref="JsonWriterOptions"/>.
    /// This is not possible with public API so Reflection is used to copy writer internals.
    /// See also: https://stackoverflow.com/questions/63376873/in-system-text-json-is-it-possible-to-specify-custom-indentation-rules.
    /// Usage:
    /// <code>
    /// if (Options.WriteArraysInOneRow and propertyType.IsArray and writer.Options.Indented)
    /// {
    ///     // Create NotIndented writer
    ///     Utf8JsonWriter writerCopy = writer.CloneNotIndented();
    ///
    ///     // Write array
    ///     JsonSerializer.Serialize(writerCopy, array, options);
    ///
    ///     // Copy internal state back to writer
    ///     writerCopy.CopyStateTo(writer);
    /// }
    /// </code>
    /// </summary>
    public static class Utf8JsonWriterCopier
    {
        private class Utf8JsonWriterReflection
        {
            private IReadOnlyCollection<string> FieldsToCopyNames { get; } = new[] { "_arrayBufferWriter", "_memory", "_inObject", "_tokenType", "_bitStack", "_currentDepth" };

            private IReadOnlyCollection<string> PropertiesToCopyNames { get; } = new[] { "BytesPending", "BytesCommitted" };

            private FieldInfo[] Fields { get; }

            private PropertyInfo[] Properties { get; }

            internal FieldInfo OutputField { get; }

            internal FieldInfo StreamField { get; }

            internal FieldInfo[] FieldsToCopy { get; }

            internal PropertyInfo[] PropertiesToCopy { get; }

            public Utf8JsonWriterReflection()
            {
                Fields = typeof(Utf8JsonWriter).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                Properties = typeof(Utf8JsonWriter).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                OutputField = Fields.FirstOrDefault(info => info.Name == "_output")!;
                StreamField = Fields.FirstOrDefault(info => info.Name == "_stream")!;

                FieldsToCopy = FieldsToCopyNames
                    .Select(name => Fields.FirstOrDefault(info => info.Name == name))
                    .Where(info => info != null)
                    .ToArray();

                PropertiesToCopy = PropertiesToCopyNames
                    .Select(name => Properties.FirstOrDefault(info => info.Name == name))
                    .Where(info => info != null)
                    .ToArray();
            }

            public void AssertStateIsValid()
            {
                if (OutputField == null)
                    throw new ArgumentException("Field _output is not found. API Changed!");
                if (StreamField == null)
                    throw new ArgumentException("Field _stream is not found. API Changed!");
                if (FieldsToCopy.Length != FieldsToCopyNames.Count)
                    throw new ArgumentException("Not all FieldsToCopy found in Utf8JsonWriter. API Changed!");
                if (PropertiesToCopy.Length != PropertiesToCopyNames.Count)
                    throw new ArgumentException("Not all FieldsToCopy found in Utf8JsonWriter. API Changed!");
            }
        }

        private static readonly Utf8JsonWriterReflection _reflectionCache = new Utf8JsonWriterReflection();

        /// <summary>
        /// Clones <see cref="Utf8JsonWriter"/> with new <see cref="JsonWriterOptions"/>.
        /// </summary>
        /// <param name="writer">Source writer.</param>
        /// <param name="newOptions">Options to use in new writer.</param>
        /// <returns>New copy of <see cref="Utf8JsonWriter"/> with new options.</returns>
        public static Utf8JsonWriter Clone(this Utf8JsonWriter writer, JsonWriterOptions newOptions)
        {
            _reflectionCache.AssertStateIsValid();

            Utf8JsonWriter writerCopy;

            // Get internal output to use in new writer
            IBufferWriter<byte>? output = (IBufferWriter<byte>?)_reflectionCache.OutputField.GetValue(writer);
            if (output != null)
            {
                // Create copy
                writerCopy = new Utf8JsonWriter(output, newOptions);
            }
            else
            {
                // Get internal stream to use in new writer
                Stream? stream = (Stream?)_reflectionCache.StreamField.GetValue(writer);

                // Create copy
                writerCopy = new Utf8JsonWriter(stream, newOptions);
            }

            // Copy internal state
            writer.CopyStateTo(writerCopy);

            return writerCopy;
        }

        /// <summary>
        /// Clones <see cref="Utf8JsonWriter"/> and sets <see cref="JsonWriterOptions.Indented"/> to false.
        /// </summary>
        /// <param name="writer">Source writer.</param>
        /// <returns>New copy of <see cref="Utf8JsonWriter"/>.</returns>
        public static Utf8JsonWriter CloneNotIndented(this Utf8JsonWriter writer)
        {
            JsonWriterOptions newOptions = writer.Options;
            newOptions.Indented = false;

            return Clone(writer, newOptions);
        }

        /// <summary>
        /// Clones <see cref="Utf8JsonWriter"/> and sets <see cref="JsonWriterOptions.Indented"/> to true.
        /// </summary>
        /// <param name="writer">Source writer.</param>
        /// <returns>New copy of <see cref="Utf8JsonWriter"/>.</returns>
        public static Utf8JsonWriter CloneIndented(this Utf8JsonWriter writer)
        {
            JsonWriterOptions newOptions = writer.Options;
            newOptions.Indented = true;

            return Clone(writer, newOptions);
        }

        /// <summary>
        /// Copies internal state of one writer to another.
        /// </summary>
        /// <param name="sourceWriter">Source writer.</param>
        /// <param name="targetWriter">Target writer.</param>
        public static void CopyStateTo(this Utf8JsonWriter sourceWriter, Utf8JsonWriter targetWriter)
        {
            foreach (var fieldInfo in _reflectionCache.FieldsToCopy)
            {
                fieldInfo.SetValue(targetWriter, fieldInfo.GetValue(sourceWriter));
            }

            foreach (var propertyInfo in _reflectionCache.PropertiesToCopy)
            {
                propertyInfo.SetValue(targetWriter, propertyInfo.GetValue(sourceWriter));
            }
        }

        /// <summary>
        /// Clones <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="options">Source options.</param>
        /// <returns>New instance of <see cref="JsonSerializerOptions"/>.</returns>
        public static JsonSerializerOptions Clone(this JsonSerializerOptions options)
        {
            JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = options.AllowTrailingCommas,
                WriteIndented = options.WriteIndented,
                PropertyNamingPolicy = options.PropertyNamingPolicy,
                DefaultBufferSize = options.DefaultBufferSize,
                DictionaryKeyPolicy = options.DictionaryKeyPolicy,
                Encoder = options.Encoder,
                IgnoreNullValues = options.IgnoreNullValues,
                IgnoreReadOnlyProperties = options.IgnoreReadOnlyProperties,
                MaxDepth = options.MaxDepth,
                PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
                ReadCommentHandling = options.ReadCommentHandling,
            };

            foreach (JsonConverter jsonConverter in options.Converters)
            {
                serializerOptions.Converters.Add(jsonConverter);
            }

            return serializerOptions;
        }
    }
}

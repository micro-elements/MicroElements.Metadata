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
    /// This is not possible with public API.
    /// </summary>
    public static class Utf8JsonWriterCopier
    {
        private class Utf8JsonWriterReflection
        {
            public readonly IReadOnlyCollection<string> FieldsToCopyNames = new[] { "_arrayBufferWriter", "_memory", "_inObject", "_tokenType", "_bitStack", "_currentDepth" };

            public readonly IReadOnlyCollection<string> PropertiesToCopyNames = new[] { "BytesPending", "BytesCommitted" };

            public FieldInfo[] Fields { get; }

            public PropertyInfo[] Properties { get; }

            public FieldInfo OutputField { get; }

            public FieldInfo StreamField { get; }

            public FieldInfo[] FieldsToCopy { get; }

            public PropertyInfo[] PropertiesToCopy { get; }

            public Utf8JsonWriterReflection()
            {
                Fields = typeof(Utf8JsonWriter).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                Properties = typeof(Utf8JsonWriter).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                OutputField = Fields.FirstOrDefault(info => info.Name == "_output");
                StreamField = Fields.FirstOrDefault(info => info.Name == "_stream");

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

        public static Utf8JsonWriter CloneNotIndented(this Utf8JsonWriter writer)
        {
            JsonWriterOptions notIndented = writer.Options;
            notIndented.Indented = false;

            return Clone(writer, notIndented);
        }

        public static Utf8JsonWriter CloneIndented(this Utf8JsonWriter writer)
        {
            JsonWriterOptions notIndented = writer.Options;
            notIndented.Indented = true;

            return Clone(writer, notIndented);
        }

        public static void CopyStateTo(this Utf8JsonWriter writer, Utf8JsonWriter writerCopy)
        {
            foreach (var fieldInfo in _reflectionCache.FieldsToCopy)
            {
                fieldInfo.SetValue(writerCopy, fieldInfo.GetValue(writer));
            }

            foreach (var propertyInfo in _reflectionCache.PropertiesToCopy)
            {
                propertyInfo.SetValue(writerCopy, propertyInfo.GetValue(writer));
            }
        }

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
                Converters = { },
            };

            foreach (JsonConverter jsonConverter in options.Converters)
            {
                serializerOptions.Converters.Add(jsonConverter);
            }

            return serializerOptions;
        }
    }
}

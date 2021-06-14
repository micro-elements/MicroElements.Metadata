// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace MicroElements.Metadata.NewtonsoftJson
{
    /// <summary>
    /// Serialization extensions.
    /// </summary>
    public static class JsonSerializerExtensions
    {
        /// <summary>
        /// Copies serializer with converters (Full copy).
        /// </summary>
        /// <param name="serializer">Source serializer.</param>
        /// <returns>Serializer copy.</returns>
        public static JsonSerializer CopyWithConverters(this JsonSerializer serializer)
        {
            var copy = serializer.CopyWithoutConverters();
            foreach (var converter in serializer.Converters)
            {
                copy.Converters.Add(converter);
            }

            return copy;
        }

        /// <summary>
        /// Copies serializer excluding one converter.
        /// </summary>
        /// <param name="serializer">Source serializer.</param>
        /// <param name="converterToRemove">Converter to exclude from result serializer.</param>
        /// <returns>Serializer copy.</returns>
        public static JsonSerializer CopyWithoutConverter(this JsonSerializer serializer, JsonConverter converterToRemove)
        {
            var copy = serializer.CopyWithoutConverters();
            foreach (var converter in serializer.Converters)
            {
                if (converter == converterToRemove)
                    continue;
                copy.Converters.Add(converter);
            }

            return copy;
        }

        /// <summary>
        /// Copies serializer without converters.
        /// </summary>
        /// <param name="serializer">Source serializer.</param>
        /// <returns>Serializer copy.</returns>
        public static JsonSerializer CopyWithoutConverters(this JsonSerializer serializer)
        {
            var copy = new JsonSerializer
            {
                Context = serializer.Context,
                Culture = serializer.Culture,
                ContractResolver = serializer.ContractResolver,
                ConstructorHandling = serializer.ConstructorHandling,
                CheckAdditionalContent = serializer.CheckAdditionalContent,
                DateFormatHandling = serializer.DateFormatHandling,
                DateFormatString = serializer.DateFormatString,
                DateParseHandling = serializer.DateParseHandling,
                DateTimeZoneHandling = serializer.DateTimeZoneHandling,
                DefaultValueHandling = serializer.DefaultValueHandling,
                EqualityComparer = serializer.EqualityComparer,
                FloatFormatHandling = serializer.FloatFormatHandling,
                Formatting = serializer.Formatting,
                FloatParseHandling = serializer.FloatParseHandling,
                MaxDepth = serializer.MaxDepth,
                MetadataPropertyHandling = serializer.MetadataPropertyHandling,
                MissingMemberHandling = serializer.MissingMemberHandling,
                NullValueHandling = serializer.NullValueHandling,
                ObjectCreationHandling = serializer.ObjectCreationHandling,
                PreserveReferencesHandling = serializer.PreserveReferencesHandling,
                ReferenceResolver = serializer.ReferenceResolver,
                ReferenceLoopHandling = serializer.ReferenceLoopHandling,
                StringEscapeHandling = serializer.StringEscapeHandling,
                TraceWriter = serializer.TraceWriter,
                TypeNameHandling = serializer.TypeNameHandling,
                SerializationBinder = serializer.SerializationBinder,
                TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling,
            };

            return copy;
        }
    }
}

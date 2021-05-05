// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace MicroElements.Metadata.Parsing
{
    public class CollectionParser : IValueParser
    {
        public CollectionParser(Type type)
        {
            Type = type;
        }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public IParseResult ParseUntyped(string? source)
        {
            string[] strings = source.TrimStart('[').TrimEnd(']').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Type elementType = typeof(string);
            if (Type.IsArray)
                elementType = Type.GetElementType();

            if (elementType == typeof(string))
                return ParseResult.Success(strings);

            IValueParser parser = GetParser(elementType);

            var elements = strings
                .Select(s => parser.ParseUntyped(s))
                .ToArray();

            if (elements.All(result => result.IsSuccess))
            {
                var values = elements.Select(result => result.ValueUntyped).ToArray();
                return ParseResult.Success<object>(values);
            }

            var errors = elements.Where(result => result.Error != null).Select(result => result.Error).ToArray();

            return new ParseResult<object>(isSuccess: false, value: null, error: errors[0]);
        }

        private IValueParser GetParser(Type elementType)
        {
            throw new NotImplementedException();
        }
    }
}

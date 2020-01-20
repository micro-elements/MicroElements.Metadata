// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.MetadataModel
{
    public static class ParserProviderExtensions
    {
        public static IReadOnlyList<IPropertyValue> ParseProperties(this IParserProvider parserProvider, IReadOnlyDictionary<string, string> row, IValueParser defaultValueParser)
        {
            Option<IPropertyValue> ParseRowOrGetDefault(IPropertyParser propertyParser) =>
                row.GetValueAsOption(propertyParser.SourceName)
                    .Match(textValue => ParseUntyped(propertyParser, textValue), propertyParser.GetDefaultValueUntyped);

            var propertyValues = parserProvider.Parsers
                .Select(ParseRowOrGetDefault)
                .SelectMany(propertyValue => propertyValue)
                .ToList();

            return propertyValues;
        }

        public static Option<IPropertyValue> ParseUntyped(IPropertyParser propertyParser, string textValue) =>
            GenericParse(propertyParser.PropertyType)(propertyParser, textValue);

        public static readonly Func<Type, Func<IPropertyParser, string, Option<IPropertyValue>>> GenericParse =
            CodeCompiler.CreateCompiledFunc<IPropertyParser, string, Option<IPropertyValue>>(Parse<CodeCompiler.GenericType>);

        public static Option<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string textValue)
        {
            var propertyParser = (IPropertyParser<T>)propertyParserUntyped;

            return propertyParser
                .ValueParser
                .Parse(textValue)
                .Map(value => (IPropertyValue)new PropertyValue<T>(propertyParser.TargetProperty, value, ValueSource.Defined));
        }
    }
}

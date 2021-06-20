﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.Parsing;
using MicroElements.Reflection;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions for <see cref="IPropertyParser"/>.
    /// </summary>
    public static class PropertyParserExtensions
    {
        /// <summary>
        /// Searches value to parse in <paramref name="sourceRow"/> than parses and returns optional parse result.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="sourceRow">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static ParseResult<IPropertyValue> ParseRowUntyped(this IPropertyParser propertyParser, IReadOnlyDictionary<string, string> sourceRow, bool useDefaultValueForAbsent = true)
        {
            if (sourceRow.TryGetValue(propertyParser.SourceName, out var value) && value.IsNotNull())
            {
                return ParseUntyped(propertyParser, value);
            }

            if (useDefaultValueForAbsent)
                return propertyParser.GetDefaultValueUntyped();

            return ParseResult.Cache<IPropertyValue>.None;
        }

        public static ParseResult<IPropertyValue> ParseRowUntyped(this IParserRule parserRule, IReadOnlyDictionary<string, string> sourceRow, bool useDefaultValueForAbsent = false)
        {
            if (parserRule.SourceName != null)
            {
                if (sourceRow.TryGetValue(parserRule.SourceName, out string textValue))
                {
                    return ParseUntyped2(parserRule, textValue);
                }
            }

            return ParseResult.Cache<IPropertyValue>.None;
        }

        /// <summary>
        /// Parses <paramref name="textValue"/> with <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="textValue">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static ParseResult<IPropertyValue> ParseUntyped(this IPropertyParser propertyParser, string textValue)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, string, ParseResult<IPropertyValue>>(propertyParser.TargetType, "Parse", Parse<CodeCompiler.GenericType>);
            return func(propertyParser, textValue);

            static ParseResult<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string textValue)
            {
                var propertyParserTyped = (IPropertyParser<T>)propertyParserUntyped;

                return propertyParserTyped
                    .ValueParser
                    .Parse(textValue)
                    .Map(value => (IPropertyValue)new PropertyValue<T>(propertyParserTyped.TargetProperty, value, ValueSource.Defined));
            }
        }

        public static ParseResult<IPropertyValue> ParseUntyped2(this IParserRule parserRule, string textValue)
        {
            if (parserRule.TargetProperty != null)
            {
                IParseResult parseResult = parserRule.Parser.ParseUntyped(textValue);
                if (parseResult.IsSuccess)
                {
                    object? parsedValue = parseResult.ValueUntyped;
                    IPropertyValue propertyValue = PropertyValueFactory.Default.CreateUntyped(parserRule.TargetProperty, parsedValue);
                    return propertyValue.ToParseResult();
                }

                return ParseResult.Failed<IPropertyValue>(parseResult.Error);
            }

            return ParseResult.Failed<IPropertyValue>();
        }

        /// <summary>
        /// Gets <see cref="IPropertyParser{T}.DefaultValue"/> for untyped <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <returns>Returns optional <see cref="IPropertyValue"/> if default value evaluated for <paramref name="propertyParser"/>.</returns>
        public static ParseResult<IPropertyValue> GetDefaultValueUntyped(this IPropertyParser propertyParser)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, ParseResult<IPropertyValue>>(propertyParser.TargetType, "GetDefaultValue", GetDefaultValue<CodeCompiler.GenericType>);
            return func(propertyParser);

            static ParseResult<IPropertyValue> GetDefaultValue<T>(IPropertyParser propertyParserUntyped)
            {
                IPropertyParser<T> propertyParser = (IPropertyParser<T>)propertyParserUntyped;
                if (propertyParser.DefaultValue != null)
                {
                    var defaultValue = propertyParser.DefaultValue.Value;
                    return ParseResult.Success<IPropertyValue>(new PropertyValue<T>(propertyParser.TargetProperty, defaultValue, ValueSource.DefaultValue));
                }

                return ParseResult.Failed<IPropertyValue>();
            }
        }
    }
}

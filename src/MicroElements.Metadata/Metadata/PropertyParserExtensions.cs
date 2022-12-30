// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Diagnostics;
using MicroElements.Metadata.Formatting;
using MicroElements.Metadata.Parsing;
using MicroElements.Reflection.CodeCompiler;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Extensions for <see cref="IPropertyParser"/>.
    /// </summary>
    public static class PropertyParserExtensions
    {
        /// <summary>
        /// Searches value to parse in <paramref name="sourceRow"/> then parses and returns optional parse result.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="sourceRow">Value to parse.</param>
        /// <returns>Parse result.</returns>
        public static ParseResult<IPropertyValue> ParseRowUntyped(
            this IPropertyParser propertyParser,
            IReadOnlyDictionary<string, string?> sourceRow)
        {
            bool isValuePresent = sourceRow.TryGetValue(propertyParser.SourceName, out string? valueToParse);
            return ParseValueUntyped(propertyParser, valueToParse, isValuePresent);
        }

        /// <summary>
        /// Parses text value according <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Property parser.</param>
        /// <param name="valueToParse">Value to parse. Can be null.</param>
        /// <param name="isValuePresent">valueToParse was provided whether it is not null or null.</param>
        /// <returns>Parse result.</returns>
        public static ParseResult<IPropertyValue> ParseValueUntyped(
            this IPropertyParser propertyParser,
            string? valueToParse,
            bool isValuePresent = true)
        {
            bool isValueAbsent = !isValuePresent;
            bool isValueNull = valueToParse is null;

            if ((isValueAbsent || isValueNull) && propertyParser.DefaultSourceValue is { Value: { } defaultSourceValue })
            {
                // Use provided value as source value.
                valueToParse = defaultSourceValue;

                isValueNull = valueToParse is null;
                isValuePresent = true;
                isValueAbsent = false;
            }

            if (isValueAbsent && propertyParser.DefaultValueUntyped != null)
            {
                return propertyParser.GetDefaultValueUntyped();
            }

            if (isValueNull && propertyParser.DefaultValueUntyped != null)
            {
                return propertyParser.GetDefaultValueUntyped();
            }

            if (isValuePresent)
            {
                ParseResult<IPropertyValue> parseResult = ParseUntyped(propertyParser, valueToParse);
                parseResult = parseResult.WrapError(message =>
                {
                    string errorCause = message != null ? $" ParseError: '{message.FormattedMessage}'" : string.Empty;
                    return new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed from source '{valueToParse.FormatValue()}'{errorCause}", MessageSeverity.Error);
                });
                return parseResult;
            }

            // Return failed result.
            return ParseResult.Failed<IPropertyValue>(new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed because source '{propertyParser.SourceName}' is absent.", MessageSeverity.Error));
        }

        /// <summary>
        /// Parses <paramref name="textValue"/> with <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="textValue">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static ParseResult<IPropertyValue> ParseUntyped(this IPropertyParser propertyParser, string? textValue)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, string?, ParseResult<IPropertyValue>>(propertyParser.TargetType, "Parse", Parse<CodeCompiler.GenericType>);
            return func(propertyParser, textValue);

            static ParseResult<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string? textValue)
            {
                var propertyParserTyped = (IPropertyParser<T>)propertyParserUntyped;

                return propertyParserTyped
                    .ValueParser
                    .Parse(textValue)
                    .Map(value => (IPropertyValue)new PropertyValue<T>(propertyParserTyped.TargetProperty, value, ValueSource.Defined));
            }
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
                IPropertyValue? propertyValue = null;
                IPropertyParser<T> propertyParser = (IPropertyParser<T>)propertyParserUntyped;
                if (propertyParser.DefaultValue != null)
                {
                    var defaultValue = propertyParser.DefaultValue.Value;
                    propertyValue = new PropertyValue<T>(propertyParser.TargetProperty, defaultValue, ValueSource.DefaultValue);
                    return ParseResult.Success(propertyValue);
                }

                propertyValue = PropertyValue.Default(propertyParser.TargetProperty);
                return ParseResult.Success(propertyValue);
            }
        }
    }
}

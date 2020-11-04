// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;

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
        public static Option<IPropertyValue> ParseRowUntyped(this IPropertyParser propertyParser, IReadOnlyDictionary<string, string> sourceRow)
        {
            return sourceRow
                .GetValueAsOption(propertyParser.SourceName)
                .Match(textValue => ParseUntyped(propertyParser, textValue), propertyParser.GetDefaultValueUntyped);
        }

        /// <summary>
        /// Parses <paramref name="textValue"/> with <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="textValue">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static Option<IPropertyValue> ParseUntyped(this IPropertyParser propertyParser, string textValue)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, string, Option<IPropertyValue>>(propertyParser.TargetType, "Parse", Parse<CodeCompiler.GenericType>);
            return func(propertyParser, textValue);

            static Option<IPropertyValue> Parse<T>(IPropertyParser propertyParserUntyped, string textValue)
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
        public static Option<IPropertyValue> GetDefaultValueUntyped(this IPropertyParser propertyParser)
        {
            var func = CodeCompiler.CachedCompiledFunc<IPropertyParser, Option<IPropertyValue>>(propertyParser.TargetType, "GetDefaultValue", GetDefaultValue<CodeCompiler.GenericType>);
            return func(propertyParser);

            static Option<IPropertyValue> GetDefaultValue<T>(IPropertyParser propertyParserUntyped)
            {
                IPropertyParser<T> propertyParser = (IPropertyParser<T>)propertyParserUntyped;
                if (propertyParser.DefaultValue != null)
                {
                    var defaultValue = propertyParser.DefaultValue();
                    return new PropertyValue<T>(propertyParser.TargetProperty, defaultValue, ValueSource.DefaultValue);
                }

                return Option<IPropertyValue>.None;
            }
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    // IParserBehavior
    public interface IParserBehavior
    {
        IReadOnlyList<ParseResult<IPropertyValue>> ParseProperties(
            IReadOnlyDictionary<string, string?> sourceRow,
            IParserProvider parserProvider);
    }

    public static class ParserBehavior
    {
        private static IParserBehavior s_default;

        public static IParserBehavior Default
        {
            get => s_default ?? StatefullParserBehavior.Instance;
            set => s_default = value;
        }
    }

    public class StatefullParserBehavior : IParserBehavior
    {
        public static IParserBehavior Instance { get; } = new StatefullParserBehavior();

        /// <inheritdoc />
        public IReadOnlyList<ParseResult<IPropertyValue>> ParseProperties(IReadOnlyDictionary<string, string?> sourceRow, IParserProvider parserProvider)
        {
            parserProvider.AssertArgumentNotNull(nameof(parserProvider));
            sourceRow.AssertArgumentNotNull(nameof(sourceRow));

            var parseContext = new ParseContext(parserProvider, sourceRow);

            IPropertyParser[] propertyParsers = parserProvider.GetParsers().ToArray();

            foreach (IPropertyParser propertyParser in propertyParsers)
            {
                // Context for property parsing.
                var propertyParserContext = new PropertyParserContext(parseContext, propertyParser);

                // Set value to context.
                bool isValueExists = sourceRow.TryGetValue(propertyParser.SourceName, out string? valueToParse);
                propertyParserContext.SetValueToParse(valueToParse, isValueExists);

                // Optional IPropertyParserCondition.ExcludeCondition
                if (propertyParser is IPropertyParserCondition { ExcludeCondition: { } excludeCondition })
                {
                    bool shouldExclude = excludeCondition(propertyParserContext);
                    if (shouldExclude)
                        continue;
                }

                // Optional IPropertyParserCondition.IncludeCondition
                if (propertyParser is IPropertyParserCondition { IncludeCondition: { } includeCondition })
                {
                    bool shouldInclude = includeCondition(propertyParserContext);
                    if (!shouldInclude)
                        continue;
                }

                ParseResult<IPropertyValue> parseResult = ParseValueUntyped(propertyParserContext);
                parseContext.AddResult(parseResult);

                // Optional IPropertyParserNotifier
                if (propertyParser is IPropertyParserNotifier { PropertyParsed: { } onParsed })
                {
                    onParsed(propertyParserContext, parseResult);
                }
            }

            return parseContext.ParseResults;
        }

        public ParseResult<IPropertyValue> ParseValueUntyped(PropertyParserContext context)
        {
            if (context.IsValueAbsentOrNull() && context.DefaultSourceValue != null)
            {
                // Use provided value as source value.
                context.SetValueToParseFromDefaultSourceValue(context.DefaultSourceValue);
            }

            IPropertyParser propertyParser = context.PropertyParser;

            if (context.IsValueAbsent() && propertyParser.DefaultValueUntyped != null)
            {
                ParseResult<IPropertyValue> defaultValueUntyped = propertyParser.GetDefaultValueUntyped();
                return defaultValueUntyped;
            }

            if (context.IsValueNull() && propertyParser.DefaultValueUntyped != null)
            {
                ParseResult<IPropertyValue> defaultValueUntyped = propertyParser.GetDefaultValueUntyped();
                return defaultValueUntyped;
            }

            if (context.IsValueAbsent())
            {
                // Return failed result.
                return ParseResult.Failed<IPropertyValue>(new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed because source '{propertyParser.SourceName}' is absent.", MessageSeverity.Error));
            }

            ParseResult<IPropertyValue> parseResult = ParseUntyped(propertyParser, context.ValueToParse);
            parseResult = parseResult.WrapError(message =>
            {
                string errorCause = message != null ? $" ParseError: '{message.FormattedMessage}'" : string.Empty;
                return new Message($"Property '{propertyParser.TargetPropertyUntyped.Name}' not parsed from source '{context.ValueToParse.FormatValue()}'{errorCause}", MessageSeverity.Error);
            });

            return parseResult;
        }

        /// <summary>
        /// Parses <paramref name="textValue"/> with <paramref name="propertyParser"/>.
        /// </summary>
        /// <param name="propertyParser">Source property parser.</param>
        /// <param name="textValue">Value to parse.</param>
        /// <returns>Optional parse result.</returns>
        public static ParseResult<IPropertyValue> ParseUntyped(IPropertyParser propertyParser, string? textValue)
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
    }

    public enum SourceType
    {
        SourceValueIsAbsent = 0,
        SourceValueExists = 1,
        DefaultSourceValue = 8,
    }
}

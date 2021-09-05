// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Core;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents <see cref="IPropertyParser"/> condition.
    /// </summary>
    public interface IPropertyParserCondition
    {
        /// <summary>
        /// Gets a condition that defines whether the parser should be excluded.
        /// </summary>
        Func<PropertyParserContext, bool>? ExcludeCondition { get; }

        /// <summary>
        /// Gets a condition that defines whether the parser should be included.
        /// </summary>
        Func<PropertyParserContext, bool>? IncludeCondition { get; }
    }

    /// <summary>
    /// Represents <see cref="IPropertyParser"/> notifier.
    /// </summary>
    public interface IPropertyParserNotifier
    {
        /// <summary>
        /// Gets a callback that executes after parsing occurred.
        /// </summary>
        Action<PropertyParserContext, ParseResult<IPropertyValue>>? PropertyParsed { get; }
    }

    /// <summary>
    /// Represents context for parsing operation.
    /// </summary>
    public class ParseContext
    {
        private readonly List<ParseResult<IPropertyValue>> _parseResults;

        /// <summary>
        /// Gets parser provider that provides parsers.
        /// </summary>
        public IParserProvider ParserProvider { get; }

        /// <summary>
        /// Gets source for parsing.
        /// </summary>
        public IReadOnlyDictionary<string, string?> Source { get; }

        /// <summary>
        /// Gets parsed results in current parse session.
        /// </summary>
        public IReadOnlyList<ParseResult<IPropertyValue>> ParseResults => _parseResults;

        /// <summary>
        /// Gets <see cref="ParseResults"/> as <see cref="IPropertyContainer"/>.
        /// </summary>
        public IPropertyContainer PropertyContainer
        {
            get
            {
                IEnumerable<IPropertyValue> propertyValues = ParseResults.Where(result => result.IsSuccess).Select(result => result.Value!);
                return new PropertyContainer(propertyValues);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseContext"/> class.
        /// </summary>
        /// <param name="parserProvider">Parser provider.</param>
        /// <param name="source">Source for parsing.</param>
        public ParseContext(
            IParserProvider parserProvider,
            IReadOnlyDictionary<string, string?> source)
        {
            ParserProvider = parserProvider;
            Source = source;
            _parseResults = new List<ParseResult<IPropertyValue>>(source.Count);
        }

        /// <summary>
        /// Adds result to result list.
        /// </summary>
        /// <param name="parseResult">Parse result.</param>
        public void AddResult(ParseResult<IPropertyValue> parseResult)
        {
            _parseResults.Add(parseResult);
        }
    }

    /// <summary>
    /// Represents context for single property parsing operation.
    /// </summary>
    public class PropertyParserContext
    {
        /// <summary>
        /// Gets parse context.
        /// </summary>
        public ParseContext ParseContext { get; }

        /// <inheritdoc cref="Metadata.ParseContext.PropertyContainer"/>
        public IPropertyContainer PropertyContainer => ParseContext.PropertyContainer;

        /// <summary>
        /// Gets property parser.
        /// </summary>
        public IPropertyParser PropertyParser { get; }

        /// <summary>
        /// Gets value to parse.
        /// </summary>
        public string? ValueToParse { get; private set; }

        /// <summary>
        /// Gets source type.
        /// </summary>
        public SourceType SourceType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyParserContext"/> class.
        /// </summary>
        /// <param name="parseContext">Parent context.</param>
        /// <param name="propertyParser">Property parser.</param>
        public PropertyParserContext(
            ParseContext parseContext,
            IPropertyParser propertyParser)
        {
            parseContext.AssertArgumentNotNull(nameof(parseContext));
            propertyParser.AssertArgumentNotNull(nameof(propertyParser));

            ParseContext = parseContext;
            PropertyParser = propertyParser;
        }

        public void SetValueToParse(string? valueToParse, bool isValueExists)
        {
            ValueToParse = valueToParse;
            SourceType = isValueExists ? SourceType.SourceValueExists : SourceType.SourceValueIsAbsent;
        }

        public void SetValueToParseFromDefaultSourceValue(string? valueToParse)
        {
            ValueToParse = valueToParse;
            SourceType = SourceType.DefaultSourceValue;
        }

        public string? DefaultSourceValue => PropertyParser.DefaultSourceValue?.Value;

        public bool IsValueAbsent() => SourceType == SourceType.SourceValueIsAbsent;

        public bool IsSourceValueExists() => SourceType == SourceType.SourceValueExists;

        public bool IsValueNull() => ValueToParse is null;

        public bool IsValueAbsentOrNull() => IsValueAbsent() || IsValueNull();

        public bool IsValueExistsAndNotNull() => SourceType == SourceType.SourceValueExists && ValueToParse != null;
    }
}

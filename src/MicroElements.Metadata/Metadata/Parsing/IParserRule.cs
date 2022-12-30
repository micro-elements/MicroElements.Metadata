// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata.Formatting;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Parser rule attaches parser to type or property.
    /// Priority for search:
    /// 1. <see cref="TargetProperty"/>,
    /// 2. <see cref="TargetType"/>.
    /// </summary>
    public interface IParserRule
    {
        /// <summary>
        /// Gets source name that uses to find source value.
        /// <para>
        /// <![CDATA[Usage: Source:Dictionary<string, string> + SourceName:string -> SourceValue:string]]>
        /// </para>
        /// <seealso cref="ISourceValueProvider"/>
        /// </summary>
        string? SourceName { get; }

        /// <summary>
        /// Gets text value parser.
        /// </summary>
        IValueParser Parser { get; }

        /// <summary>
        /// Gets target type that <see cref="Parser"/> can parse.
        /// </summary>
        Type? TargetType { get; }

        /// <summary>
        /// Gets concrete property that should use <see cref="Parser"/>.
        /// </summary>
        IProperty? TargetProperty { get; }
    }

    /// <summary>
    /// Parser rule attaches parser to type or property.
    /// </summary>
    public class ParserRule : IParserRule
    {
        /// <summary>
        /// Empty parser rule. Uses <see cref="EmptyParser.Instance"/>.
        /// </summary>
        public static ParserRule Empty { get; } = new ParserRule(EmptyParser.Instance);

        /// <inheritdoc />
        public string? SourceName { get; }

        /// <inheritdoc />
        public IValueParser Parser { get; }

        /// <inheritdoc />
        public Type? TargetType { get; }

        /// <inheritdoc />
        public IProperty? TargetProperty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserRule"/> class.
        /// </summary>
        /// <param name="parser">Value parser.</param>
        /// <param name="targetType">Optional target type.</param>
        /// <param name="targetProperty">Optional target property.</param>
        /// <param name="sourceName">Optional source name.</param>
        public ParserRule(
            IValueParser parser,
            Type? targetType = null,
            IProperty? targetProperty = null,
            string? sourceName = null)
        {
            SourceName = sourceName;
            Parser = parser;
            TargetProperty = targetProperty;
            TargetType = targetType;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            IEnumerable<(string, string?)> KeyValuePairs()
            {
                yield return (nameof(SourceName), SourceName);
                yield return (nameof(Parser), Parser.FormatValue());
                yield return (nameof(TargetType), TargetType.FormatValue());
                yield return (nameof(TargetProperty), TargetProperty.FormatValue());
            }

            return KeyValuePairs().Where(tuple => tuple.Item2 is not null).FormatAsTuple();
        }
    }
}

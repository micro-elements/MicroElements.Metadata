// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
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
    }
}

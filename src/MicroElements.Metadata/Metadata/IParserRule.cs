// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Parser rule.
    /// Priority for search:
    /// 1. <see cref="TargetProperty"/>,
    /// 2. <see cref="TargetType"/>.
    /// </summary>
    public interface IParserRule
    {
        /// <summary>
        /// Gets concrete property that should use <see cref="Parser"/>.
        /// </summary>
        IProperty? TargetProperty { get; }

        /// <summary>
        /// Gets target type that <see cref="Parser"/> can parse.
        /// </summary>
        Type? TargetType { get; }

        /// <summary>
        /// Gets value parser.
        /// </summary>
        IValueParser Parser { get; }
    }

    public class ParserRule : IParserRule
    {
        /// <inheritdoc />
        public IProperty? TargetProperty { get; set; }

        /// <inheritdoc />
        public Type? TargetType { get; set; }

        /// <inheritdoc />
        public IValueParser Parser { get; set; }

        public ParserRule(
            IValueParser parser,
            Type? targetType = null,
            IProperty? targetProperty = null)
        {
            TargetProperty = targetProperty;
            TargetType = targetType;
            Parser = parser;
        }
    }

    public static class ParserRuleExtensions
    {


        public static IParserRule? GetParser(
            this IReadOnlyCollection<IParserRule> parserRules,
            IProperty property,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            IParserRule? parserRule = null;

            if (parserRule == null)
            {
                propertyComparer ??= PropertyComparer.ByReferenceComparer;
                parserRule = parserRules.FirstOrDefault(rule => rule.TargetProperty != null && propertyComparer.Equals(rule.TargetProperty, property));
            }

            if (parserRule == null)
            {
                parserRule = parserRules.FirstOrDefault(rule => rule.TargetType != null && property.Type == rule.TargetType);
            }

            return parserRule;
        }
    }
}

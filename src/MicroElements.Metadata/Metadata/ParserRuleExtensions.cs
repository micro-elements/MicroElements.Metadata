// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Metadata.Parsers;

namespace MicroElements.Metadata
{
    public static class ParserRuleExtensions
    {
        public static IParserRule? GetParserRule(
            this IReadOnlyCollection<IParserRule> parserRules,
            IProperty property,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            IParserRule? parserRule = null;

            if (parserRule == null)
            {
                // Search by concrete property.
                propertyComparer ??= PropertyComparer.ByReferenceComparer;
                parserRule = parserRules
                    .Where(rule => rule.TargetProperty != null)
                    .FirstOrDefault(rule => propertyComparer.Equals(rule.TargetProperty!, property));
            }

            if (parserRule == null)
            {
                // Search for type.
                parserRule = parserRules
                    .Where(rule => rule.TargetType != null)
                    .FirstOrDefault(rule => rule.TargetType == property.Type);
            }

            if (parserRule == null)
            {
                // Enum support.
                if (property.Type.IsEnum)
                {
                    parserRule = new ParserRule(new EnumUntypedParser(property.Type), property.Type);
                }
            }

            bool searchNotTargetedParser = false;
            if (parserRule == null && searchNotTargetedParser)
            {
                // Not targeted parser
                parserRule = parserRules
                    .FirstOrDefault(rule => rule.TargetType == null && rule.TargetProperty == null);
            }

            return parserRule;
        }
    }
}

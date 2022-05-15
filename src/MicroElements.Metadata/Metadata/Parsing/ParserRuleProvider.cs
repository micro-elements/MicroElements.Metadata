// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Reflection.TypeExtensions;
using MicroElements.Validation;

namespace MicroElements.Metadata.Parsing
{
    public class ParserRuleProvider : IParserRuleProvider
    {
        private static readonly IPropertyFactory DefaultPropertyFactory = new CachedPropertyFactory();

        private readonly IReadOnlyCollection<IParserRule> _parserRules;
        private readonly IEqualityComparer<IProperty> _propertyComparer;
        private readonly IPropertyFactory _propertyFactory;

        public static ParserRuleProvider Create(
            IValueParserProvider valueParserProvider,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            valueParserProvider.AssertArgumentNotNull(nameof(valueParserProvider));

            var parserRules = valueParserProvider.GetValueParsers().ToParserRules().ToArray();
            return new ParserRuleProvider(parserRules, propertyComparer);
        }

        public ParserRuleProvider(
            IReadOnlyCollection<IParserRule> parserRules,
            IEqualityComparer<IProperty>? propertyComparer = null)
        {
            parserRules.AssertArgumentNotNull(nameof(parserRules));

            _parserRules = parserRules;
            _propertyComparer = propertyComparer ?? Metadata.PropertyComparer.ByReferenceComparer;
            _propertyFactory = DefaultPropertyFactory;
        }

        /// <inheritdoc />
        public IEqualityComparer<IProperty> PropertyComparer => _propertyComparer;

        /// <inheritdoc />
        public IReadOnlyCollection<IParserRule> GetParserRules() => _parserRules;

        /// <inheritdoc />
        public IParserRule? GetParserRule(Type type)
        {
            IProperty property = _propertyFactory.Create(type, type.GetFriendlyName());
            return GetParserRule(property);
        }

        /// <inheritdoc />
        public IParserRule? GetParserRule(IProperty property)
        {
            IParserRule? parserRule = null;

            if (parserRule == null)
            {
                // Search by concrete property.
                parserRule = _parserRules
                    .Where(rule => rule.TargetProperty != null)
                    .FirstOrDefault(rule => _propertyComparer.Equals(rule.TargetProperty!, property));
            }

            if (parserRule == null)
            {
                // Search for type.
                parserRule = _parserRules
                    .Where(rule => rule.TargetType != null)
                    .FirstOrDefault(rule => rule.TargetType == property.Type);
            }

            if (parserRule == null)
            {
                if (property.Type.IsEnum)
                {
                    // Enum.
                    parserRule = new ParserRule(new EnumUntypedParser(property.Type), property.Type);
                }
                else if (Nullable.GetUnderlyingType(property.Type) is { } baseType && baseType.IsEnum)
                {
                    // Nullable enum.
                    parserRule = new ParserRule(new EnumUntypedParser(baseType, allowNull: true), property.Type);
                }
            }

            if (parserRule == null)
            {
                if (property.Type.IsAssignableTo(typeof(ICollection)) || property.Type.Name.StartsWith("IReadOnlyCollection"))
                {
                    parserRule = new ParserRule(new CollectionParser(property.Type, this), property.Type);
                }
            }

            bool searchNotTargetedParser = false;
            if (parserRule == null && searchNotTargetedParser)
            {
                // Not targeted parser
                parserRule = _parserRules
                    .FirstOrDefault(rule => rule.TargetType == null && rule.TargetProperty == null);
            }

            return parserRule;
        }
    }
}

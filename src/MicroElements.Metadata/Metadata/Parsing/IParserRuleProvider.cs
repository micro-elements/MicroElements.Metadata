// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Provides parsers rules.
    /// Knows how to match source and target type or property.
    /// </summary>
    public interface IParserRuleProvider
    {
        /// <summary>
        /// Gets PropertyComparer that uses for property search.
        /// </summary>
        IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <summary>
        /// Gets parser rules.
        /// </summary>
        /// <returns>Enumerable of <see cref="IParserRule"/>.</returns>
        IReadOnlyCollection<IParserRule> GetParserRules();

        /// <summary>
        /// Gets parser rule for type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Parser rule for type.</returns>
        IParserRule? GetParserRule(Type type);

        /// <summary>
        /// Gets parser rule for property.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <returns>Parser rule for property.</returns>
        IParserRule? GetParserRule(IProperty property);
    }
}

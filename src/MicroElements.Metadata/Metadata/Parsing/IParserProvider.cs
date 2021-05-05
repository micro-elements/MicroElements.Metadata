// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property parsers.
    /// </summary>
    public interface IParserProvider
    {
        /// <summary>
        /// Gets property parsers.
        /// </summary>
        /// <returns>Enumerable of <see cref="IPropertyParser"/>.</returns>
        IEnumerable<IPropertyParser> GetParsers();
    }

    /// <summary>
    /// Provides value parsers.
    /// Value parsers knows only how to parse text to some type.
    /// For more high-level parsing use <see cref="IParserRuleProvider"/>.
    /// </summary>
    public interface IValueParserProvider
    {
        /// <summary>
        /// Gets value parsers.
        /// </summary>
        /// <returns>Enumerable of <see cref="IValueParser"/>.</returns>
        IEnumerable<IValueParser> GetValueParsers();
    }

    /// <summary>
    /// Provides parsers rules.
    /// Knows how to match source and target type or property.
    /// </summary>
    public interface IParserRuleProvider
    {
        /// <summary>
        /// Gets parser rules.
        /// </summary>
        /// <returns>Enumerable of <see cref="IParserRule"/>.</returns>
        IEnumerable<IParserRule> GetParserRules();
    }

    public interface IValueParserProvider2
    {
        /// <summary>
        /// Gets value parsers.
        /// </summary>
        /// <returns>Enumerable of <see cref="IValueParser"/>.</returns>
        IParserRule GetParser(IProperty property);
    }
}

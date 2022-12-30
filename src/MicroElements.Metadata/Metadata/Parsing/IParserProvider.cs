// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata.Parsing
{
    /// <summary>
    /// Provides property parsers.
    /// </summary>
    [Obsolete("Use IParserRuleProvider where possible instead IParserProvider")]
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
    /// Represents parser provider with discriminator.
    /// </summary>
    public interface IParserProviderWithDiscriminator
    {
        /// <summary>
        /// Gets the property that can be used to determine one of parse conditions.
        /// </summary>
        IProperty? Discriminator { get; }
    }
}

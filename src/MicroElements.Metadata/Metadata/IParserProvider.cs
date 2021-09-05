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

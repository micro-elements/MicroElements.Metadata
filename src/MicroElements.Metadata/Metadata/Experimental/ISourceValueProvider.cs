// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides source values for parsing.
    /// Can be used with <see cref="IParserRule.SourceName"/> to get source value for parsing.
    /// </summary>
    public interface ISourceValueProvider
    {
        bool TryGetSourceValue(string name, out string value);
    }

    public class SourceNameProvider : ISourceValueProvider
    {
        private IDictionary<string, string> _values;

        /// <inheritdoc />
        public bool TryGetSourceValue(string name, out string value)
        {
            return _values.TryGetValue(name, out value);
        }
    }
}

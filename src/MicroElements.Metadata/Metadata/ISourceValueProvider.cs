// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides source values for parsing.
    /// Can be used with <see cref="IParserRule.SourceName"/> to get source value for parsing.
    /// </summary>
    public interface ISourceValueProvider
    {
        Option<string> GetSourceValue(string name);
    }

    public class SourceNameProvider : ISourceValueProvider
    {
        private IDictionary<string, string> _values;

        /// <inheritdoc />
        public Option<string> GetSourceValue(string name)
        {
            if (_values.TryGetValue(name, out var value))
                return value;
            return Option<string>.None;
        }
    }
}

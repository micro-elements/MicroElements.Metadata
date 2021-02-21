// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    /*
     * See Also
     * https://www.w3.org/2019/wot/json-schema
     */


    public interface ICombinator : IMetadata
    {
        CombinatorType Type { get; }

        IReadOnlyCollection<IMetadata> Values { get; }
    }

    public enum CombinatorType
    {
        And = 1,
        Or = 2,
        Xor = 3,
        Not = 4,
        AllOf = And,
        AnyOf = Or,
        OneOf = Xor
    }

    public class Combinator : ICombinator
    {
        /// <inheritdoc />
        public CombinatorType Type { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IMetadata> Values { get; }

        public Combinator(CombinatorType type, IReadOnlyCollection<IMetadata> values)
        {
            Type = type;
            Values = values;
        }
    }

    /// <summary>
    /// https://json-schema.org/understanding-json-schema/reference/combining.html
    /// </summary>
    public interface IOneOf : IMetadata
    {
        IReadOnlyCollection<IMetadata> Values { get; }
    }

    public interface IAllOf : IMetadata
    {
        IReadOnlyCollection<IMetadata> Values { get; }
    }

    public interface IAnyOf : IMetadata
    {
        IReadOnlyCollection<IMetadata> Values { get; }
    }

    public interface IOr : IAnyOf
    {
    }

    public interface IAnd : IAllOf
    {
    }

}

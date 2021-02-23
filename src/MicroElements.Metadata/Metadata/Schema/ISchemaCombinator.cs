// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents schema combination.
    /// JsonSchema: https://json-schema.org/understanding-json-schema/reference/combining.html.
    /// JsonSchema: https://www.w3.org/2019/wot/json-schema.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.AnySchema)]
    public interface ISchemaCombinator : IMetadata
    {
        /// <summary>
        /// Gets combinator type.
        /// </summary>
        CombinatorType Type { get; }

        /// <summary>
        /// Gets values to combine.
        /// </summary>
        IReadOnlyCollection<ISchema> Values { get; }
    }

    /// <summary>
    /// Combinator type.
    /// </summary>
    public enum CombinatorType
    {
        AllOf = 1,
        AnyOf = 2,
        OneOf = 3,
        Not = 4,

        Unsupported = 0,

        And = AllOf,
        Or = AnyOf,
        Xor = OneOf,

        Xsd_All = AllOf,
        Xsd_Choice = OneOf,

        // Json does not support property order
        Xsd_Sequence = AllOf,
    }
}

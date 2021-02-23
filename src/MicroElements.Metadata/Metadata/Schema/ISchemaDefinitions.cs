// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata.Schema
{
    public interface ISchemaDefinitions : IMetadata
    {
        IReadOnlyDictionary<string, ISchema> Definitions { get; }
    }
}

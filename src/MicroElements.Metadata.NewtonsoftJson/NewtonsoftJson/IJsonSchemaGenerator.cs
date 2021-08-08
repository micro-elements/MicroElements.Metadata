// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.NewtonsoftJson
{
    public interface IJsonSchemaGenerator
    {
        object GenerateSchema(ISchema schema);
    }
}

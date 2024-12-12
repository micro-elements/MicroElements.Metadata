// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Serialization
{
    public interface IJsonSchemaGenerator
    {
        object GenerateSchema(ISchema schema);
    }

    public interface IJsonSchemaSerializer
    {
        IEnumerable<(string Key, object? Value)> GenerateSchema(ISchema schema);
    }


    public class GenerateSchemaContext
    {
        public ISchema Schema { get; init; }

        public string? PropertyName { get; set; }
        public object? ResultAsObject { get; set; }
        public object? ResultAsAray { get; set; }
    }
}

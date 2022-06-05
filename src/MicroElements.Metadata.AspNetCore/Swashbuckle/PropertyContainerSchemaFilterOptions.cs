// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Swashbuckle
{
    /// <summary>
    /// Options to configure <see cref="PropertyContainerSchemaFilter"/>.
    /// </summary>
    public class PropertyContainerSchemaFilterOptions : ICloneable<PropertyContainerSchemaFilterOptions>
    {
        /// <summary>
        /// Gets or sets a function that resolves property name by proper naming strategy.
        /// </summary>
        public Func<string, string>? ResolvePropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether known schemas should be generated as separate schemas and should be references by ref.
        /// </summary>
        public bool GenerateKnownSchemasAsRefs { get; set; } = true;
    }
}

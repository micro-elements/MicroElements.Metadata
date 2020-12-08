// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata.Swashbuckle
{
    /// <summary>
    /// Options to configure <see cref="PropertyContainerSchemaFilter"/>.
    /// </summary>
    public class PropertyContainerSchemaFilterOptions
    {
        /// <summary>
        /// Gets or sets a function that resolves property name by proper naming strategy.
        /// </summary>
        public Func<string, string>? ResolvePropertyName { get; set; }
    }
}

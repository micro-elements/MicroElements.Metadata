// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Represents property with value.
    /// </summary>
    public class PropertyValueContract
    {
        /// <summary>
        /// Gets or sets property name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets property value.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets property value type.
        /// </summary>
        public string? Type { get; set; }
    }
}

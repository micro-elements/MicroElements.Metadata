// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public class PropertyContainerContract
    {
        /// <summary>
        /// Gets or sets properties.
        /// </summary>
        public PropertyValueContract[]? Properties { get; set; }
    }
}

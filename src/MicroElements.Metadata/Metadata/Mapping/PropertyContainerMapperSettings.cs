// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Diagnostics;
using MicroElements.Metadata.Formatting;

namespace MicroElements.Metadata.Mapping
{
    /// <summary>
    /// Settings for property container mapping.
    /// </summary>
    public class PropertyContainerMapperSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the mapper should add properties not from schema.
        /// </summary>
        public bool? AddPropertiesNotFromSchema { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mapper should add properties with null values.
        /// </summary>
        public bool? AddPropertiesWithNullValues { get; set; }

        /// <summary>
        /// Gets or sets optional property comparer for search property in schema.
        /// Default: <see cref="Metadata.PropertyComparer.ByNameOrAliasIgnoreCase"/>.
        /// </summary>
        public IEqualityComparer<IProperty>? PropertyComparer { get; set; }

        /// <summary>
        /// Gets or sets optional property factory.
        /// </summary>
        public IPropertyFactory? PropertyFactory { get; set; }

        /// <summary>
        /// Gets or sets optional property value factory.
        /// </summary>
        public IPropertyValueFactory? PropertyValueFactory { get; set; }

        /// <summary>
        /// Gets or sets optional value formatter if target property is string.
        /// </summary>
        public IValueFormatter? ValueFormatter { get; set; }

        /// <summary>
        /// Gets or sets action that will be invoked on mapping or validation error.
        /// </summary>
        public Action<Message>? LogMessage { get; set; }
    }
}

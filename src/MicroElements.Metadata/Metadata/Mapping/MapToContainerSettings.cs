﻿// Copyright (c) MicroElements. All rights reserved.
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
    public class MapToContainerSettings
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

    /// <summary>
    /// Settings for property container mapping.
    /// The same as <see cref="MapToContainerSettings"/> but with all fields initialized.
    /// </summary>
    public class MapToContainerContext
    {
        /// <summary>
        /// Gets a value indicating whether the mapper should add properties not from schema.
        /// </summary>
        public bool AddPropertiesNotFromSchema { get; }

        /// <summary>
        /// Gets a value indicating whether the mapper should add properties with null values.
        /// </summary>
        public bool AddPropertiesWithNullValues { get; }

        /// <summary>
        /// Gets optional property comparer for search property in schema.
        /// Default: <see cref="Metadata.PropertyComparer.ByNameOrAliasIgnoreCase"/>.
        /// </summary>
        public IEqualityComparer<IProperty> PropertyComparer { get; }

        /// <summary>
        /// Gets optional property factory.
        /// </summary>
        public IPropertyFactory PropertyFactory { get; }

        /// <summary>
        /// Gets optional property value factory.
        /// </summary>
        public IPropertyValueFactory PropertyValueFactory { get; }

        /// <summary>
        /// Gets optional value formatter if target property is string.
        /// </summary>
        public IValueFormatter ValueFormatter { get; }

        /// <summary>
        /// Gets action that will be invoked on mapping or validation error.
        /// </summary>
        public Action<Message> LogMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapToContainerContext"/> class.
        /// </summary>
        /// <param name="settings">Optional settings.</param>
        public MapToContainerContext(MapToContainerSettings? settings = null)
        {
            AddPropertiesNotFromSchema = settings?.AddPropertiesNotFromSchema ?? false;
            AddPropertiesWithNullValues = settings?.AddPropertiesWithNullValues ?? false;
            PropertyComparer = settings?.PropertyComparer ?? Metadata.PropertyComparer.ByNameOrAliasIgnoreCase;
            PropertyFactory = settings?.PropertyFactory ?? Metadata.PropertyFactory.Default;
            PropertyValueFactory = settings?.PropertyValueFactory ?? new CachedPropertyValueFactory(Metadata.PropertyValueFactory.Default, PropertyComparer);
            ValueFormatter = settings?.ValueFormatter ?? Formatter.SingleValueFormatter;
            LogMessage = settings?.LogMessage ?? (message => { /*empty*/ });
        }
    }
}

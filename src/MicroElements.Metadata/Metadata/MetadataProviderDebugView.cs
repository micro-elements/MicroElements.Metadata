// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Debug view that shows Metadata in debugger for Any type.
    /// </summary>
    public class MetadataProviderDebugView
    {
        private readonly object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderDebugView"/> class.
        /// </summary>
        /// <param name="value">Source value.</param>
        public MetadataProviderDebugView(object value) => _value = value;

        /// <summary>
        /// Gets object metadata.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer Metadata => _value.AsMetadataProvider().GetMetadataContainer(autoCreate: false);
    }

    /// <summary>
    /// Debug view that shows IPropertyContainer.
    /// </summary>
    public class PropertyContainerDebugView
    {
        private readonly IPropertyContainer _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerDebugView"/> class.
        /// </summary>
        /// <param name="value">Source value.</param>
        public PropertyContainerDebugView(object value)
        {
            _value = (value as IPropertyContainer) ?? PropertyContainer.Empty;
        }

        /// <summary>
        /// Gets properties with values.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IReadOnlyCollection<IPropertyValue> Properties => _value.Properties;

        /// <summary>
        /// Gets default search options for container.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public SearchOptions SearchOptions => _value.SearchOptions;

        /// <summary>
        /// Gets optional parent property source.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer? ParentSource => _value.ParentSource;

        /// <summary>
        /// Object metadata.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer Metadata => _value.AsMetadataProvider().GetMetadataContainer(autoCreate: false);
    }
}

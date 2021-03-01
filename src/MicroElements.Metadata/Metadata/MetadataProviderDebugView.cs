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
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderDebugView"/> class.
        /// </summary>
        /// <param name="value">Source value.</param>
        public MetadataProviderDebugView(object value)
        {
            Value = value;
        }

        private object Value { get; }

        /// <summary>
        /// Object metadata.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer Metadata => Value.AsMetadataProvider().GetMetadataContainer(autoCreate: false);
    }

    /// <summary>
    /// Debug view that shows IPropertyContainer.
    /// </summary>
    public class PropertyContainerDebugView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerDebugView"/> class.
        /// </summary>
        /// <param name="value">Source value.</param>
        public PropertyContainerDebugView(object value)
        {
            Value = (value as IPropertyContainer) ?? PropertyContainer.Empty;
        }

        private IPropertyContainer Value { get; }

        /// <summary>
        /// Gets properties with values.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IReadOnlyCollection<IPropertyValue> Properties => Value.Properties;

        /// <summary>
        /// Gets default search options for container.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public SearchOptions SearchOptions => Value.SearchOptions;

        /// <summary>
        /// Gets optional parent property source.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer? ParentSource => Value.ParentSource;

        /// <summary>
        /// Object metadata.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public IPropertyContainer Metadata => Value.AsMetadataProvider().GetMetadataContainer(autoCreate: false);
    }
}

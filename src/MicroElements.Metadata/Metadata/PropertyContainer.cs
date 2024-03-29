﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MicroElements.Metadata.Formatting;

namespace MicroElements.Metadata
{
    /// <summary>
    /// ReadOnly property container.
    /// </summary>
    [DebuggerTypeProxy(typeof(PropertyContainerDebugView))]
    public partial class PropertyContainer : IPropertyContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        /// <param name="sourceValues">Source property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            if (sourceValues == null)
            {
                Properties = Array.Empty<IPropertyValue>();
            }
            else
            {
                if (sourceValues is IPropertyContainer propertyContainer)
                    sourceValues = propertyContainer.Properties;

                bool isWritableCollection = sourceValues is ICollection<IPropertyValue> { IsReadOnly: false } || sourceValues is IList { IsReadOnly: false };

                if (sourceValues is IReadOnlyCollection<IPropertyValue> readOnlyCollection && !isWritableCollection)
                {
                    Properties = readOnlyCollection;
                }
                else
                {
                    // Protective copy because external collection can be changed outside
                    Properties = sourceValues.ToArray();
                }
            }

            ParentSource = parentPropertySource;
            SearchOptions = searchOptions ?? Search.Default;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties { get; }

        /// <inheritdoc />
        public IPropertyContainer? ParentSource { get; }

        /// <inheritdoc />
        public SearchOptions SearchOptions { get; }

        /// <inheritdoc />
        public int Count => Properties.Count;

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => Properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public override string ToString()
        {
            if (ReferenceEquals(this, Empty) || Count == 0)
                return EmptyName;

            return Formatter.FullRecursiveFormatter.TryFormat(Properties) ?? "FormatError";
        }

        /// <summary>
        /// Gets or calculates value for property.
        /// It's a shortcut for <see cref="SearchExtensions.GetValue{T}"/> for use in derived classes without keyword this.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <param name="defaultValue">Default value that returns if property value was not found.</param>
        /// <returns>The value for property.</returns>
        protected T? GetValue<T>(IProperty<T> property, SearchOptions? search = null, T? defaultValue = default)
        {
            return SearchExtensions.GetValue(this, property, search, defaultValue);
        }
    }

    /// <summary>
    /// Static PropertyContainer staff.
    /// </summary>
    public partial class PropertyContainer
    {
        internal const string EmptyName = "Empty";

        /// <summary>
        /// Empty property container singleton instance.
        /// </summary>
        public static readonly IPropertyContainer Empty = new PropertyContainer(searchOptions: SearchOptions.ExistingOnly);

        /// <summary>
        /// Merges <paramref name="propertyContainers"/>.
        /// </summary>
        /// <param name="mergeMode">Merge mode. Default: <see cref="PropertyAddMode.Set"/>.</param>
        /// <param name="propertyContainers">Containers to merge with initial.</param>
        /// <returns>New <see cref="IPropertyContainer"/> instance.</returns>
        public static IPropertyContainer Merge(PropertyAddMode mergeMode, params IPropertyContainer?[]? propertyContainers)
        {
            return propertyContainers.Merge(mergeMode);
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata
{
    public class HierarchicalContainer2 : IPropertyContainer
    {
        private readonly IPropertyContainer _propertyContainer;
        private readonly IPropertyContainer? _parent;

        public HierarchicalContainer2(IPropertyContainer propertyContainer, IPropertyContainer? parent)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            _propertyContainer = PropertyContainer.CreateHierarchicalContainer(propertyContainer, parent);
        }

        /// <inheritdoc />
        public override string ToString() => _propertyContainer.ToString();

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.Properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyContainer.Count;

        /// <inheritdoc />
        public IPropertyContainer? ParentSource => _propertyContainer.ParentSource;

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer.Properties;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer.SearchOptions;
    }

    public partial class PropertyContainer
    {
        public static IPropertyContainer CreateHierarchicalContainer(
            IPropertyContainer propertyContainer,
            IPropertyContainer? parent,
            bool mergeHierarchy = true)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            static bool HasParent(IPropertyContainer? container) => container?.ParentSource is { Count: > 0 };

            if (mergeHierarchy && (HasParent(propertyContainer) || HasParent(parent)))
            {
                var hierarchy1 = propertyContainer.GetHierarchy().Reverse();
                var hierarchy2 = parent?.GetHierarchy().Reverse();
                var hierarchy = hierarchy1.Concat(hierarchy2 ?? Array.Empty<IPropertyContainer>()).ToArray();

                IPropertyContainer last = hierarchy[^1];
                for (int i = hierarchy.Length - 2; i >= 0; i--)
                {
                    last = new PropertyContainerWithParent(hierarchy[i], last);
                }

                return last;
            }

            return new PropertyContainerWithParent(propertyContainer, parent);
        }
    }

    /// <summary>
    /// PropertyContainer with parent wrapper.
    /// </summary>
    public class PropertyContainerWithParent : IPropertyContainer
    {
        private readonly IPropertyContainer _propertyContainer;
        private readonly IPropertyContainer? _parent;

        public PropertyContainerWithParent(IPropertyContainer propertyContainer, IPropertyContainer? parent = null)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));

            _propertyContainer = propertyContainer;
            _parent = parent;
        }

        /// <inheritdoc />
        public override string ToString() => _parent != null ? $"{_propertyContainer} -> {_parent}" : $"{_propertyContainer}";

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer.Properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyContainer.Count;

        /// <inheritdoc />
        public IPropertyContainer? ParentSource => _parent;

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer.Properties;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer.SearchOptions;
    }
}

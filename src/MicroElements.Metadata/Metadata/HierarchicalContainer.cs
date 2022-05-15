// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata
{
    public class HierarchicalContainer : IPropertyContainer
    {
        private readonly IPropertyContainer _propertyContainer;

        public HierarchicalContainer(IPropertyContainer propertyContainer1, IPropertyContainer? propertyContainer2)
        {
            propertyContainer1.AssertArgumentNotNull(nameof(propertyContainer1));

            _propertyContainer = PropertyContainer.CreateHierarchicalContainer(propertyContainer1, propertyContainer2);
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
            IPropertyContainer propertyContainer1,
            IPropertyContainer? propertyContainer2,
            bool mergeHierarchy = true)
        {
            propertyContainer1.AssertArgumentNotNull(nameof(propertyContainer1));

            static bool HasParent(IPropertyContainer? container) => container?.ParentSource != null && container.ParentSource.Count > 0;

            if (mergeHierarchy && (HasParent(propertyContainer1) || HasParent(propertyContainer2)))
            {
                var hierarchy1 = propertyContainer1.GetHierarchy().Reverse();
                var hierarchy2 = propertyContainer2?.GetHierarchy().Reverse();
                var hierarchy = hierarchy1.Concat(hierarchy2 ?? Array.Empty<IPropertyContainer>()).ToArray();

                IPropertyContainer last = hierarchy[hierarchy.Length - 1];
                for (int i = hierarchy.Length - 2; i >= 0; i--)
                {
                    last = new PropertyContainerPair(hierarchy[i], last);
                }

                return new PropertyContainerPair(last, null);
            }

            return new PropertyContainerPair(propertyContainer1, propertyContainer2);
        }
    }

    public class PropertyContainerPair : IPropertyContainer
    {
        private readonly IPropertyContainer _propertyContainer1;
        private readonly IPropertyContainer? _propertyContainer2;

        public PropertyContainerPair(IPropertyContainer propertyContainer1, IPropertyContainer? propertyContainer2 = null)
        {
            propertyContainer1.AssertArgumentNotNull(nameof(propertyContainer1));

            _propertyContainer1 = propertyContainer1;
            _propertyContainer2 = propertyContainer2;
        }

        /// <inheritdoc />
        public override string ToString() => _propertyContainer2 != null ? $"{_propertyContainer1} -> {_propertyContainer2}" : $"{_propertyContainer1}";

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => _propertyContainer1.Properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyContainer1.Count;

        /// <inheritdoc />
        public IPropertyContainer? ParentSource => _propertyContainer2;

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => _propertyContainer1.Properties;

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer1.SearchOptions;
    }
}

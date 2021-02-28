// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides <see cref="IPropertyValueFactory"/> for each property IEqualityComparer.
    /// Useful for <see cref="CachedPropertyValueFactory"/> because it is comparer dependent.
    /// </summary>
    public interface IPropertyValueFactoryProvider
    {
        /// <summary>
        /// Gets <see cref="IPropertyValueFactory"/> for <paramref name="propertyComparer"/>.
        /// </summary>
        /// <param name="propertyComparer">Property comparer.</param>
        /// <returns><see cref="IPropertyValueFactory"/> instance.</returns>
        IPropertyValueFactory GetFactory(IEqualityComparer<IProperty> propertyComparer);
    }

    public class PropertyValueFactoryProvider : IPropertyValueFactoryProvider
    {
        private readonly ConcurrentDictionary<IEqualityComparer<IProperty>, IPropertyValueFactory> _factories = new ();

        private readonly Func<IEqualityComparer<IProperty>, IPropertyValueFactory> _factoryFactory;

        public PropertyValueFactoryProvider(Func<IEqualityComparer<IProperty>, IPropertyValueFactory> factoryFactory)
        {
            _factoryFactory = factoryFactory;
        }

        /// <inheritdoc />
        public IPropertyValueFactory GetFactory(IEqualityComparer<IProperty> propertyComparer)
        {
            return _factories.GetOrAdd(propertyComparer, comparer => _factoryFactory(comparer));
        }
    }
}

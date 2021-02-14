// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents <see cref="IPropertyValue"/> factory.
    /// </summary>
    public interface IPropertyValueFactory
    {
        /// <summary>
        /// Creates new instance if <see cref="IPropertyValue"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Property.</param>
        /// <param name="value">Value for property.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>Created property value.</returns>
        IPropertyValue<T> Create<T>(IProperty<T> property, T? value, ValueSource? valueSource = null);

        /// <summary>
        /// Creates new instance if <see cref="IPropertyValue"/>.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="value">Value for property.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>Created property value.</returns>
        IPropertyValue CreateUntyped(IProperty property, object? value, ValueSource? valueSource = null);
    }

    public interface IPropertyValueFactoryProvider
    {
        IPropertyValueFactory GetFactory(IEqualityComparer<IProperty> propertyComparer);
    }

    public class PropertyValueFactoryProvider : IPropertyValueFactoryProvider
    {
        ConcurrentDictionary<IEqualityComparer<IProperty>, IPropertyValueFactory> _factories = new ConcurrentDictionary<IEqualityComparer<IProperty>, IPropertyValueFactory>();

        private Func<IEqualityComparer<IProperty>, IPropertyValueFactory> _factoryFactory;

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

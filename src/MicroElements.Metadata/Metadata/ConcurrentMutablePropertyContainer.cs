// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that contains properties and values for these properties.
    /// </summary>
    public class ConcurrentMutablePropertyContainer : IMutablePropertyContainer
    {
        private readonly object _syncRoot = new object();
        private readonly MutablePropertyContainer _propertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMutablePropertyContainer"/> class.
        /// </summary>
        /// <param name="sourceValues">Property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public ConcurrentMutablePropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
        {
            _propertyContainer = new MutablePropertyContainer(sourceValues, parentPropertySource, searchOptions);
        }

        /// <inheritdoc />
        public override string ToString() => DoOnLock(() => _propertyContainer.ToString());

        #region IPropertyContainer

        /// <inheritdoc />
        public IPropertyContainer? ParentSource => _propertyContainer.ParentSource;

        /// <inheritdoc />
        public IReadOnlyCollection<IPropertyValue> Properties => DoOnLock(() => _propertyContainer.Properties.ToList());

        /// <inheritdoc />
        public SearchOptions SearchOptions => _propertyContainer.SearchOptions;

        #endregion

        #region IReadOnlyCollection

        /// <inheritdoc />
        public IEnumerator<IPropertyValue> GetEnumerator() => Properties.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => DoOnLock(() => _propertyContainer.Count);

        #endregion

        #region Mutability

        /// <inheritdoc />
        public IPropertyValue<T> SetValue<T>(IProperty<T> property, [AllowNull] T value, ValueSource? valueSource = default) =>
            DoOnLock(() => _propertyContainer.SetValue(property, value, valueSource));

        /// <inheritdoc />
        public void SetValue(IPropertyValue propertyValue) =>
            DoOnLock(() => _propertyContainer.SetValue(propertyValue));

        /// <inheritdoc />
        public void Add(IPropertyValue propertyValue) =>
            DoOnLock(() => _propertyContainer.Add(propertyValue));

        /// <inheritdoc />
        public IPropertyValue<T>? RemoveValue<T>(IProperty<T> property) =>
            DoOnLock(() => _propertyContainer.RemoveValue(property));

        /// <inheritdoc />
        public void Clear() =>
            DoOnLock(() => _propertyContainer.Clear());

        #endregion

        /// <summary>
        /// Gets or calculates value for property.
        /// For use in derived classes.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Property to find.</param>
        /// <param name="search">Search options.</param>
        /// <returns>The value for property.</returns>
        [return: MaybeNull]
        protected T GetValue<T>(IProperty<T> property, SearchOptions? search = null)
        {
            return SearchExtensions.GetValue(this, property, search);
        }

        private T DoOnLock<T>(Func<T> action)
        {
            lock (_syncRoot)
            {
                return action();
            }
        }

        private void DoOnLock(Action action)
        {
            lock (_syncRoot)
            {
                action();
            }
        }
    }
}

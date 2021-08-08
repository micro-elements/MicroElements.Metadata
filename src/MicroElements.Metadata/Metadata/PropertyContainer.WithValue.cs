// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// WithValue extensions for ReadOnly containers.
    /// </summary>
    public static partial class PropertyContainerExtensions
    {
        /// <summary>
        /// Returns new container with new property value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <param name="propertyAddMode">Property add mode. Default: PropertyAddMode.Set.</param>
        /// <returns>New container with desired property value.</returns>
        [Pure]
        public static PropertyContainer<TSchema> WithValue<TSchema, TValue>(
            this PropertyContainer<TSchema> propertyContainer,
            IProperty<TValue> property,
            TValue value,
            ValueSource? valueSource = default,
            PropertyAddMode propertyAddMode = PropertyAddMode.Set)
            where TSchema : IPropertySet, new()
        {
            var propertyValue = new PropertyValue<TValue>(property, value, valueSource);
            return propertyContainer.WithValue(propertyValue, propertyAddMode);
        }

        /// <summary>
        /// Returns new container with new property value.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <param name="propertyAddMode">Property add mode. Default: PropertyAddMode.Set.</param>
        /// <returns>New container with desired property value.</returns>
        [Pure]
        public static PropertyContainer<TSchema> WithValue<TSchema>(
            this PropertyContainer<TSchema> propertyContainer,
            IPropertyValue propertyValue,
            PropertyAddMode propertyAddMode = PropertyAddMode.Set)
            where TSchema : IPropertySet, new()
        {
            var propertyValues = propertyContainer.Properties.WithValue(propertyValue, propertyContainer.SearchOptions, propertyAddMode);

            return new PropertyContainer<TSchema>(
                sourceValues: propertyValues,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

        /// <summary>
        /// Returns new container with new property value.
        /// </summary>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <param name="propertyAddMode">Property add mode. Default: PropertyAddMode.Set.</param>
        /// <returns>New container with desired property value.</returns>
        [Pure]
        public static IPropertyContainer WithValue<TValue>(
            this IPropertyContainer propertyContainer,
            IProperty<TValue> property,
            TValue value,
            ValueSource? valueSource = default,
            PropertyAddMode propertyAddMode = PropertyAddMode.Set)
        {
            var propertyValue = new PropertyValue<TValue>(property, value, valueSource);
            return propertyContainer.WithValue(propertyValue, propertyAddMode);
        }

        /// <summary>
        /// Returns new container with new property value.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <param name="propertyAddMode">Property add mode. Default: PropertyAddMode.Set.</param>
        /// <returns>New container with desired property value.</returns>
        [Pure]
        public static IPropertyContainer WithValue(
            this IPropertyContainer propertyContainer,
            IPropertyValue propertyValue,
            PropertyAddMode propertyAddMode = PropertyAddMode.Set)
        {
            var propertyValues = propertyContainer.Properties.WithValue(propertyValue, propertyContainer.SearchOptions, propertyAddMode);

            return new PropertyContainer(
                sourceValues: propertyValues,
                parentPropertySource: propertyContainer.ParentSource,
                searchOptions: propertyContainer.SearchOptions);
        }

        /// <summary>
        /// Returns new property value enumeration with desired property value.
        /// </summary>
        /// <param name="propertyValues">Source property values.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <param name="searchOptions">Search options for replace scenarios.</param>
        /// <param name="propertyAddMode">Property add mode. Default: PropertyAddMode.Set.</param>
        /// <returns>New property value enumeration with desired property value.</returns>
        [Pure]
        public static IEnumerable<IPropertyValue> WithValue(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyValue propertyValue,
            SearchOptions searchOptions,
            PropertyAddMode propertyAddMode = PropertyAddMode.Set)
        {
            IEnumerable<IPropertyValue> result = Enumerable.Empty<IPropertyValue>();

            switch (propertyAddMode)
            {
                case PropertyAddMode.Add:
                    result = propertyValues.AddValue(propertyValue);
                    break;
                case PropertyAddMode.Set:
                    result = propertyValues.SetValue(propertyValue, searchOptions);
                    break;
                case PropertyAddMode.SetNotExisting:
                    result = propertyValues.SetNotExisting(propertyValue, searchOptions);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns new property value enumeration with desired property value added to end.
        /// </summary>
        /// <param name="propertyValues">Source property values.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <returns>New property value enumeration with desired property value added to end.</returns>
        [Pure]
        public static IEnumerable<IPropertyValue> AddValue(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyValue propertyValue)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            return propertyValues.Append(propertyValue);
        }

        /// <summary>
        /// Returns new property value enumeration with desired property value that replaces existing.
        /// </summary>
        /// <param name="propertyValues">Source property values.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <param name="searchOptions">Search options that uses to compare properties.</param>
        /// <returns>New property value enumeration with desired property value that replaces existing.</returns>
        [Pure]
        public static IEnumerable<IPropertyValue> SetValue(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyValue propertyValue,
            SearchOptions searchOptions)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            bool isSet = false;
            foreach (IPropertyValue existingPropertyValue in propertyValues)
            {
                if (searchOptions.PropertyComparer.Equals(existingPropertyValue.PropertyUntyped, propertyValue.PropertyUntyped))
                {
                    isSet = true;
                    yield return propertyValue;
                }
                else
                {
                    yield return existingPropertyValue;
                }
            }

            if (!isSet)
                yield return propertyValue;
        }

        /// <summary>
        /// Returns new property value enumeration. Sets property value if property is not set already.
        /// </summary>
        /// <param name="propertyValues">Source property values.</param>
        /// <param name="propertyValue">Property to set.</param>
        /// <param name="searchOptions">Search options that uses to compare properties.</param>
        /// <returns>New property value enumeration.</returns>
        [Pure]
        public static IEnumerable<IPropertyValue> SetNotExisting(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyValue propertyValue,
            SearchOptions searchOptions)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            bool exists = false;
            foreach (IPropertyValue existingPropertyValue in propertyValues)
            {
                if (!exists && searchOptions.PropertyComparer.Equals(existingPropertyValue.PropertyUntyped, propertyValue.PropertyUntyped))
                {
                    exists = true;
                }

                yield return existingPropertyValue;
            }

            if (!exists)
                yield return propertyValue;
        }
    }
}

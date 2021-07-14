// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    public static class WithValueExtensions
    {
        public static PropertyContainer<TSchema> WithValue<TSchema, TValue>(this PropertyContainer<TSchema> source, IProperty<TValue> property, TValue value, ValueSource? valueSource = default, PropertyAddMode propertyAddMode = PropertyAddMode.Set)
            where TSchema : IPropertySet, new()
        {
            var propertyValue = new PropertyValue<TValue>(property, value, valueSource);
            var propertyValues = source.Properties.WithValue(propertyValue, source.SearchOptions, propertyAddMode);

            return new PropertyContainer<TSchema>(
                sourceValues: propertyValues,
                parentPropertySource: source.ParentSource,
                searchOptions: source.SearchOptions);
        }

        public static PropertyContainer<TSchema> WithValue<TSchema>(this PropertyContainer<TSchema> source, IPropertyValue propertyValue, PropertyAddMode propertyAddMode = PropertyAddMode.Set)
            where TSchema : IPropertySet, new()
        {
            var propertyValues = source.Properties.WithValue(propertyValue, source.SearchOptions, propertyAddMode);

            return new PropertyContainer<TSchema>(
                sourceValues: propertyValues,
                parentPropertySource: source.ParentSource,
                searchOptions: source.SearchOptions);
        }

        public static IPropertyContainer WithValue(this IPropertyContainer source, IPropertyValue propertyValue, PropertyAddMode propertyAddMode = PropertyAddMode.Set)
        {
            var propertyValues = source.Properties.WithValue(propertyValue, source.SearchOptions, propertyAddMode);

            return new PropertyContainer(
                sourceValues: propertyValues,
                parentPropertySource: source.ParentSource,
                searchOptions: source.SearchOptions);
        }

        public static IEnumerable<IPropertyValue> WithValue(
            this IEnumerable<IPropertyValue> propertyValues,
            IPropertyValue propertyValue,
            SearchOptions searchOptions,
            PropertyAddMode propertyAddMode)
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

        public static IEnumerable<IPropertyValue> AddValue(this IEnumerable<IPropertyValue> propertyValues, IPropertyValue propertyValue)
        {
            propertyValue.AssertArgumentNotNull(nameof(propertyValue));

            return propertyValues.Append(propertyValue);
        }

        public static IEnumerable<IPropertyValue> SetValue(this IEnumerable<IPropertyValue> propertyValues, IPropertyValue propertyValue, SearchOptions searchOptions)
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

        public static IEnumerable<IPropertyValue> SetNotExisting(this IEnumerable<IPropertyValue> propertyValues, IPropertyValue propertyValue, SearchOptions searchOptions)
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

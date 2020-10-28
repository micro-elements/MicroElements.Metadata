﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default search algorithm.
    /// </summary>
    public class DefaultSearchAlgorithm : ISearchAlgorithm
    {
        /// <summary>
        /// Static instance of default search algorithm.
        /// </summary>
        public static readonly ISearchAlgorithm Instance = new DefaultSearchAlgorithm();

        /// <inheritdoc />
        public IPropertyValue? SearchPropertyValueUntyped(
            IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? searchOptions = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            SearchOptions search = searchOptions ?? propertyContainer.SearchOptions;

            // Search property by EqualityComparer
            IPropertyValue? propertyValue = propertyContainer
                .FirstOrDefault(pv => search.PropertyComparer.Equals(pv.PropertyUntyped, property));

            // Good try! Return
            if (propertyValue != null)
                return propertyValue;

            // Search in parent if needed
            if (search.SearchInParent && propertyContainer.ParentSource != null)
            {
                propertyValue = SearchPropertyValueUntyped(propertyContainer.ParentSource, property, search);

                if (propertyValue != null)
                    return propertyValue;
            }

            return search.ReturnNotDefined ? PropertyValue.Create(property, property.Type.GetDefaultValue(), ValueSource.NotDefined) : null;
        }

        /// <inheritdoc />
        public IPropertyValue<T>? GetPropertyValue<T>(
            IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? searchOptions = default)
        {
            propertyContainer.AssertArgumentNotNull(nameof(propertyContainer));
            property.AssertArgumentNotNull(nameof(property));

            SearchOptions search = searchOptions ?? propertyContainer.SearchOptions;

            // Base search by ByReferenceComparer.
            IPropertyValue? propertyValue = SearchPropertyValueUntyped(propertyContainer, property, search
                .WithPropertyComparer(PropertyComparer.ByReferenceComparer)
                .UseDefaultValue(false)
                .ReturnNull());

            // Good job - return result!
            if (propertyValue != null)
                return (IPropertyValue<T>)propertyValue;

            // Property can be calculated.
            if (search.CalculateValue && property.Calculator != null)
            {
                var calculationResult = property.Calculator.Calculate(propertyContainer);
                var calculatedValue = new PropertyValue<T>(property, calculationResult.Value, calculationResult.ValueSource);

                if (calculatedValue.Source == ValueSource.NotDefined && !search.ReturnNotDefined)
                    calculatedValue = null;

                return calculatedValue;
            }

            // Search by provided options.
            propertyValue = SearchPropertyValueUntyped(propertyContainer, property, search
                .UseDefaultValue(false)
                .ReturnNull());

            // Nice. We have result!
            if (propertyValue != null)
                return (IPropertyValue<T>)propertyValue;

            // Maybe default value?
            if (search.UseDefaultValue)
                return new PropertyValue<T>(property, property.DefaultValue(), ValueSource.DefaultValue);

            // Return null or NotDefined
            return search.ReturnNotDefined ? new PropertyValue<T>(property, default, ValueSource.NotDefined) : null;
        }
    }
}
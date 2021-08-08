// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Reflection;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Default search algorithm.
    /// </summary>
    public sealed class DefaultSearchAlgorithm : ISearchAlgorithm
    {
        /// <summary>
        /// Static instance of default search algorithm.
        /// </summary>
        public static readonly ISearchAlgorithm Instance = new DefaultSearchAlgorithm();

        private readonly IPropertyValueFactory _propertyValueFactory = PropertyValueFactory.Default;
        private IPropertyValueFactoryProvider _factoryProvider = new PropertyValueFactoryProvider(comparer => PropertyValueFactory.Default);

        /// <inheritdoc />
        public IPropertyValue? SearchPropertyValueUntyped(
            IPropertyContainer propertyContainer,
            IProperty property,
            SearchOptions? searchOptions = null)
        {
            if (propertyContainer == null)
                throw new ArgumentNullException(nameof(propertyContainer));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            SearchOptions search = searchOptions ?? propertyContainer.SearchOptions;

            // Search property by EqualityComparer
            IPropertyValue? propertyValue = null;
            var properties = propertyContainer.Properties;
            if (properties is IList<IPropertyValue> propertyValues)
            {
                // For is for performance reason here
                for (int i = 0; i < propertyValues.Count; i++)
                {
                    if (search.PropertyComparer.Equals(propertyValues[i].PropertyUntyped, property))
                    {
                        propertyValue = propertyValues[i];
                        break;
                    }
                }
            }
            else
            {
                foreach (IPropertyValue? pv in propertyContainer.Properties)
                {
                    if (search.PropertyComparer.Equals(pv.PropertyUntyped, property))
                    {
                        propertyValue = pv;
                        break;
                    }
                }
            }

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

        private static readonly SearchOptions _fastSearchOptions = default(SearchOptions)
            .WithPropertyComparer(PropertyComparer.ByReferenceComparer)
            .UseDefaultValue(false)
            .ReturnNull();

        /// <inheritdoc />
        public IPropertyValue<T>? GetPropertyValue<T>(
            IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? searchOptions = default)
        {
            if (propertyContainer == null)
                throw new ArgumentNullException(nameof(propertyContainer));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            SearchOptions search = searchOptions ?? propertyContainer.SearchOptions;

            // Base search by ByReferenceComparer.
            IPropertyValue? propertyValue = SearchPropertyValueUntyped(propertyContainer, property, _fastSearchOptions);

            // Good job - return result!
            if (propertyValue != null)
                return (IPropertyValue<T>)propertyValue;

            // Property can be calculated.
            if (search.CalculateValue && property.GetCalculator() is { } calculator)
            {
                var calculationResult = calculator.Calculate(propertyContainer, search);
                var calculatedValue = _propertyValueFactory.Create(property, calculationResult.Value, calculationResult.ValueSource);

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
            if (search.UseDefaultValue && property.DefaultValue.IsDefaultValueAllowed)
                return _propertyValueFactory.Create(property, property.DefaultValue.Value, ValueSource.DefaultValue);

            // Return null or NotDefined
            return search.ReturnNotDefined ? _propertyValueFactory.Create(property, default, ValueSource.NotDefined) : null;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Reflection.TypeExtensions;

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

        // TODO: Вынести IPropertyValueFactory в SearchOptions?
        // TODO: IPropertyValue возвращать из Calculator?
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

            if (propertyContainer is IIndexedPropertyContainer indexed)
            {
                propertyValue = indexed.GetPropertyValue(property);
            }
            else
            {
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
                var calculationContext = new CalculationContext(propertyContainer, search);
                var calculatedValue = calculator.Calculate(ref calculationContext);
                var calculatedValueSource = calculationContext.ValueSource ?? ValueSource.NotDefined;
                IPropertyValue<T>? calculatedPropertyValue = _propertyValueFactory.Create(property, calculatedValue, calculatedValueSource);

                if (calculatedPropertyValue.Source == ValueSource.NotDefined && !search.ReturnNotDefined)
                    calculatedPropertyValue = null;

                return calculatedPropertyValue;
            }

            // Search by provided options.
            propertyValue = SearchPropertyValueUntyped(propertyContainer, property, search
                .UseDefaultValue(false)
                .ReturnNull());

            // Nice. We have result!
            if (propertyValue != null)
                return (IPropertyValue<T>)propertyValue;

            // Maybe default value?
            if (search.UseDefaultValue && property.DefaultValue is { } defaultValue)
                return _propertyValueFactory.Create(property, defaultValue.Value, ValueSource.DefaultValue);

            // Return null or NotDefined
            return search.ReturnNotDefined ? _propertyValueFactory.Create(property, default, ValueSource.NotDefined) : null;
        }

        /// <inheritdoc />
        public void GetPropertyValue2<T>(
            IPropertyContainer propertyContainer,
            IProperty<T> property,
            SearchOptions? searchOptions,
            out PropertyValueData<T> result)
        {
            SearchOptions search = searchOptions ?? propertyContainer.SearchOptions;

            // Base search by ByReferenceComparer.
            IPropertyValue<T>? propertyValue = SearchPropertyValueUntyped(propertyContainer, property, _fastSearchOptions) as IPropertyValue<T>;

            // Good job - return result!
            if (propertyValue != null)
            {
                result = new PropertyValueData<T>(property, propertyValue.Value, propertyValue.Source);
                return;
            }

            // Property can be calculated.
            if (search.CalculateValue && property.GetCalculator() is { } calculator)
            {
                var calculationContext = new CalculationContext(propertyContainer, search);
                var calculatedValue = calculator.Calculate(ref calculationContext);
                var calculatedValueSource = calculationContext.ValueSource ?? ValueSource.NotDefined;

                result = new PropertyValueData<T>(property, calculatedValue, calculatedValueSource);
                return;
            }

            // Search by provided options.
            propertyValue = SearchPropertyValueUntyped(propertyContainer, property, search
                .UseDefaultValue(false)
                .ReturnNull()) as IPropertyValue<T>;

            // Nice. We have result!
            if (propertyValue != null)
            {
                result = new PropertyValueData<T>(property, propertyValue.Value, propertyValue.Source);
                return;
            }

            // Maybe default value?
            if (search.UseDefaultValue && property.DefaultValue is { } defaultValue)
            {
                result = new PropertyValueData<T>(property, defaultValue.Value, ValueSource.DefaultValue);
                return;
            }

            // Return null or NotDefined
            result = new PropertyValueData<T>(property, default, ValueSource.NotDefined);
        }
    }
}

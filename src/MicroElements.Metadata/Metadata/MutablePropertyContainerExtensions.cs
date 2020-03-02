using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Builder extensions.
    /// </summary>
    public static class MutablePropertyContainerExtensions
    {
        /// <summary>
        /// Sets parent property source and returns the same changed propertyContainer.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <returns>The same container with changed parent.</returns>
        public static IMutablePropertyContainer WithParentPropertySource(this IMutablePropertyContainer propertyContainer, IPropertyContainer parentPropertySource)
        {
            propertyContainer.SetParentPropertySource(parentPropertySource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value, ValueSource valueSource = default)
        {
            propertyContainer.SetValue(property, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets value by string property name and returns the same container.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValue<T>(this IMutablePropertyContainer propertyContainer, string propertyName, T value, ValueSource valueSource = default)
        {
            propertyContainer.SetValue(propertyName, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValueUntyped(this IMutablePropertyContainer propertyContainer, IProperty property, object value, ValueSource valueSource = default)
        {
            propertyContainer.SetValueUntyped(property, value, valueSource);
            return propertyContainer;
        }

        /// <summary>
        /// Sets value by string property name.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public static IPropertyValue<T> SetValue<T>(this IMutablePropertyContainer propertyContainer, string propertyName, T value, ValueSource valueSource = default)
        {
            Type valueType = typeof(T);
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(Search.ByNameOrAlias(propertyName).SearchInParent());

            if (propertyValue != null)
            {
                IProperty existingProperty = propertyValue.PropertyUntyped;
                if (existingProperty.Type != valueType)
                {
                    throw new ArgumentException($"Existing property {existingProperty.Name} has type {existingProperty.Type} but value has type {valueType}");
                }

                return propertyContainer.SetValue((IProperty<T>)existingProperty, value, valueSource);
            }

            return propertyContainer.SetValue(new Property<T>(propertyName), value, valueSource);
        }

        /// <summary>
        /// Sets value by string property name.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValue(this IMutablePropertyContainer propertyContainer, string propertyName, object value, ValueSource valueSource = default)
        {
            Type valueType = value.GetType();
            IPropertyValue propertyValue = propertyContainer.GetPropertyValueUntyped(Search.ByNameOrAlias(propertyName).SearchInParent());

            if (propertyValue != null)
            {
                IProperty existingProperty = propertyValue.PropertyUntyped;
                if (existingProperty.Type != valueType)
                {
                    throw new ArgumentException($"Existing property {existingProperty.Name} has type {existingProperty.Type} but value has type {valueType}");
                }

                return propertyContainer.SetValueUntyped(existingProperty, value, valueSource);
            }

            return propertyContainer.SetValueUntyped(Property.Create(valueType, propertyName), value, valueSource);
        }

        /// <summary>
        /// Sets property and value (non generic version).
        /// </summary>
        /// <param name="propertyContainer">PropertyContainer to change.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="valueSource">Value source.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValueUntyped(this IMutablePropertyContainer propertyContainer, IProperty property, object value, ValueSource valueSource = default)
        {
            IPropertyValue propertyValue = PropertyValue.Create(property, value, valueSource);
            propertyContainer.SetValue(propertyValue);
            return propertyValue;
        }

        /// <summary>
        /// Adds property values.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="propertyValues">PropertyValue list.</param>
        public static void AddRange(this IMutablePropertyContainer propertyContainer, IEnumerable<IPropertyValue> propertyValues)
        {
            foreach (IPropertyValue propertyValue in propertyValues)
            {
                propertyContainer.Add(propertyValue);
            }
        }
    }
}

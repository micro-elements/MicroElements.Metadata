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
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValue<T>(this IMutablePropertyContainer propertyContainer, IProperty<T> property, T value)
        {
            propertyContainer.SetValue(property, value);
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
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValue<T>(this IMutablePropertyContainer propertyContainer, string propertyName, T value)
        {
            propertyContainer.SetValue(propertyName, value);
            return propertyContainer;
        }

        /// <summary>
        /// Sets property value and returns the same container.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>The same container with changed property.</returns>
        public static IMutablePropertyContainer WithValue(this IMutablePropertyContainer propertyContainer, IProperty property, object value)
        {
            propertyContainer.SetValue(property, value);
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
        /// <returns><see cref="IPropertyValue{T}"/> that holds value for property.</returns>
        public static IPropertyValue<T> SetValue<T>(this IMutablePropertyContainer propertyContainer, string propertyName, T value)
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

                return propertyContainer.SetValue((IProperty<T>)existingProperty, value);
            }

            return propertyContainer.SetValue(new Property<T>(propertyName), value);
        }

        /// <summary>
        /// Sets value by string property name.
        /// Overrides property value if exists with the same <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyContainer">MutablePropertyContainer.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="value">Value to set.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValue(this IMutablePropertyContainer propertyContainer, string propertyName, object value)
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

                return propertyContainer.SetValue(existingProperty, value);
            }

            return propertyContainer.SetValue(Property.Create(valueType, propertyName), value);
        }

        /// <summary>
        /// Sets property and value (non generic version).
        /// </summary>
        /// <param name="propertyContainer">PropertyContainer to change.</param>
        /// <param name="property">Property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns><see cref="IPropertyValue"/> that holds value for property.</returns>
        public static IPropertyValue SetValue(this IMutablePropertyContainer propertyContainer, IProperty property, object value)
        {
            IPropertyValue propertyValue = PropertyValue.Create(property, value);
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

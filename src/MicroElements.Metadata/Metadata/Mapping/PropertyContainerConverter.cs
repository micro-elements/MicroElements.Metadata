// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Mapping
{
    /// <summary>
    /// PropertyContainer convert extensions.
    /// </summary>
    public static class PropertyContainerConverter
    {
        /// <summary>
        /// Converts <see cref="IPropertyContainer"/> to <see cref="IPropertyContainer"/> of type <typeparamref name="TPropertyContainer"/>.
        /// </summary>
        /// <typeparam name="TPropertyContainer">PropertyContainer type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="returnTheSameIfNoNeedToConvert">Returns the same instance if source container is already matches <typeparamref name="TPropertyContainer"/>.</param>
        /// <returns>Result property container.</returns>
        public static TPropertyContainer ToPropertyContainerOfType<TPropertyContainer>(this IPropertyContainer propertyContainer, bool returnTheSameIfNoNeedToConvert = true)
            where TPropertyContainer : IPropertyContainer
        {
            return (TPropertyContainer)ToPropertyContainerOfType(propertyContainer, typeof(TPropertyContainer), returnTheSameIfNoNeedToConvert);
        }

        /// <summary>
        /// Converts <see cref="IPropertyContainer"/> to <see cref="IPropertyContainer"/> of type <paramref name="outputType"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="outputType">Result type.</param>
        /// <param name="returnTheSameIfNoNeedToConvert">Returns the same instance if source container is already matches <paramref name="outputType"/>.</param>
        /// <returns>Result property container.</returns>
        public static IPropertyContainer ToPropertyContainerOfType(this IPropertyContainer propertyContainer, Type outputType, bool returnTheSameIfNoNeedToConvert = true)
        {
            if (returnTheSameIfNoNeedToConvert && propertyContainer.IsAssignableTo(outputType))
                return propertyContainer;

            if (outputType == typeof(IMutablePropertyContainer) || outputType == typeof(MutablePropertyContainer))
                return new MutablePropertyContainer(propertyContainer.Properties, propertyContainer.ParentSource, propertyContainer.SearchOptions);

            if (outputType == typeof(IPropertyContainer) || outputType == typeof(PropertyContainer))
                return new PropertyContainer(propertyContainer.Properties, propertyContainer.ParentSource, propertyContainer.SearchOptions);

            if (outputType.IsConcreteType())
            {
                /*
                 *  public PropertyContainer(
                        IEnumerable<IPropertyValue>? sourceValues = null,
                        IPropertyContainer? parentPropertySource = null,
                        SearchOptions? searchOptions = null)
                 */
                object?[] ctorArgs = { propertyContainer.Properties, propertyContainer.ParentSource, propertyContainer.SearchOptions };
                IPropertyContainer resultContainer = (IPropertyContainer)Activator.CreateInstance(outputType, args: ctorArgs);
                return resultContainer;
            }

            // Return ReadOnly PropertyContainer as a result.
            return new PropertyContainer(propertyContainer.Properties, propertyContainer.ParentSource, propertyContainer.SearchOptions);
        }
    }
}

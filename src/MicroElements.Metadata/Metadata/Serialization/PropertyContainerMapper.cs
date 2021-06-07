// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Mapping extensions.
    /// </summary>
    public static class PropertyContainerMapper
    {
        /// <summary>
        /// Converts <see cref="IPropertyContainer"/> to <see cref="IPropertyContainer"/> of type <typeparamref name="TPropertyContainer"/>.
        /// </summary>
        /// <typeparam name="TPropertyContainer">PropertyContainer type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <returns>Result property container.</returns>
        public static TPropertyContainer ToPropertyContainerOfType<TPropertyContainer>(this IPropertyContainer propertyContainer)
            where TPropertyContainer : IPropertyContainer
        {
            return (TPropertyContainer)ToPropertyContainerOfType(propertyContainer, typeof(TPropertyContainer));
        }

        /// <summary>
        /// Converts <see cref="IPropertyContainer"/> to <see cref="IPropertyContainer"/> of type <paramref name="outputType"/>.
        /// </summary>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="outputType">Result type.</param>
        /// <returns>Result property container.</returns>
        public static IPropertyContainer ToPropertyContainerOfType(this IPropertyContainer propertyContainer, Type outputType)
        {
            if (propertyContainer is IMutablePropertyContainer && (outputType == typeof(IMutablePropertyContainer) || outputType == typeof(MutablePropertyContainer)))
                return propertyContainer;

            if (outputType == typeof(IPropertyContainer) || outputType == typeof(PropertyContainer))
                return new PropertyContainer(sourceValues: propertyContainer.Properties);

            if (outputType.IsConcreteType())
            {
                /*
                 *  public PropertyContainer(
                        IEnumerable<IPropertyValue>? sourceValues = null,
                        IPropertyContainer? parentPropertySource = null,
                        SearchOptions? searchOptions = null)
                 */
                object[] ctorArgs = new object[] { propertyContainer.Properties, null, null };
                IPropertyContainer resultContainer = (IPropertyContainer)Activator.CreateInstance(outputType, args: ctorArgs);
                return resultContainer;
            }

            // Return ReadOnly PropertyContainer as a result.
            return new PropertyContainer(sourceValues: propertyContainer.Properties);
        }

        /// <summary>
        /// Converts source object to <see cref="IPropertyContainer"/> using <see cref="IPropertySet"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="sourceData">Source object.</param>
        /// <param name="addPropertiesNotFromSchema">Should include properties not from schema.</param>
        /// <param name="addPropertiesWithNullValues">Should add properties with null values.</param>
        /// <param name="propertyComparer">Optional property comparer for search property in schema. Default: <see cref="PropertyComparer.ByTypeAndNameIgnoreCaseComparer"/>.</param>
        /// <param name="propertyFactory">Optional property factory.</param>
        /// <param name="propertyValueFactory">Optional property value factory.</param>
        /// <returns>Result property container.</returns>
        public static PropertyContainer<TSchema> ToPropertyContainer<TSchema>(
            this object sourceData,
            bool addPropertiesNotFromSchema = false,
            bool addPropertiesWithNullValues = false,
            IEqualityComparer<IProperty>? propertyComparer = null,
            IPropertyFactory? propertyFactory = null,
            IPropertyValueFactory? propertyValueFactory = null)
            where TSchema : IPropertySet, new()
        {
            propertyComparer ??= PropertyComparer.ByTypeAndNameIgnoreCaseComparer;
            propertyFactory ??= new CachedPropertyFactory();
            propertyValueFactory ??= new CachedPropertyValueFactory(new PropertyValueFactory(), propertyComparer);

            var schema = new TSchema();
            IProperty[] schemaProperties = schema.GetProperties().ToArray();

            var searchOptions = SearchOptions.Default.WithPropertyComparer(propertyComparer);
            var propertyContainer = new MutablePropertyContainer(searchOptions: searchOptions);

            PropertyInfo[] propertyInfos = sourceData.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object? value = propertyInfo.GetValue(sourceData);

                IProperty propertyFromType = propertyFactory.Create(propertyInfo.PropertyType, propertyInfo.Name);
                IProperty? schemaProperty = schemaProperties.FirstOrDefault(property => propertyComparer.Equals(property, propertyFromType));

                IProperty? propertyToAdd = schemaProperty;
                if (schemaProperty == null && addPropertiesNotFromSchema)
                {
                    // Property IsNotFromSchema
                    propertyToAdd = propertyFromType.SetIsNotFromSchema();
                }

                if (propertyToAdd != null)
                {
                    if (value is not null)
                    {
                        IPropertyValue propertyValue = propertyValueFactory.CreateUntyped(propertyToAdd, value);
                        propertyContainer.Add(propertyValue);
                    }
                    else
                    {
                        if (addPropertiesWithNullValues)
                        {
                            IPropertyValue propertyValue = propertyValueFactory.CreateUntyped(propertyToAdd, TypeExtensions.GetDefaultValue(propertyToAdd.Type), ValueSource.NotDefined);
                            propertyContainer.Add(propertyValue);
                        }
                    }
                }
            }

            return new PropertyContainer<TSchema>(sourceValues: propertyContainer, searchOptions: searchOptions);
        }

        /// <summary>
        /// Maps <see cref="IPropertyContainer"/> to object of type <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="factory">Optional factory.</param>
        /// <returns>Result object.</returns>
        public static TModel ToObject<TModel>(this IPropertyContainer propertyContainer, Func<TModel>? factory = null)
        {
            PropertyInfo[] propertyInfos = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            TModel model = factory != null ? factory() : Activator.CreateInstance<TModel>();

            foreach (IPropertyValue propertyValue in propertyContainer.Properties)
            {
                object? value = propertyValue.ValueUntyped;

                PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(info =>
                    string.Equals(info.Name, propertyValue.PropertyUntyped.Name, StringComparison.OrdinalIgnoreCase) &&
                    propertyValue.PropertyUntyped.Type.IsAssignableTo(info.PropertyType));

                if (propertyInfo == null)
                {
                    propertyInfo = propertyInfos.FirstOrDefault(info =>
                        string.Equals(info.Name, propertyValue.PropertyUntyped.Name, StringComparison.OrdinalIgnoreCase));

                    if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
                    {
                        if (Enum.TryParse(propertyInfo.PropertyType, value?.ToString(), ignoreCase: true, out object? enumValue))
                        {
                            value = enumValue;
                        }
                    }

                    if (propertyInfo != null && propertyInfo.PropertyType.IsNullableStruct())
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType)!;
                        if (underlyingType.IsEnum && Enum.TryParse(underlyingType, value?.ToString(), ignoreCase: true, out object? enumValue))
                        {
                            value = enumValue;
                        }
                    }

                    if (propertyInfo == null)
                    {
                        int breakpoint = 1;
                    }

                }

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(model, value);
                }
                else
                {
                    int breakpoint = 2;
                }
            }

            return model;
        }
    }

    public class PropertyContainerMapperSettings
    {
        public bool? AddPropertiesNotFromSchema { get; set; }

        public bool? AddPropertiesWithNullValues { get; set; }

        public IEqualityComparer<IProperty>? PropertyComparer { get; set; }

        public IPropertyFactory? PropertyFactory { get; set; }

        public IPropertyValueFactory? PropertyValueFactory { get; set; }
    }
}

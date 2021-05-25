// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;

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
        /// <param name="includePropertiesNotFromSchema">Should include properties not from schema.</param>
        /// <param name="propertyComparer">Optional property comparer for search property in schema. Default: <see cref="PropertyComparer.ByTypeAndNameIgnoreCaseComparer"/>.</param>
        /// <param name="propertyFactory">Optional property factory.</param>
        /// <param name="propertyValueFactory">Optional property value factory.</param>
        /// <returns>Result property container.</returns>
        public static PropertyContainer<TSchema> ToPropertyContainer<TSchema>(
            this object sourceData,
            bool includePropertiesNotFromSchema = false,
            IEqualityComparer<IProperty>? propertyComparer = null,
            IPropertyFactory? propertyFactory = null,
            IPropertyValueFactory? propertyValueFactory = null)
            where TSchema : IPropertySet, new()
        {
            propertyComparer ??= PropertyComparer.ByTypeAndNameIgnoreCaseComparer;
            propertyFactory ??= new CachedPropertyFactory();
            propertyValueFactory ??= new CachedPropertyValueFactory(new PropertyValueFactory(), propertyComparer);

            var schema = new TSchema();
            IProperty[] schemaProperties = schema.ToArray();

            var searchOptions = SearchOptions.Default.WithPropertyComparer(propertyComparer);
            var propertyContainer = new MutablePropertyContainer(searchOptions: searchOptions);

            PropertyInfo[] propertyInfos = sourceData.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object? value = propertyInfo.GetValue(sourceData);

                IProperty propertyFromType = propertyFactory.Create(propertyInfo.PropertyType, propertyInfo.Name);
                IProperty? schemaProperty = schemaProperties.FirstOrDefault(property => propertyComparer.Equals(property, propertyFromType));

                // propertyInfo.PropertyType can be Nullable but property in schema can be not nullable
                if (schemaProperty == null && value != null && value.GetType() != propertyInfo.PropertyType)
                {
                    propertyFromType = propertyFactory.Create(value.GetType(), propertyInfo.Name);
                    schemaProperty = schemaProperties.FirstOrDefault(property => propertyComparer.Equals(property, propertyFromType));
                }

                IProperty? propertyToAdd = schemaProperty;
                if (schemaProperty == null && includePropertiesNotFromSchema)
                    propertyToAdd = propertyFromType;

                if (propertyToAdd != null)
                {
                    IPropertyValue propertyValue = propertyValueFactory.CreateUntyped(propertyToAdd, value);
                    propertyContainer.Add(propertyValue);
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
            //var schema = propertyContainer.GetType().GetSchemaByKnownPropertySet();
            //IProperty[]? schemaProperties = schema?.ToArray();
            PropertyInfo[] propertyInfos = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            TModel model = factory != null ? factory() : Activator.CreateInstance<TModel>();

            foreach (IPropertyValue propertyValue in propertyContainer.Properties)
            {
                PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(info =>
                    info.Name == propertyValue.PropertyUntyped.Name &&
                    propertyValue.PropertyUntyped.Type.IsAssignableTo(info.PropertyType));

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(model, propertyValue.ValueUntyped);
                }
            }

            return model;
        }
    }
}

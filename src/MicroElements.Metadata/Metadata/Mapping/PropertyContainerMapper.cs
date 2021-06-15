// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Mapping
{
    /// <summary>
    /// PropertyContainer mapping extensions.
    /// </summary>
    public static class PropertyContainerMapper
    {
        /// <summary>
        /// Converts source object to <see cref="IPropertyContainer"/> using schema <typeparamref name="TSchema"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="value">Source object.</param>
        /// <param name="settings">Optional settings.</param>
        /// <param name="configure">Optional configure action.</param>
        /// <returns>Property container created from source object.</returns>
        public static PropertyContainer<TSchema> MapToContainer<TSchema>(
            this object value,
            PropertyContainerMapperSettings? settings = null,
            Action<PropertyContainerMapperSettings>? configure = null)
            where TSchema : IPropertySet, new()
        {
            // TODO: Exception or diagnostic messages
            // TODO: IValueFormatter, IConverter
            // TODO: Convert errors, type mismatch, nullable support

            if (configure != null)
            {
                settings ??= new PropertyContainerMapperSettings();
                configure.Invoke(settings);
            }

            // Context is same as settings but all properties in context are initialized.
            var context = new PropertyContainerMappingContext(settings);

            return value.MapToContainer<TSchema>(context);
        }

        /// <summary>
        /// Converts source values to <see cref="IPropertyContainer"/> enumeration using schema <typeparamref name="TSchema"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="values">Source values.</param>
        /// <param name="settings">Optional settings.</param>
        /// <param name="configure">Optional configure action.</param>
        /// <returns>Property container enumeration created from source values.</returns>
        public static IEnumerable<PropertyContainer<TSchema>> MapToContainers<TSchema>(
            this IEnumerable values,
            PropertyContainerMapperSettings? settings = null,
            Action<PropertyContainerMapperSettings>? configure = null)
            where TSchema : IPropertySet, new()
        {
            if (configure != null)
            {
                settings ??= new PropertyContainerMapperSettings();
                configure.Invoke(settings);
            }

            // Context is same as settings but all properties in context are initialized.
            var context = new PropertyContainerMappingContext(settings);

            foreach (var value in values)
            {
                yield return value.MapToContainer<TSchema>(context);
            }
        }

        /// <summary>
        /// Converts source object to <see cref="IPropertyContainer"/> using schema <typeparamref name="TSchema"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="sourceData">Source object.</param>
        /// <param name="context">Mapping settings.</param>
        /// <returns>Property container created from source object.</returns>
        public static PropertyContainer<TSchema> MapToContainer<TSchema>(
            this object sourceData,
            PropertyContainerMappingContext context)
            where TSchema : IPropertySet, new()
        {
            context.AssertArgumentNotNull(nameof(context));

            var schema = new TSchema();
            IProperty[] schemaProperties = schema.GetProperties().ToArray();

            var searchOptions = SearchOptions.Default.WithPropertyComparer(context.PropertyComparer);
            var propertyContainer = new MutablePropertyContainer(searchOptions: searchOptions);

            PropertyInfo[] propertyInfos = sourceData.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object? value = propertyInfo.GetValue(sourceData);

                IProperty propertyFromType = context.PropertyFactory.Create(propertyInfo.PropertyType, propertyInfo.Name);
                IProperty? schemaProperty = schemaProperties.FirstOrDefault(property => context.PropertyComparer.Equals(property, propertyFromType));

                IProperty? propertyToAdd = schemaProperty;
                if (schemaProperty == null && context.AddPropertiesNotFromSchema)
                {
                    // Property IsNotFromSchema
                    propertyToAdd = propertyFromType.SetIsNotFromSchema();
                }

                if (propertyToAdd != null)
                {
                    if (value is not null)
                    {
                        if (value.GetType().IsAssignableTo(propertyToAdd.Type))
                        {
                            IPropertyValue propertyValue = context.PropertyValueFactory.CreateUntyped(propertyToAdd, value);
                            propertyContainer.Add(propertyValue);
                        }
                        else
                        {
                            object? convertedValue = null;
                            if (propertyToAdd.Type == typeof(string))
                            {
                                convertedValue = context.ValueFormatter.Format(value, value.GetType());
                            }
                            else if (value is IConvertible convertible)
                            {
                                convertedValue = convertible.ToType(propertyToAdd.Type, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                // CAN NOT CONVERT TO SCHEMA TYPE!!!
                                IPropertyValue propertyValue = context.PropertyValueFactory.CreateUntyped(propertyFromType, value);
                                propertyContainer.Add(propertyValue);
                            }

                            if (convertedValue != null)
                            {
                                IPropertyValue propertyValue = context.PropertyValueFactory.CreateUntyped(propertyToAdd, convertedValue);
                                propertyContainer.Add(propertyValue);
                            }
                        }
                    }
                    else
                    {
                        if (context.AddPropertiesWithNullValues)
                        {
                            IPropertyValue propertyValue = context.PropertyValueFactory.CreateUntyped(propertyToAdd, TypeExtensions.GetDefaultValue(propertyToAdd.Type), ValueSource.NotDefined);
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
        public static TModel MapToObject<TModel>(
            this IPropertyContainer propertyContainer,
            Func<TModel>? factory = null)
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

        /// <summary>
        /// Maps <see cref="IPropertyContainer"/> enumeration to <typeparamref name="TModel"/> enumeration.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="propertyContainers">Source property container enumeration.</param>
        /// <param name="factory">Optional factory.</param>
        /// <returns>Result object.</returns>
        public static IEnumerable<TModel> MapToObjects<TModel>(
            this IEnumerable<IPropertyContainer> propertyContainers,
            Func<TModel>? factory = null)
        {
            foreach (IPropertyContainer propertyContainer in propertyContainers)
            {
                yield return propertyContainer.MapToObject(factory);
            }
        }
    }
}

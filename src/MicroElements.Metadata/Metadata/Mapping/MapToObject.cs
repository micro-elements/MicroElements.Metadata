// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;

namespace MicroElements.Metadata.Mapping
{
    /// <summary>
    /// PropertyContainer mapping extensions.
    /// </summary>
    public static partial class PropertyContainerMapper
    {
        /// <summary>
        /// Maps <see cref="IPropertyContainer"/> to object of type <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="settings">Mapping factory.</param>
        /// <returns>Result object.</returns>
        public static TModel MapToObject<TModel>(
            this IPropertyContainer propertyContainer,
            MapToObjectSettings<TModel>? settings = null)
        {
            settings ??= new MapToObjectSettings<TModel>();

            PropertyInfo[] propertyInfos = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            TModel model = settings.Factory != null ? settings.Factory() : Activator.CreateInstance<TModel>();

            foreach (IPropertyValue propertyValue in propertyContainer.Properties)
            {
                if (settings.SourceFilter?.Invoke(propertyValue.PropertyUntyped) == false)
                    continue;

                string targetPropertyName = settings.TargetName?.Invoke(propertyValue.PropertyUntyped) ?? propertyValue.PropertyUntyped.Name;

                (PropertyInfo? propertyInfo, object? value) = TryGetTargetPropertyAndValue(propertyValue, propertyInfos);

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

        public static (PropertyInfo? PropertyInfo, object? Value) TryGetTargetPropertyAndValue(IPropertyValue propertyValue, PropertyInfo[] propertyInfos)
        {
            object? value = propertyValue.ValueUntyped;

            // Search by name and type (best choice, no map required)
            PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(info =>
                string.Equals(info.Name, propertyValue.PropertyUntyped.Name, StringComparison.OrdinalIgnoreCase) &&
                propertyValue.PropertyUntyped.Type.IsAssignableTo(info.PropertyType));

            if (propertyInfo == null)
            {
                // Search by name only (other type, need to map)
                propertyInfo = propertyInfos.FirstOrDefault(info =>
                    string.Equals(info.Name, propertyValue.PropertyUntyped.Name, StringComparison.OrdinalIgnoreCase));

                if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
                {
                    if (Enum.TryParse(propertyInfo.PropertyType, value?.ToString(), ignoreCase: true, out object? enumValue))
                    {
                        value = enumValue;
                        //return;
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

            return (propertyInfo, value);
        }

        /// <summary>
        /// Maps <see cref="IPropertyContainer"/> enumeration to <typeparamref name="TModel"/> enumeration.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="propertyContainers">Source property container enumeration.</param>
        /// <param name="settings">Optional mapping settings.</param>
        /// <returns>Result object.</returns>
        public static IEnumerable<TModel> MapToObjects<TModel>(
            this IEnumerable<IPropertyContainer> propertyContainers,
            MapToObjectSettings<TModel>? settings = null)
        {
            foreach (IPropertyContainer propertyContainer in propertyContainers)
            {
                yield return propertyContainer.MapToObject(settings);
            }
        }
    }
}

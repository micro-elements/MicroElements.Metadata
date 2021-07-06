// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;
using MicroElements.Validation;

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
            TModel model = settings.Factory != null ? settings.Factory() : Activator.CreateInstance<TModel>();

            PropertyInfo[] propertyInfos = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (IPropertyValue propertyValue in propertyContainer.Properties)
            {
                if (settings.SourceFilter?.Invoke(propertyValue.PropertyUntyped) == false)
                    continue;

                (PropertyInfo? propertyInfo, object? value) = TryGetTargetPropertyAndValue(propertyValue, typeof(TModel), propertyInfos, settings);

                if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(model, value);
                    }
                    else
                    {
                        settings.LogMessage?.Invoke(new Message($"Property {propertyInfo.Name} is not writable", MessageSeverity.Error));
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Fills object with values from property container.
        /// </summary>
        /// <typeparam name="TModel">Model type.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="settings">Mapping factory.</param>
        /// <returns>Result object.</returns>
        public static TModel FillObject<TModel>(
            this IPropertyContainer propertyContainer,
            TModel model,
            MapToObjectSettings<TModel>? settings = null)
        {
            model.AssertArgumentNotNull(nameof(model));

            // Override factory with provided object.
            MapToObjectSettings<TModel> context = settings ?? new MapToObjectSettings<TModel>();
            context = context with { Factory = () => model };

            return MapToObject(propertyContainer, context);
        }

        public static (PropertyInfo? PropertyInfo, object? Value) TryGetTargetPropertyAndValue(
            IPropertyValue propertyValue,
            Type modelType,
            PropertyInfo[]? propertyInfos,
            IMapToObjectSettings settings)
        {
            object? value = propertyValue.ValueUntyped;

            string targetPropertyName = settings.TargetName?.Invoke(propertyValue.PropertyUntyped) ?? propertyValue.PropertyUntyped.Name;

            propertyInfos ??= modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Search by name and type (best choice, no map required)
            PropertyInfo? propertyInfo = propertyInfos.FirstOrDefault(info =>
                string.Equals(info.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase) &&
                propertyValue.PropertyUntyped.Type.IsAssignableTo(info.PropertyType));

            if (propertyInfo == null)
            {
                // Search by name only (other type, need to map)
                propertyInfo = propertyInfos.FirstOrDefault(info =>
                    string.Equals(info.Name, targetPropertyName, StringComparison.OrdinalIgnoreCase));

                string? textValue = value?.ToString();
                if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
                {
                    if (Enum.TryParse(propertyInfo.PropertyType, textValue, ignoreCase: true, out object? enumValue))
                    {
                        value = enumValue;
                    }
                    else
                    {
                        settings.LogMessage?.Invoke(MappingError.NotEnumValue(propertyInfo.PropertyType, textValue));
                    }
                }

                if (propertyInfo != null && propertyInfo.PropertyType.IsNullableStruct())
                {
                    Type underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType)!;
                    if (underlyingType.IsEnum)
                    {
                        if (Enum.TryParse(underlyingType, textValue, ignoreCase: true, out object? enumValue))
                        {
                            value = enumValue;
                        }
                        else
                        {
                            settings.LogMessage?.Invoke(MappingError.NotEnumValue(propertyInfo.PropertyType, textValue));
                        }
                    }
                }

                if (propertyInfo == null)
                {
                    settings.LogMessage?.Invoke(new Message("Property {propertyName} was not found in type {modelType}", MessageSeverity.Error)
                        .WithArgs(targetPropertyName, modelType.FullName));
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

    public static class MappingError
    {
        public static Message NotEnumValue(Type enumType, string? text)
        {
            return new Message(
                $"Can not convert value {text} to enum {enumType.GetFriendlyName()}",
                MessageSeverity.Error);
        }
    }
}

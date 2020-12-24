// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Shared;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Metadata serializer settings.
    /// </summary>
    public interface IMapperSettings
    {
        /// <summary>
        /// Gets type name.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>Type name.</returns>
        string GetTypeName(Type type);

        /// <summary>
        /// Gets type by name.
        /// It should work with all type names generated with <see cref="GetTypeName"/>.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <returns>Type.</returns>
        Type GetTypeByName(string typeName);

        /// <summary>
        /// Serializes value to string.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <param name="value">Optional value.</param>
        /// <returns>String or null.</returns>
        string? SerializeValue(Type type, object? value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        Result<object?, Message> DeserializeValue(Type type, string? text);

    }

    public class DefaultMapperSettings : IMapperSettings
    {
        public static readonly DefaultMapperSettings Instance = new DefaultMapperSettings();

        /// <summary>
        /// Invariant format info. Uses '.' as decimal separator for floating point numbers.
        /// </summary>
        public static readonly NumberFormatInfo DefaultNumberFormatInfo = NumberFormatInfo.ReadOnly(
            new NumberFormatInfo {
                NumberDecimalSeparator = ".",
            });

        public static TypeCache TypeCache { get; }

        static DefaultMapperSettings()
        {
            var typeRegistrations = Enumerable.Empty<TypeRegistration>()
                .Concat(TypeCache.NumericTypesWithNullable.TypeSource.TypeRegistrations)
                .Concat(TypeCache.NodaTimeTypes.Value.TypeSource.TypeRegistrations)
                .Concat(new[] 
                {
                    new TypeRegistration(typeof(string), "string"),
                    new TypeRegistration(typeof(DateTime), "DateTime"),
                    new TypeRegistration(typeof(DateTime?), "DateTime?"),
                })
                .ToArray();

            TypeCache = TypeCache.Create(
                AssemblySource.Default,
                TypeSource.Empty.With(typeRegistrations: typeRegistrations));
        }

        public DefaultMapperSettings()
        { }

        /// <inheritdoc />
        public string GetTypeName(Type type)
        {
            return TypeCache.GetAliasForType(type) ?? type.FullName;
        }

        /// <inheritdoc />
        public Type GetTypeByName(string typeName)
        {
            return TypeCache.GetByAliasOrFullName(typeName);
        }

        /// <inheritdoc />
        public string? SerializeValue(Type type, object? value)
        {
            if (value == null)
                return "null";

            if (value is string stringValue)
                return stringValue;

            if (value is double doubleNumber)
                return doubleNumber.ToString(DefaultNumberFormatInfo);

            if (value is float floatNumber)
                return floatNumber.ToString(DefaultNumberFormatInfo);

            if (value is decimal decimalNumber)
                return decimalNumber.ToString(DefaultNumberFormatInfo);

            if (value is DateTime dateTime)
                return dateTime == dateTime.Date ? $"{dateTime:yyyy-MM-dd}" : $"{dateTime:yyyy-MM-ddTHH:mm:ss}";

            string typeFullName = type.FullName;

            if (typeFullName == "NodaTime.LocalDate" && value is IFormattable localDate)
                return localDate.ToString("yyyy-MM-dd", null);

            if (typeFullName == "NodaTime.LocalDateTime" && value is IFormattable localDateTime)
                return localDateTime.ToString("yyyy-MM-ddTHH:mm:ss", null);

            if (value is ICollection collection)
                return collection.FormatAsTuple(startSymbol: "[", endSymbol: "]");

            return $"{value}";
        }

        /// <inheritdoc />
        public Result<object?, Message> DeserializeValue(Type type, string? text)
        {
            if (text == "null")
                return (object?)null;

            if (type == typeof(string))
                return text;

            if (type.IsConcreteAndAssignableTo<ICollection>())
                return DeserializeCollection(type, text);

            Message ReturnWithError() =>
                new Message(
                        "Value {text} can not be parsed as {type}",
                        MessageSeverity.Error,
                        eventName: "ParseError")
                    .WithArgs(text, type);

            #region Numbers

            if (type == typeof(double) || type == typeof(double?))
            {
                if (double.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return ReturnWithError();
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                if (int.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return ReturnWithError();
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                if (float.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return ReturnWithError();
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                if (decimal.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return ReturnWithError();
            }

            #endregion

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (DateTime.TryParse(text, out var result))
                    return result;

                return ReturnWithError();
            }

            string typeFullName = type.FullName;

            if (typeFullName.StartsWith("NodaTime"))
            {
                if (typeFullName == "NodaTime.LocalDate")
                {
                    if (DateTime.TryParse(text, out var dateTime))
                    {
                        return TryCreateInstance(type, dateTime.Year, dateTime.Month, dateTime.Day);
                    }
                }
                if (typeFullName == "NodaTime.LocalDateTime")
                {
                    if (DateTime.TryParse(text, out var dateTime))
                    {
                        return TryCreateInstance(type, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                    }
                }
            }

            Result<object?, Message> TryCreateInstance(Type type, params object[] args)
            {
                try
                {
                    return Activator.CreateInstance(type, args);
                }
                catch (Exception e)
                {
                    return Error.CreateError("CreateError", "Value {text} can not be parsed as {type}. Error: {error}", text, type, e.Message).Message;
                }
            }

            return ReturnWithError();
        }

        private Result<object?, Message> DeserializeCollection(Type type, string text)
        {
            string[] strings = text.TrimStart('[').TrimEnd(']').Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

            Type elementType = typeof(string);
            if (type.IsArray)
                elementType = type.GetElementType();

            if (elementType == typeof(string))
                return strings;

            var elements = strings
                .Select(s => DeserializeValue(elementType, s).GetValueOrDefault(message => elementType.GetDefaultValue()))
                .ToArray(elementType);

            return elements;
        }
    }

    public static class Mapper
    {
        public static object ToArray(this IEnumerable<object?> objects, Type type)
        {
            object?[] array = objects.ToArray();
            Array typedArray = Array.CreateInstance(type, array.Length);
            Array.Copy(array, typedArray, array.Length);
            return typedArray;
        }

        public static PropertyContainerContract ToContract(this IPropertyContainer propertyContainer, IMapperSettings mapperSettings)
        {
            var properties = propertyContainer.Properties.Select(value => value.ToContract(mapperSettings)).ToArray();
            return new PropertyContainerContract()
            {
                Properties = properties
            };
        }

        public static PropertyValueContract ToContract(this IPropertyValue propertyValue, IMapperSettings mapperSettings)
        {
            return new PropertyValueContract()
            {
                Name = propertyValue.PropertyUntyped.Name,
                Type = mapperSettings.GetTypeName(propertyValue.PropertyUntyped.Type),
                Value = mapperSettings.SerializeValue(propertyValue.PropertyUntyped.Type, propertyValue.ValueUntyped)
            };
        }

        public static IPropertyContainer ToModel(this PropertyContainerContract contract, IMapperSettings mapperSettings)
        {
            var propertyValues = contract.Properties.NotNull().Select(valueContract => valueContract.ToModel(mapperSettings));
            return new PropertyContainer(sourceValues: propertyValues);
        }

        public static IPropertyValue ToModel(this PropertyValueContract propertyValueContract, IMapperSettings mapperSettings, IMutableMessageList<Message>? messages = null)
        {
            Type propertyType = mapperSettings.GetTypeByName(propertyValueContract.Type);
            string propertyName = propertyValueContract.Name;
            IProperty property = Property.Create(propertyType, propertyName);

            var propertyValueResult = mapperSettings.DeserializeValue(propertyType, propertyValueContract.Value);

            object? value = propertyValueResult.GetValueOrDefault(message =>
            {
                messages?.Add(message);
                return null;
            });

            IPropertyValue propertyValue = PropertyValue.Create(property, value: value, valueSource: propertyValueResult.IsSuccess ? ValueSource.Defined : ValueSource.NotDefined);
            return propertyValue;
        }
    }
}

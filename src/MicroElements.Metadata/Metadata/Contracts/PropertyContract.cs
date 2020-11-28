// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Shared;

namespace MicroElements.Metadata.Contracts
{
    public class PropertyContainerContract
    {
        public PropertyValueContract[] Properties { get; set; }
    }

    public class PropertyValueContract
    {
        /// <summary>
        /// Gets property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets property value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets property value type.
        /// </summary>
        public string Type { get; set; }
    }

    public interface IMapperSettings
    {
        string GetTypeName(Type type);

        Type GetTypeByName(string typeName);

        string? SerializeValue(Type type, object? value);

        Result<object?, IError<string>> DeserializeValue(Type type, string? text);
    }

    public abstract class TypeConverter
    {
        public abstract bool CanConvert(Type typeToConvert);

        // This is used internally to quickly determine the type being converted for JsonConverter<T>.
        internal virtual Type TypeToConvert => null;
    }

    public abstract class TypeConverter<T> : TypeConverter
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(T);
        }

        public abstract string? Serialize(T value);

        public abstract Result<T, IError<string>> Deserialize(string? text);
    }

    public class DefaultMapperSettings : IMapperSettings
    {
        /// <summary>
        /// Invariant format info. Uses '.' as decimal separator for floating point numbers.
        /// </summary>
        public static readonly NumberFormatInfo DefaultNumberFormatInfo = NumberFormatInfo.ReadOnly(
            new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
            });

        private static TypeCache TypeCache { get; }

        static DefaultMapperSettings()
        {
            TypeRegistration[] registrations =
            {
                new TypeRegistration(typeof(string), "string"),

                new TypeRegistration(typeof(int), "int"),
                new TypeRegistration(typeof(int?), "int?"),

                new TypeRegistration(typeof(double), "double"),
                new TypeRegistration(typeof(double?), "double?"),

                new TypeRegistration(typeof(float), "float"),
                new TypeRegistration(typeof(float?), "float?"),

                new TypeRegistration(typeof(decimal), "decimal"),
                new TypeRegistration(typeof(decimal?), "decimal?"),

                new TypeRegistration(typeof(DateTime), "DateTime"),
                new TypeRegistration(typeof(DateTime?), "DateTime?"),
            };

            IEnumerable<TypeRegistration> NodaTypeRegistrations(string fullName, string alias)
            {
                Type? nodaType = TypeCache.Default.GetType(fullName);
                if (nodaType != null)
                {
                    yield return new TypeRegistration(nodaType, alias);
                    if (nodaType.IsValueType)
                    {
                        Type nullableType = typeof(Nullable<>).MakeGenericType(nodaType);
                        yield return new TypeRegistration(nullableType, alias + "?");
                    }
                }
            }

            IEnumerable<TypeRegistration> nodaTimeRegistrations = Enumerable.Empty<TypeRegistration>()
                .Concat(NodaTypeRegistrations("NodaTime.LocalDate", "LocalDate"))
                .Concat(NodaTypeRegistrations("NodaTime.LocalDateTime", "LocalDateTime"))
                .ToArray();

            var typeRegistrations = registrations.Concat(nodaTimeRegistrations).ToArray();

            TypeCache = TypeCache.Create(
                AssemblySource.Default,
                TypeSource.Default.With(typeRegistrations: typeRegistrations));
        }

        public DefaultMapperSettings()
        {
        }

        /// <inheritdoc />
        public string GetTypeName(Type type)
        {
            if (type == typeof(string))
                return "string";

            if (type == typeof(int))
                return "int";
            if (type == typeof(int?))
                return "int?";

            if (type == typeof(double))
                return "double";
            if (type == typeof(double?))
                return "double?";

            if (type == typeof(float))
                return "float";
            if (type == typeof(float?))
                return "float?";

            if (type == typeof(decimal))
                return "decimal";
            if (type == typeof(decimal?))
                return "decimal?";

            if (type == typeof(DateTime))
                return "DateTime";
            if (type == typeof(DateTime?))
                return "DateTime?";

            string typeFullName = type.FullName;

            if (typeFullName == "NodaTime.LocalDate")
                return "LocalDate";
            if (typeFullName.StartsWith("System.Nullable`1[[NodaTime.LocalDate, NodaTime"))
                return "LocalDate?";

            if (typeFullName == "NodaTime.LocalDateTime")
                return "LocalDateTime";
            if (typeFullName.StartsWith("System.Nullable`1[[NodaTime.LocalDateTime, NodaTime"))
                return "LocalDateTime?";

            return typeFullName;
        }

        /// <inheritdoc />
        public Type GetTypeByName(string typeName)
        {
            return TypeCache.GetByAlias(typeName) ?? TypeCache.GetType(typeName);
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

            return $"{value}";
        }

        /// <inheritdoc />
        public Result<object?, IError<string>> DeserializeValue(Type type, string? text)
        {
            if (text == "null")
                return Result.Success<object>(null);

            if (type == typeof(string))
                return text;

            #region Numbers

            if (type == typeof(double) || type == typeof(double?))
            {
                if (double.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                if (int.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                if (float.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                if (decimal.TryParse(text, NumberStyles.Number, DefaultNumberFormatInfo, out var result))
                    return result;

                return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
            }

            #endregion

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (DateTime.TryParse(text, out var result))
                    return result;

                return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
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

            Result<object?, IError<string>> TryCreateInstance(Type type, params object[] args)
            {
                try
                {
                    return Activator.CreateInstance(type, args);
                }
                catch (Exception e)
                {
                    return Error.CreateError("CreateError", "Value {text} can not be parsed as {type}. Error: {error}", text, type, e.Message);
                }
            }

            return Error.CreateError("ParseError", "Value {text} can not be parsed as {type}", text, type);
        }
    }

    public static class Mapper
    {
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

        public static IPropertyValue ToModel(this PropertyValueContract propertyValueContract, IMapperSettings mapperSettings, IMutableMessageList<Message>? messages = null)
        {
            Type propertyType = mapperSettings.GetTypeByName(propertyValueContract.Type);
            string propertyName = propertyValueContract.Name;
            var propertyValueResult = mapperSettings.DeserializeValue(propertyType, propertyValueContract.Value);

            IProperty property = Property.Create(propertyType, propertyName);

            object? value = propertyValueResult.GetValueOrDefault(error =>
            {
                messages?.Add(error.Message);
                return propertyType.GetDefaultValue();
            });

            IPropertyValue propertyValue = PropertyValue.Create(property, value: value, valueSource: propertyValueResult.IsSuccess ? ValueSource.Defined : ValueSource.NotDefined);
            return propertyValue;
        }

    }

}

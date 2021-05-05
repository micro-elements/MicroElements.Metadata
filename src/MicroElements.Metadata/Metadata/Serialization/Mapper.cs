// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroElements.Diagnostics;
using MicroElements.Functional;
using MicroElements.Metadata.Formatting;
using MicroElements.Metadata.Parsing;
using Message = MicroElements.Diagnostics.Message;
using MessageSeverity = MicroElements.Diagnostics.MessageSeverity;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Metadata serializer settings.
    /// </summary>
    public interface IMapperSettings : ITypeMapper
    {
        /// <summary>
        /// Serializes value to string.
        /// </summary>
        /// <param name="type">Value type.</param>
        /// <param name="value">Optional value.</param>
        /// <returns>String or null.</returns>
        string? SerializeValue(Type type, object? value);

        /// <summary>
        /// Deserializes value from string.
        /// </summary>
        /// <param name="type">Target type.</param>
        /// <param name="text">Source text.</param>
        /// <returns>Parse result.</returns>
        IParseResult DeserializeValue(Type type, string? text);
    }

    /// <summary>
    /// Default metadata serializer settings.
    /// </summary>
    public class DefaultMapperSettings : IMapperSettings
    {
        public static readonly DefaultMapperSettings Instance = new DefaultMapperSettings();

        private readonly ITypeMapper _typeMapper;
        private readonly IValueFormatter _valueFormatter;

        public DefaultMapperSettings(
            ITypeMapper? typeMapper = null,
            IValueFormatter? valueFormatter = null)
        {
            _typeMapper = typeMapper ?? DefaultTypeMapper.Instance;
            _valueFormatter = valueFormatter ?? Formatter.FullToStringFormatter;
        }

        /// <inheritdoc />
        public string GetTypeName(Type type)
            => _typeMapper.GetTypeName(type);

        /// <inheritdoc />
        public Type? GetTypeByName(string typeName)
            => _typeMapper.GetTypeByName(typeName);

        /// <inheritdoc />
        public string? SerializeValue(Type type, object? value)
            => _valueFormatter.Format(value, type);

        /// <inheritdoc />
        public IParseResult DeserializeValue(Type type, string? text)
        {
            if (text == null || text == "null")
                return ParseResult.Success((object?)null);

            IValueParserProvider valueParserProvider = DefaultValueParserProvider.Instance;
            var cachedParserProvider = CachedValueParserProvider.Create(valueParserProvider);
            IPropertyFactory propertyFactory = new CachedPropertyFactory();
            IProperty property = propertyFactory.Create(type, type.FullName);
            IValueParser valueParser = cachedParserProvider.GetParser(property);

            IParseResult parseResult = valueParser.ParseUntyped(text);
            return parseResult;

            //if (type == typeof(string))
            //    return text;

            //if (type.IsConcreteAndAssignableTo<ICollection>())
            //    return DeserializeCollection(type, text);

            //IParseResult ReturnWithError<T>()
            //{
            //    Message error = new Message(
            //            "Value {text} can not be parsed as {type}",
            //            MessageSeverity.Error,
            //            eventName: "ParseError")
            //        .WithArgs(text, type);

            //    return ParseResult.Failed<T>(error);
            //}

            //#region Numbers

            //if (type == typeof(double) || type == typeof(double?))
            //{
            //    return Parser.DoubleParser.Parse(text);
            //}

            //if (type == typeof(int) || type == typeof(int?))
            //{
            //    return Parser.IntParser.Parse(text);
            //}

            //if (type == typeof(float) || type == typeof(float?))
            //{
            //    return Parser.FloatParser.Parse(text);
            //}

            //if (type == typeof(decimal) || type == typeof(decimal?))
            //{
            //    return Parser.DecimalParser.Parse(text);
            //}

            //#endregion

            //if (type == typeof(DateTime) || type == typeof(DateTime?))
            //{
            //    return Parser.DateTimeParser.Parse(text);
            //}

            //string typeFullName = type.FullName;

            //if (typeFullName.StartsWith("NodaTime"))
            //{
            //    if (typeFullName == "NodaTime.LocalDate")
            //    {
            //        if (DateTime.TryParse(text, out var dateTime))
            //        {
            //            return TryCreateInstance(type, dateTime.Year, dateTime.Month, dateTime.Day);
            //        }
            //    }
            //    if (typeFullName == "NodaTime.LocalDateTime")
            //    {
            //        if (DateTime.TryParse(text, out var dateTime))
            //        {
            //            return TryCreateInstance(type, dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
            //        }
            //    }
            //}

            //Result<object?, Message> TryCreateInstance(Type type, params object[] args)
            //{
            //    try
            //    {
            //        return Activator.CreateInstance(type, args);
            //    }
            //    catch (Exception e)
            //    {
            //        return Error.CreateError("CreateError", "Value {text} can not be parsed as {type}. Error: {error}", text, type, e.Message).Message;
            //    }
            //}

            //return ReturnWithError();
        }

        //private Result<object?, Message> DeserializeCollection(Type type, string text)
        //{
        //    string[] strings = text.TrimStart('[').TrimEnd(']').Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

        //    Type elementType = typeof(string);
        //    if (type.IsArray)
        //        elementType = type.GetElementType();

        //    if (elementType == typeof(string))
        //        return strings;

        //    var elements = strings
        //        .Select(s => DeserializeValue(elementType, s).GetValueOrDefault(message => elementType.GetDefaultValue()))
        //        .ToArray(elementType);

        //    return elements;
        //}
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

        public static IPropertyValue ToModel(this PropertyValueContract propertyValueContract, IMapperSettings mapperSettings, Diagnostics.IMutableMessageList<Message>? messages = null)
        {
            Type propertyType = mapperSettings.GetTypeByName(propertyValueContract.Type);
            string propertyName = propertyValueContract.Name;
            IProperty property = Property.Create(propertyType, propertyName);

            var propertyValueResult = mapperSettings.DeserializeValue(propertyType, propertyValueContract.Value);

            if (propertyValueResult.Error != null)
            {
                messages?.Add(propertyValueResult.Error);
            }

            object? value = propertyValueResult.ValueUntyped;

            IPropertyValue propertyValue = PropertyValue.Create(property, value: value, valueSource: propertyValueResult.IsSuccess ? ValueSource.Defined : ValueSource.NotDefined);
            return propertyValue;
        }
    }
}

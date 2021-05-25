// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Diagnostics;
using MicroElements.Extensions;
using MicroElements.Metadata.Formatting;
using MicroElements.Metadata.Parsing;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Metadata serializer settings.
    /// </summary>
    public interface IMetadataSerializer : ITypeMapper
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
    public class DefaultMetadataSerializer : IMetadataSerializer
    {
        public static readonly DefaultMetadataSerializer Instance = new DefaultMetadataSerializer();

        private readonly ITypeMapper _typeMapper;
        private readonly IValueFormatter _valueFormatter;
        private readonly IParserRuleProvider _parserRuleProvider;

        public DefaultMetadataSerializer(
            ITypeMapper? typeMapper = null,
            IValueFormatter? valueFormatter = null,
            IValueParserProvider? valueParserProvider = null)
        {
            _typeMapper = typeMapper ?? DefaultTypeMapper.Instance;
            _valueFormatter = valueFormatter ?? Formatter.FullToStringFormatter;

            valueParserProvider ??= DefaultValueParserProvider.Instance;

            _parserRuleProvider = CachedParserRuleProvider.Create(valueParserProvider: valueParserProvider);
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

            IValueParser valueParser = _parserRuleProvider.GetParserOrEmpty(type);

            IParseResult parseResult = valueParser.ParseUntyped(text);

            return parseResult;
        }
    }

    public static class Mapper
    {
        public static Array ToArrayOfType(this IEnumerable<object?> objects, Type type)
        {
            object?[] array = objects.ToArray();
            Array typedArray = Array.CreateInstance(type, array.Length);
            Array.Copy(array, typedArray, array.Length);
            return typedArray;
        }

        public static PropertyContainerContract ToContract(this IPropertyContainer propertyContainer, IMetadataSerializer metadataSerializer)
        {
            var properties = propertyContainer.Properties.Select(value => value.ToContract(metadataSerializer)).ToArray();
            return new PropertyContainerContract()
            {
                Properties = properties
            };
        }

        public static PropertyValueContract ToContract(this IPropertyValue propertyValue, IMetadataSerializer metadataSerializer)
        {
            return new PropertyValueContract()
            {
                Name = propertyValue.PropertyUntyped.Name,
                Type = metadataSerializer.GetTypeName(propertyValue.PropertyUntyped.Type),
                Value = metadataSerializer.SerializeValue(propertyValue.PropertyUntyped.Type, propertyValue.ValueUntyped)
            };
        }

        public static IPropertyContainer ToModel(this PropertyContainerContract contract, IMetadataSerializer metadataSerializer)
        {
            var propertyValues = contract.Properties.NotNull().Select(valueContract => valueContract.ToModel(metadataSerializer));
            return new PropertyContainer(sourceValues: propertyValues);
        }

        public static IPropertyValue ToModel(this PropertyValueContract propertyValueContract, IMetadataSerializer metadataSerializer, Diagnostics.IMutableMessageList<Message>? messages = null)
        {
            Type propertyType = metadataSerializer.GetTypeByName(propertyValueContract.Type);
            string propertyName = propertyValueContract.Name;
            IProperty property = Property.Create(propertyType, propertyName);

            var propertyValueResult = metadataSerializer.DeserializeValue(propertyType, propertyValueContract.Value);

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

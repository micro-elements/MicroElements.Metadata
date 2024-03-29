﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Diagnostics;
using MicroElements.Metadata.Parsing;
using MicroElements.Metadata.Schema;
using MicroElements.Validation;

namespace MicroElements.Metadata.Xml
{
    /// <summary>
    /// <see cref="IXmlParserContext"/> extensions.
    /// </summary>
    public static class XmlParserContextExtensions
    {
        /// <summary>
        /// Copies input <paramref name="context"/> with possible changes.
        /// </summary>
        /// <param name="context">Source parser context.</param>
        /// <param name="parserSettings">Optional parser settings.</param>
        /// <param name="schema">Optional schema.</param>
        /// <param name="messages">Optional message list.</param>
        /// <param name="parserRuleProvider">Optional parser rule provider.</param>
        /// <param name="schemaCache">Optional schemas cache.</param>
        /// <param name="validatorsCache">Optional validators cache.</param>
        /// <returns>New <see cref="IXmlParserContext"/> instance.</returns>
        public static IXmlParserContext With(
            this IXmlParserContext context,
            IXmlParserSettings? parserSettings = null,
            IMutableObjectSchema? schema = null,
            IMutableMessageList<Message>? messages = null,
            IParserRuleProvider? parserRuleProvider = null,
            ConcurrentDictionary<IProperty, ISchema>? schemaCache = null,
            ConcurrentDictionary<IProperty, IPropertyValidationRules>? validatorsCache = null)
        {
            context.AssertArgumentNotNull(nameof(context));

            return new XmlParserContext(
                parserSettings: parserSettings ?? context.ParserSettings,
                schema: schema ?? context.Schema,
                messages: messages ?? context.Messages,
                parserRuleProvider: parserRuleProvider ?? context.ParserRuleProvider,
                schemaCache: schemaCache ?? context.SchemaCache,
                validatorsCache: validatorsCache ?? context.ValidatorsCache);
        }

        /// <summary>
        /// Gets validation rules for property.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <returns>Validation rules for property.</returns>
        public static IPropertyValidationRules GetValidatorsCached(this IXmlParserContext context, IProperty property)
        {
            if (context.ValidatorsCache.TryGetValue(property, out IPropertyValidationRules result))
            {
                return result;
            }

            IValidationRule[] validationRules = context.ParserSettings.ValidationProvider.GetValidationRules(property).ToArray();
            result = new PropertyValidationRules(property, validationRules);
            context.ValidatorsCache.TryAdd(property, result);

            return result;
        }

        /// <summary>
        /// Gets or adds schema to property.
        /// If property is null then returns new empty schema.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <returns>Schema attached to property.</returns>
        public static ISchema GetOrCreateNewSchemaCached(this IXmlParserContext context, IProperty? property)
        {
            if (property == null)
                return new MutableObjectSchema();

            if (context.SchemaCache.TryGetValue(property, out var result))
            {
                return result;
            }

            result = property.GetNewSchema();
            if (result == null)
            {
                result = new MutableObjectSchema(name: property.Name);
                property.SetSchema(result);
            }

            context.SchemaCache.TryAdd(property, result);

            return result;
        }

        /// <summary>
        /// Gets schema for property.
        /// If property is null then returns null.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <param name="property">Source property.</param>
        /// <returns>Optional schema attached to property.</returns>
        public static ISchema? GetSchema(this IXmlParserContext context, IProperty? property)
        {
            if (property == null)
                return null;

            context.SchemaCache.TryGetValue(property, out var schema);
            return schema;
        }

        /// <summary>
        /// Gets properties that was not created from schema.
        /// </summary>
        /// <param name="parserContext">Source parser context.</param>
        /// <param name="schema">Source schema.</param>
        /// <param name="recursive">Recursive search in children schemas.</param>
        /// <returns>Properties that was not created from schema.</returns>
        public static IEnumerable<IProperty> GetPropertiesNotFromSchema(this IXmlParserContext parserContext, ISchema? schema = null, bool recursive = true)
        {
            // Use root schema.
            schema ??= parserContext.Schema;

            IEnumerable<IProperty> properties = schema.GetProperties();
            foreach (IProperty property in properties)
            {
                if (property.GetIsNotFromSchema())
                    yield return property;

                if (recursive)
                {
                    if (parserContext.GetSchema(property) is { } subSchema)
                    {
                        foreach (IProperty subProperty in parserContext.GetPropertiesNotFromSchema(subSchema, recursive))
                        {
                            yield return subProperty;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets properties that was not created from schema for the property.
        /// </summary>
        /// <param name="parserContext">Source parser context.</param>
        /// <param name="property">Property to get schema..</param>
        /// <param name="recursive">Recursive search in children schemas.</param>
        /// <returns>Properties that was not created from schema.</returns>
        public static IEnumerable<IProperty> GetPropertiesNotFromSchema(this IXmlParserContext parserContext, IProperty property, bool recursive = true)
        {
            ISchema? schema = parserContext.GetSchema(property);
            if (schema == null)
                return Array.Empty<IProperty>();

            return parserContext.GetPropertiesNotFromSchema(schema, recursive: recursive);
        }
    }
}

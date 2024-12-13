// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MicroElements.Metadata.AspNetCore;
using MicroElements.Metadata.JsonSchema;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection.TypeExtensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.Swashbuckle;

/// <summary>
/// Creates properties for <see cref="IPropertyContainer"/> according attached <see cref="IPropertySet"/>.
/// Use <see cref="PropertySetAttribute"/> to attach property list to <see cref="IPropertyContainer"/>.
/// </summary>
public class PropertyContainerSchemaFilter : ISchemaFilter
{
    private readonly IOptions<PropertyContainerSchemaFilterOptions> _options;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly SchemaGeneratorOptions _schemaGeneratorOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyContainerSchemaFilter"/> class.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <param name="serializerOptions">JsonSerializerOptions.</param>
    /// <param name="generatorOptions">Swagger generator options.</param>
    public PropertyContainerSchemaFilter(
        IOptions<PropertyContainerSchemaFilterOptions>? options,
        IOptions<AspNetJsonSerializerOptions> serializerOptions,
        SwaggerGenOptions generatorOptions)
    {
        _options = options ?? new OptionsWrapper<PropertyContainerSchemaFilterOptions>(new PropertyContainerSchemaFilterOptions());
        _serializerOptions = serializerOptions.Value.JsonSerializerOptions;
        _schemaGeneratorOptions = generatorOptions.SchemaGeneratorOptions;
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsAssignableTo<IPropertyContainer>())
        {
            IPropertySet? propertySet = null;
            Type? knownSchemaType = null;

            // Get by provided attribute [PropertySet].
            var propertySetAttribute = context.MemberInfo?.GetCustomAttribute<PropertySetAttribute>();
            if (propertySetAttribute != null)
            {
                knownSchemaType = propertySetAttribute.Type;
                propertySet = propertySetAttribute.GetPropertySet();
            }

            // Get by IKnownPropertySet
            if (propertySet == null)
            {
                propertySet = context.Type.GetSchemaByKnownPropertySet();
            }

            // "$ref": "#/components/schemas/KnownSchema"
            if (_options.Value.GenerateKnownSchemasAsRefs && propertySet != null)
            {
                knownSchemaType ??= propertySet.GetType();
                string knownSchemaId = _schemaGeneratorOptions.SchemaIdSelector(knownSchemaType);

                if (!context.SchemaRepository.Schemas.TryGetValue(knownSchemaId, out OpenApiSchema knownSchema))
                {
                    // Generate and fill knownSchema once for type.
                    knownSchema = context.SchemaGenerator.GenerateSchema(knownSchemaType, context.SchemaRepository);
                    FillObjectSchema(knownSchema, context, propertySet);
                    context.SchemaRepository.Schemas[knownSchemaId] = knownSchema;
                }

                schema.Type = null;
                schema.Items = null;
                schema.Properties = null;

                // "$ref": "#/components/schemas/KnownSchema"
                schema.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = knownSchemaId,
                };
            }
            else if (propertySet != null)
            {
                // Generate inlined schema
                FillObjectSchema(schema, context, propertySet);
            }
            else
            {
                // No known schema for property container.
                schema.Type = "object";
                schema.Items = null;
                schema.Properties = new Dictionary<string, OpenApiSchema>();
                schema.AdditionalPropertiesAllowed = true;
            }
        }
    }

    private void FillObjectSchema(OpenApiSchema schema, SchemaFilterContext context, IPropertySet propertySet)
    {
        schema.Type = "object";
        schema.Items = null;
        schema.Properties = new Dictionary<string, OpenApiSchema>();

        foreach (IProperty property in propertySet.GetProperties())
        {
            OpenApiSchema? propertySchema = context.SchemaGenerator.GenerateSchema(property.Type, context.SchemaRepository);
            propertySchema.Description = property.Description ?? propertySchema.Description;

            if (property.GetOrEvaluateNullability() is { } allowNull)
                propertySchema.Nullable = allowNull.IsNullAllowed;

            if (_options.Value.GenerateKnownSchemasAsRefs && property.GetSchema() is { } separateSchema)
            {
                string knownSchemaId = separateSchema.Name;

                if (!context.SchemaRepository.Schemas.TryGetValue(knownSchemaId, out OpenApiSchema knownSchema))
                {
                    // Generate and fill knownSchema once for type.
                    knownSchema = GenerateSchema(separateSchema);
                    context.SchemaRepository.Schemas[knownSchemaId] = knownSchema;
                }

                // "$ref": "#/components/schemas/knownSchemaId"
                OpenApiReference schemaRef = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = knownSchemaId,
                };

                if (propertySchema.Reference != null)
                {
                    propertySchema.AllOf.Add(new OpenApiSchema { Reference = propertySchema.Reference });
                    propertySchema.Reference = null;
                }

                propertySchema.AllOf.Add(new OpenApiSchema { Reference = schemaRef });

                if (knownSchema.Type == propertySchema.Type)
                {
                    // No need to duplicate type from known schema
                    propertySchema.Type = null;
                }
            }
            else
            {
                propertySchema = FillDataSchema(propertySchema, property);
            }

            string propertyName = _options.Value.ResolvePropertyName!(property.Name);
            schema.Properties.Add(propertyName, propertySchema);
        }

        if (propertySet.GetComponent<IOneOf>() is { } oneOf)
        {
            ISchema[] oneOfSchemas = oneOf.OneOf().ToArray();
            foreach (ISchema oneOfSchema in oneOfSchemas)
            {
                string knownSchemaId = oneOfSchema.Name;

                IPropertySet? otherPropSet;
                otherPropSet = oneOfSchema as IPropertySet;
                if (otherPropSet == null)
                {
                    otherPropSet = (oneOfSchema as IProperty)?.Type.GetSchemaByKnownPropertySet();
                }

                if (otherPropSet != null)
                {
                    if (!context.SchemaRepository.Schemas.TryGetValue(knownSchemaId, out OpenApiSchema knownSchema))
                    {
                        // Generate and fill knownSchema once for type.
                        knownSchema = new OpenApiSchema();
                        FillObjectSchema(knownSchema, context, otherPropSet);
                        context.SchemaRepository.Schemas[knownSchemaId] = knownSchema;
                    }

                    schema.OneOf.Add(new OpenApiSchema
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = knownSchemaId },
                    });
                }
            }
        }
    }

    private OpenApiSchema GenerateSchema(ISchema schema)
    {
        OpenApiSchema openApiSchema = new OpenApiSchema();
        openApiSchema = FillDataSchema(openApiSchema, schema);
        return openApiSchema;
    }

    private OpenApiSchema FillDataSchema(OpenApiSchema openApiSchema, ISchema schema)
    {
        if (openApiSchema.Type == null)
        {
            ISchema schemaForType = JsonTypeMapper.Instance.GetTypeNameExt(schema.Type);
            openApiSchema.Type = schemaForType.Name;

            if (schemaForType.GetStringFormat() is { } defaultStringFormat)
            {
                openApiSchema.Format = defaultStringFormat.Format;
            }
        }

        openApiSchema.Properties ??= new Dictionary<string, OpenApiSchema>();

        openApiSchema.Description ??= schema.GetComponent<ISchemaDescription>()?.Description;

        if (schema.GetNullability() is { } allowNull)
        {
            openApiSchema.Nullable = allowNull.IsNullAllowed;
        }

        if (schema.GetAllowedValuesUntyped() is { } allowedValues)
        {
            if (schema.Type.IsEnum)
            {
                openApiSchema.Type = "string";
            }

            openApiSchema.Enum = new List<IOpenApiAny>();
            foreach (object allowedValue in allowedValues.ValuesUntyped)
            {
                string jsonValue = JsonConverterFunc(allowedValue);
                IOpenApiAny openApiAny = OpenApiAnyFactory.CreateFromJson(jsonValue);
                openApiSchema.Enum.Add(openApiAny);
            }
        }

        if (schema.GetNumericInterval() is { } numericInterval)
        {
            openApiSchema.Minimum = numericInterval.Minimum;
            openApiSchema.ExclusiveMinimum = numericInterval.ExclusiveMinimum;
            openApiSchema.Maximum = numericInterval.Maximum;
            openApiSchema.ExclusiveMaximum = numericInterval.ExclusiveMaximum;
        }

        if (schema.GetStringMinLength() is { } stringMinLength)
        {
            openApiSchema.MinLength = stringMinLength.MinLength;
        }

        if (schema.GetStringMaxLength() is { } stringMaxLength)
        {
            openApiSchema.MaxLength = stringMaxLength.MaxLength;
        }

        if (schema.GetStringFormat() is { } stringFormat)
        {
            openApiSchema.Format = stringFormat.Format;
        }

        if (schema.GetStringPattern() is { } stringPattern)
        {
            openApiSchema.Pattern = stringPattern.Expression;
        }

        return openApiSchema;
    }

    private string JsonConverterFunc(object value)
    {
        return JsonSerializer.Serialize(value, _serializerOptions);
    }
}

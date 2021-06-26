// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using MicroElements.Metadata.Schema;
using MicroElements.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.Swashbuckle
{
    /// <summary>
    /// Creates properties for <see cref="IPropertyContainer"/> according attached <see cref="IPropertySet"/>.
    /// Use <see cref="PropertySetAttribute"/> to attach property list to <see cref="IPropertyContainer"/>.
    /// </summary>
    public class PropertyContainerSchemaFilter : ISchemaFilter
    {
        private readonly PropertyContainerSchemaFilterOptions _options;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly SchemaGeneratorOptions _schemaGeneratorOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerSchemaFilter"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="serializerOptions">JsonSerializerOptions.</param>
        /// <param name="generatorOptions">Swagger generator options.</param>
        public PropertyContainerSchemaFilter(
            PropertyContainerSchemaFilterOptions? options,
            IOptions<JsonSerializerOptions> serializerOptions,
            SwaggerGenOptions generatorOptions)
        {
            _options = options?.Clone() ?? new PropertyContainerSchemaFilterOptions();
            _serializerOptions = serializerOptions.Value;
            _schemaGeneratorOptions = generatorOptions.SchemaGeneratorOptions;

            Func<string, string>? resolvePropertyName = options?.ResolvePropertyName;
            resolvePropertyName ??= propertyName => _serializerOptions.PropertyNamingPolicy.ConvertName(propertyName);
            resolvePropertyName ??= propertyName => propertyName;

            _options.ResolvePropertyName = resolvePropertyName;
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
                if (_options.GenerateKnownSchemasAsRefs && propertySet != null)
                {
                    knownSchemaType ??= propertySet.GetType();
                    string knownSchemaId = _schemaGeneratorOptions.SchemaIdSelector(knownSchemaType);

                    if (!context.SchemaRepository.Schemas.TryGetValue(knownSchemaId, out OpenApiSchema knownSchema))
                    {
                        // Generate and fill knownSchema once for type.
                        knownSchema = context.SchemaGenerator.GenerateSchema(knownSchemaType, context.SchemaRepository);
                        FillSchema(knownSchema, context, propertySet);
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
                    FillSchema(schema, context, propertySet);
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

        private string JsonConverterFunc(object value)
        {
            return JsonSerializer.Serialize(value, _serializerOptions);
        }

        private void FillSchema(OpenApiSchema schema, SchemaFilterContext context, IPropertySet propertySet)
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

                if (property.GetAllowedValuesUntyped() is { } allowedValues)
                {
                    propertySchema.Enum = new List<IOpenApiAny>();
                    foreach (object allowedValue in allowedValues.ValuesUntyped)
                    {
                        string jsonValue = JsonConverterFunc(allowedValue);
                        IOpenApiAny openApiAny = OpenApiAnyFactory.CreateFromJson(jsonValue);
                        propertySchema.Enum.Add(openApiAny);
                    }
                }

                string? propertyName = _options.ResolvePropertyName!(property.Name);
                schema.Properties.Add(propertyName, propertySchema);
            }
        }
    }
}

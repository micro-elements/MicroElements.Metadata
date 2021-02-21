// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainerSchemaFilter"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public PropertyContainerSchemaFilter(PropertyContainerSchemaFilterOptions? options)
        {
            _options = new PropertyContainerSchemaFilterOptions
            {
                ResolvePropertyName = options?.ResolvePropertyName ?? (propertyName => propertyName),
            };
        }

        /// <inheritdoc />
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsAssignableTo<IPropertyContainer>())
            {
                schema.Type = "object";
                schema.Items = null;
                schema.Properties = new Dictionary<string, OpenApiSchema>();

                IPropertySet? propertySet = null;

                // Get by provided attribute [PropertySet].
                propertySet = context.MemberInfo?.GetCustomAttribute<PropertySetAttribute>()?.GetPropertySet();

                if (propertySet == null)
                {
                    // Get by IKnownPropertySet
                    propertySet = context.Type.GetSchemaByKnownPropertySet();
                }

                if (propertySet != null)
                {
                    foreach (IProperty property in propertySet)
                    {
                        var propertySchema = context.SchemaGenerator.GenerateSchema(property.Type, context.SchemaRepository);
                        propertySchema.Description = property.Description ?? propertySchema.Description;

                        if (property.GetOrEvaluateNullability() is { } allowNull)
                        {
                            propertySchema.Nullable = allowNull.IsNullAllowed;
                        }

                        if (property.GetAllowedValuesUntyped() is { } allowedValues)
                        {
                            propertySchema.Enum = new List<IOpenApiAny>();
                            foreach (object allowedValue in allowedValues.ValuesUntyped)
                            {
                                IOpenApiAny openApiAny = OpenApiAnyFactory.CreateFor(propertySchema, allowedValue);
                                propertySchema.Enum.Add(openApiAny);
                            }
                        }

                        var propertyName = _options.ResolvePropertyName!(property.Name);
                        schema.Properties.Add(propertyName, propertySchema);
                    }
                }
            }
        }
    }
}

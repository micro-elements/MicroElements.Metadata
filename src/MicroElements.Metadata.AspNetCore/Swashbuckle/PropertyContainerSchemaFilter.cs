// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Reflection;
using MicroElements.Functional;
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

                PropertySetAttribute? propertySetAttribute = context.MemberInfo.GetCustomAttribute<PropertySetAttribute>();
                if (propertySetAttribute != null)
                {
                    IPropertySet? propertySet = propertySetAttribute.GetPropertySet();
                    if (propertySet != null)
                    {
                        foreach (IProperty property in propertySet)
                        {
                            var propertySchema = context.SchemaGenerator.GenerateSchema(property.Type, context.SchemaRepository);
                            propertySchema.Description = property.Description?.Text ?? propertySchema.Description;

                            var propertyName = _options.ResolvePropertyName!(property.Name);
                            schema.Properties.Add(propertyName, propertySchema);
                        }
                    }
                }
            }
        }
    }
}

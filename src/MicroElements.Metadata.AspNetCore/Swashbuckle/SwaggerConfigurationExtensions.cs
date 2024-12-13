// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text.Json;
using MicroElements.Metadata.AspNetCore;
using MicroElements.Metadata.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.Swashbuckle
{
    /// <summary>
    /// Extensions for DI.
    /// </summary>
    public static class SwaggerConfigurationExtensions
    {
        /// <summary>
        /// Configures swagger to properly generate info for <see cref="IPropertyContainer"/>.
        /// If you want to configure filter than use <see cref="ConfigureSwaggerForPropertyContainers"/>.
        /// </summary>
        /// <param name="swagger"><see cref="SwaggerGenOptions"/> instance.</param>
        /// <returns>The same options.</returns>
        public static SwaggerGenOptions ConfigureForPropertyContainers(this SwaggerGenOptions swagger)
        {
            bool containsFilter = swagger.SchemaFilterDescriptors.FirstOrDefault(descriptor => descriptor.Type == typeof(PropertyContainerSchemaFilter)) != null;

            if (!containsFilter)
                swagger.SchemaFilter<PropertyContainerSchemaFilter>(swagger);

            return swagger;
        }

        /// <summary>
        /// Configures swagger to properly generate info for <see cref="IPropertyContainer"/>.
        /// By default uses Microsoft.AspNetCore.Mvc.JsonOptions registered in AspNetCore but allows to configure manually.
        /// </summary>
        /// <param name="services">Source services.</param>
        /// <param name="configure">Allows to configure options.</param>
        public static void ConfigureSwaggerForPropertyContainers(
            this IServiceCollection services,
            Action<PropertyContainerSchemaFilterOptions>? configure = null)
        {
            services.ConfigureOptions<ConfigurePropertyContainerSchemaFilterOptions>();
            if (configure != null)
                services.Configure(configure);

            // Configure swagger
            services.ConfigureSwaggerGen(swagger => swagger.ConfigureForPropertyContainers());
        }
    }

    internal sealed class ConfigurePropertyContainerSchemaFilterOptions(IServiceProvider serviceProvider)
        : IConfigureNamedOptions<PropertyContainerSchemaFilterOptions>
    {
        /// <inheritdoc />
        public void Configure(PropertyContainerSchemaFilterOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Configure(string? name, PropertyContainerSchemaFilterOptions options)
        {
            JsonSerializerOptions jsonOptions = serviceProvider.GetJsonSerializerOptionsOrDefault();
            options.ResolvePropertyName = ResolvePropertyName;

            string ResolvePropertyName(string propertyName)
            {
                string resolvedPropertyName = jsonOptions is { PropertyNamingPolicy: { } propertyNamingPolicy }
                    ? propertyNamingPolicy.ConvertName(propertyName)
                    : propertyName;
                return resolvedPropertyName;
            }
        }
    }
}

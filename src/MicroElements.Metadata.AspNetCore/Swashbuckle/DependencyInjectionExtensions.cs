// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using MicroElements.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.Swashbuckle
{
    /// <summary>
    /// Extensions for DI.
    /// </summary>
    public static class DependencyInjectionExtensions
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
                swagger.SchemaFilter<PropertyContainerSchemaFilter>();

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
            if (configure != null)
            {
                services.Configure(configure);

                services.TryAddTransient(provider =>
                {
                    var options = provider.GetService<IOptions<PropertyContainerSchemaFilterOptions>>();
                    return options?.Value ?? new PropertyContainerSchemaFilterOptions();
                });
            }
            else
            {
                services.TryAddTransient(provider =>
                {
                    JsonSerializerOptions? jsonSerializerOptions = null;

                    Type? jsonOptionsType = TypeCache.Default.Value.GetByFullName("Microsoft.AspNetCore.Mvc.JsonOptions");
                    if (jsonOptionsType != null)
                    {
                        // IOptions<JsonOptions>
                        Type jsonOptionsInterfaceType = typeof(IOptions<>).MakeGenericType(jsonOptionsType);
                        object? jsonOptionsOption = provider.GetService(jsonOptionsInterfaceType);

                        if (jsonOptionsOption != null)
                        {
                            PropertyInfo? valueProperty = jsonOptionsInterfaceType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                            PropertyInfo? jsonSerializerOptionsProperty = jsonOptionsType.GetProperty("JsonSerializerOptions", BindingFlags.Instance | BindingFlags.Public);

                            if (valueProperty != null && jsonSerializerOptionsProperty != null)
                            {
                                // JsonOptions
                                var jsonOptions = valueProperty.GetValue(jsonOptionsOption);

                                // JsonSerializerOptions
                                if (jsonOptions != null)
                                    jsonSerializerOptions = jsonSerializerOptionsProperty.GetValue(jsonOptions) as JsonSerializerOptions;
                            }
                        }
                    }

                    return new PropertyContainerSchemaFilterOptions().ConfigureFromJsonOptions(jsonSerializerOptions);
                });
            }

            services.ConfigureSwaggerGen(swagger => swagger.ConfigureForPropertyContainers());
        }

        /// <summary>
        /// Configures <see cref="PropertyContainerSchemaFilterOptions"/> from <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="options">Options to configure.</param>
        /// <param name="jsonOptions">JsonSerializerOptions.</param>
        /// <returns>The same options.</returns>
        public static PropertyContainerSchemaFilterOptions ConfigureFromJsonOptions(
            this PropertyContainerSchemaFilterOptions options,
            JsonSerializerOptions? jsonOptions)
        {
            options.ResolvePropertyName = jsonOptions.ResolvePropertyNameByJsonSerializerOptions;
            return options;
        }

        private static string ResolvePropertyNameByJsonSerializerOptions(this JsonSerializerOptions? jsonOptions, string propertyName)
        {
            return jsonOptions is { PropertyNamingPolicy: { } propertyNamingPolicy }
                ? propertyNamingPolicy.ConvertName(propertyName)
                : propertyName;
        }
    }
}

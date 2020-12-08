// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using MicroElements.Metadata.Swashbuckle;
using MicroElements.Metadata.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.Metadata.AspNetCore
{
    /// <summary>
    /// Extensions for DI.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// 1. Configures <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.
        /// 2. Configures swagger to properly generate info for <see cref="IPropertyContainer"/>.
        ///    By default uses Microsoft.AspNetCore.Mvc.JsonOptions registered in AspNetCore but allows to configure manually.
        /// </summary>
        /// <param name="services">Source services.</param>
        /// <param name="configureSwaggerOptions">Allows to configure <see cref="PropertyContainerSchemaFilterOptions"/>.</param>
        public static void ConfigureForPropertyContainers(
            this IServiceCollection services,
            Action<PropertyContainerSchemaFilterOptions>? configureSwaggerOptions = null)
        {
            // Allows to serialize property containers
            services.Configure<JsonSerializerOptions>(options => options.ConfigureJsonForPropertyContainers());

            // Allows to use property containers in swagger
            services.ConfigureSwaggerForPropertyContainers(configureSwaggerOptions);
        }
    }
}

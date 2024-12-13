// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroElements.Metadata.AspNetCore
{
    /// <summary>
    /// DependencyInjection.
    /// </summary>
    public static class ReflectionDependencyInjectionExtensions
    {
        public static void ConfigureJsonOptionsForAspNetCore(this IServiceCollection services, Action<JsonSerializerOptions> configureJson)
        {
            services.Configure<JsonOptions>(options => configureJson(options.JsonSerializerOptions));
        }

        public static void ConfigureJsonOptionsForMinimalApi(this IServiceCollection services, Action<JsonSerializerOptions> configureJson)
        {
            services.ConfigureHttpJsonOptions(options => configureJson(options.SerializerOptions));
        }

        public static JsonSerializerOptions GetJsonSerializerOptionsOrDefault(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions
                   ?? serviceProvider.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()?.Value.SerializerOptions
                   ?? new JsonSerializerOptions();
        }
    }
}

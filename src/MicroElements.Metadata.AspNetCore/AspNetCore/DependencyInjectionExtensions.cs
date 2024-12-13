// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text.Json;
using MicroElements.Metadata.Serialization;
using MicroElements.Metadata.Swashbuckle;
using MicroElements.Metadata.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroElements.Metadata.AspNetCore
{
    /// <summary>
    /// Extensions for DI.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// <para>1. Configures <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.</para>
        /// <para>2. Configures AspNetCore <see cref="JsonSerializerOptions"/> to serialize <see cref="IPropertyContainer"/>.</para>
        /// <para>3. Configures swagger to properly generate schema for <see cref="IPropertyContainer"/>.
        ///    - By default uses Microsoft.AspNetCore.Mvc.JsonOptions registered in AspNetCore but allows to configure manually.
        ///    - Uses <see cref="PropertySetAttribute"/> to determine <see cref="IPropertySet"/> for schema.
        ///    - Uses <see cref="IKnownPropertySet{TPropertySet}"/> to determine <see cref="IPropertySet"/> for schema.</para>
        /// </summary>
        /// <param name="services">Source services.</param>
        /// <param name="configureMetadataJson">Configure Metadata Json serialization.</param>
        /// <param name="configureSwaggerOptions">Allows to configure <see cref="PropertyContainerSchemaFilterOptions"/>.</param>
        /// <returns>The same <paramref name="services"/> instance.</returns>
        public static IServiceCollection AddMetadata(
            this IServiceCollection services,
            Action<MetadataJsonSerializationOptions>? configureMetadataJson = null,
            Action<PropertyContainerSchemaFilterOptions>? configureSwaggerOptions = null)
        {
            // Configure Metadata Json serialization
            configureMetadataJson ??= options => { };
            services.Configure<MetadataJsonSerializationOptions>(configureMetadataJson);

            // Configures System.Text.Json serialization for AspNetCore and MinimalApi
            services.ConfigureJsonOptionsForAspNetCore(options => options.ConfigureJsonForPropertyContainers(configureMetadataJson));
            services.ConfigureJsonOptionsForMinimalApi(options => options.ConfigureJsonForPropertyContainers(configureMetadataJson));

            // Register wrapper for AspNetCore json options.
            services.ConfigureOptions<ConfigureAspNetJsonSerializerOptions>();

            // Allows to use property containers in swagger
            services.ConfigureSwaggerForPropertyContainers(configureSwaggerOptions);

            // Register ISerializerDataContractResolver (if not registered already)
            services.TryAddTransient<ISerializerDataContractResolver>(provider =>
            {
                var serializerOptions = provider.GetJsonSerializerOptionsOrDefault();
                return new JsonSerializerDataContractResolver(serializerOptions);
            });

            // Decorate ISerializerDataContractResolver with MetadataSerializerBehavior
            services.Decorate<ISerializerDataContractResolver>(resolver => new MetadataSerializerBehavior(resolver));

            return services;
        }

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

        internal static IServiceCollection Decorate<TService>(
            this IServiceCollection services,
            Func<TService, TService> decorate)
            where TService : class
        {
            ServiceDescriptor? serviceToDecorate = services
                .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));

            if (serviceToDecorate != null)
            {
                ServiceDescriptor CreateServiceDescriptor(Func<IServiceProvider, TService> func) =>
                    ServiceDescriptor.Describe(typeof(TService), func, serviceToDecorate.Lifetime);

                if (serviceToDecorate.ImplementationFactory != null)
                {
                    services.Replace(CreateServiceDescriptor(provider =>
                    {
                        var service = (TService)serviceToDecorate.ImplementationFactory(provider);
                        return decorate(service);
                    }));
                }
                else if (serviceToDecorate.ImplementationInstance != null)
                {
                    services.Replace(CreateServiceDescriptor(provider =>
                    {
                        var service = (TService)serviceToDecorate.ImplementationInstance;
                        return decorate(service);
                    }));
                }
                else if (serviceToDecorate.ImplementationType != null)
                {
                    services.Replace(CreateServiceDescriptor(provider =>
                    {
                        var service = (TService)ActivatorUtilities.CreateInstance(provider, serviceToDecorate.ImplementationType);
                        return decorate(service);
                    }));
                }
            }

            return services;
        }
    }
}

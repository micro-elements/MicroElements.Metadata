// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.Json;
using MicroElements.Metadata.AspNetCore;
using MicroElements.Metadata.ComponentModel;
using Microsoft.Extensions.Options;

namespace MicroElements.Metadata.Swashbuckle;

/// <summary>
/// Options to configure <see cref="PropertyContainerSchemaFilter"/>.
/// </summary>
public class PropertyContainerSchemaFilterOptions : ICloneable<PropertyContainerSchemaFilterOptions>
{
    /// <summary>
    /// Gets or sets a function that resolves property name by proper naming strategy.
    /// </summary>
    public Func<string, string>? ResolvePropertyName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether known schemas should be generated as separate schemas and should be references by ref.
    /// </summary>
    public bool GenerateKnownSchemasAsRefs { get; set; } = true;
}

internal sealed class ConfigurePropertyContainerSchemaFilterOptions(IServiceProvider serviceProvider)
    : IConfigureNamedOptions<PropertyContainerSchemaFilterOptions>
{
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

    /// <inheritdoc />
    public void Configure(PropertyContainerSchemaFilterOptions options) => Configure(null, options);
}

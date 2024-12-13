using System;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MicroElements.Metadata.AspNetCore;

/// <summary>
/// JsonSerializerOptions for Metadata serialization. Value wrapper that can be used in netstandard.
/// </summary>
public class AspNetJsonSerializerOptions
{
    /// <summary>
    /// Gets the <see cref="System.Text.Json.JsonSerializerOptions"/> from AspNet host.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
}

internal sealed class ConfigureAspNetJsonSerializerOptions(IServiceProvider serviceProvider)
    : IConfigureNamedOptions<AspNetJsonSerializerOptions>
{
    /// <inheritdoc />
    public void Configure(string? name, AspNetJsonSerializerOptions options)
    {
        JsonSerializerOptions jsonOptions = serviceProvider.GetJsonSerializerOptionsOrDefault();
        options.JsonSerializerOptions = jsonOptions;
    }

    /// <inheritdoc />
    public void Configure(AspNetJsonSerializerOptions options) => Configure(null, options);
}

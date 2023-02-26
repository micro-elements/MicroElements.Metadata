using MicroElements.Collections.TwoLayerCache;

namespace MicroElements.Metadata;

public static partial class MetadataProviderExtensions
{
    /// <summary>
    /// Cached version of <see cref="GetMetadata{TMetadata}"/>.
    /// </summary>
    /// <inheritdoc cref="GetMetadata{TMetadata}"/>
    public static TMetadata? GetMetadataCached<TMetadata>(
        this IMetadataProvider metadataProvider,
        string? metadataName = null,
        TMetadata? defaultValue = default,
        bool searchInSchema = false)
    {
        return TwoLayerCache
            .Instance<(IMetadataProvider MetadataProvider, string? MetadataName, bool SearchInSchema), TMetadata?>()
            .GetOrAdd((metadataProvider, metadataName, searchInSchema), (key, def) =>
                    key.MetadataProvider.GetMetadata<TMetadata>(key.MetadataName, searchInSchema: key.SearchInSchema, defaultValue: def), defaultValue);
    }
}

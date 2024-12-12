using System.Diagnostics;

namespace MicroElements.Metadata;

/// <summary>
/// MetadataProvider с неявным свойством Metadata
/// (реализация по умолчанию, свойство доступно только через интерфейс <see cref="IManualMetadataProvider"/>).
/// Особенности:
/// 1. По умолчанию _metadata не создается для экономии памяти
/// 2. При присвоении Metadata сохраняется как ReadOnly metadata, что позволяет избежать копирования в некоторых сценариях
/// </summary>
[DebuggerTypeProxy(typeof(MetadataProviderDebugView))]
public record PrivateMetadataProvider : IManualMetadataProvider
{
    protected static readonly IPropertyContainer _empty = new PropertyContainer(searchOptions: MetadataProvider.DefaultSearchOptions);
    protected IPropertyContainer? _metadata;

    /// <inheritdoc />
    IPropertyContainer IManualMetadataProvider.Metadata => _metadata ?? _empty;

    /// <inheritdoc />
    void IMetadataProvider.SetMetadataContainer(IPropertyContainer metadata) => SetMetadataField(metadata);

    protected void SetMetadataField(IPropertyContainer metadata)
    {
        if (metadata.Count > 0)
        {
            if (metadata is IMutablePropertyContainer || !metadata.SearchOptions.Equals(_empty.SearchOptions))
            {
                // Create readonly copy with needed SearchOptions
                _metadata = new PropertyContainer(metadata, searchOptions: _empty.SearchOptions);
            }
            else
            {
                // Already readonly with the needed SearchOptions
                _metadata = metadata;
            }
        }
        else
        {
            _metadata = metadata;
        }
    }
}

/// <summary>
/// MetadataProvider с явным публичным свойством Metadata.
/// Особенности:
/// 1. По умолчанию _metadata не создается для экономии памяти
/// 2. При присвоении Metadata сохраняется как ReadOnly metadata, что позволяет избежать копирования в некоторых сценариях
/// </summary>
public record PublicMetadataProvider : PrivateMetadataProvider
{
    /// <inheritdoc cref="IManualMetadataProvider" />
    public IPropertyContainer Metadata
    {
        get => _metadata ?? _empty;
        init => SetMetadataField(value);
    }
}

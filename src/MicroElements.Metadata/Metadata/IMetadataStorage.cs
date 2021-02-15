namespace MicroElements.Metadata
{
    public interface IMetadataStorage
    {
        /// <summary>
        /// Gets metadata for the current instance.
        /// </summary>
        /// <param name="autoCreate">Should create metadata if it was not created before.</param>
        /// <returns>Metadata for instance.</returns>
        IPropertyContainer GetInstanceMetadata(object instance, bool autoCreate = true);

        /// <summary>
        /// Replaces metadata for the current instance.
        /// </summary>
        /// <param name="metadata">New metadata.</param>
        void SetInstanceMetadata(object instance, IPropertyContainer metadata);
    }

    public sealed class MetadataGlobalCacheStorage : IMetadataStorage
    {
        public static readonly IMetadataStorage Instance = new MetadataGlobalCacheStorage();

        /// <inheritdoc />
        public IPropertyContainer GetInstanceMetadata(object instance, bool autoCreate = true) => MetadataGlobalCache.GetInstanceMetadata(instance, autoCreate);

        /// <inheritdoc />
        public void SetInstanceMetadata(object instance, IPropertyContainer metadata) => MetadataGlobalCache.SetInstanceMetadata(instance, metadata);
    }
}

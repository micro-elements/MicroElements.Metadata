// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents <see cref="IMetadataProvider"/> wrapper for any object.
    /// If object is <see cref="IMetadataProvider"/> then returns <see cref="IMetadataProvider.GetMetadataContainer"/>,
    /// otherwise returns <see cref="MetadataGlobalCache.GetInstanceMetadata"/>.
    /// </summary>
    public readonly struct MetadataProviderWrapper : IMetadataProvider
    {
        private readonly object _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataProviderWrapper"/> struct.
        /// </summary>
        /// <param name="instance">Object instance.</param>
        public MetadataProviderWrapper(object instance)
        {
            instance.AssertArgumentNotNull(nameof(instance));
            _instance = instance;
        }

        /// <summary>
        /// Gets instance metadata.
        /// </summary>
        public IPropertyContainer Metadata => GetMetadataContainer(autoCreate: false);

        /// <inheritdoc />
        public IPropertyContainer GetMetadataContainer(bool autoCreate = false)
        {
            return _instance switch
            {
                IMetadataProvider metadataProvider => metadataProvider.GetMetadataContainer(autoCreate),
                _ => _instance.GetInstanceMetadata(autoCreate),
            };
        }

        /// <inheritdoc />
        public void SetMetadataContainer(IPropertyContainer metadata)
        {
            if (_instance is IMetadataProvider metadataProvider)
            {
                metadataProvider.SetMetadataContainer(metadata);
                return;
            }

            _instance.SetInstanceMetadata(metadata);
        }
    }

    public readonly struct CompositeMetadataProvider : IMetadataProvider
    {
        private readonly IMetadataProvider _firstMetadataProvider;
        private readonly IMetadataProvider _secondMetadataProvider;

        public CompositeMetadataProvider(IMetadataProvider firstMetadataProvider, IMetadataProvider secondMetadataProvider)
        {
            _firstMetadataProvider = firstMetadataProvider;
            _secondMetadataProvider = secondMetadataProvider;
        }

        /// <inheritdoc />
        public IPropertyContainer GetMetadataContainer(bool autoCreate = false)
        {
            IPropertyContainer metadataContainer1 = _firstMetadataProvider.GetMetadataContainer(autoCreate);
            IPropertyContainer metadataContainer2 = _secondMetadataProvider.GetMetadataContainer(autoCreate);

            return new PropertyContainer(metadataContainer1.Properties, parentPropertySource: metadataContainer2);
        }

        /// <inheritdoc />
        public void SetMetadataContainer(IPropertyContainer metadata)
        {
            throw new System.NotImplementedException();
        }
    }
}

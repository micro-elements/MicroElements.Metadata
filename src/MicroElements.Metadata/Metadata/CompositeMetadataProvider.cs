// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
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

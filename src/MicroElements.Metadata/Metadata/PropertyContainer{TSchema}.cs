// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents <see cref="IPropertyContainer"/> with schema.
    /// </summary>
    /// <typeparam name="TSchema">Schema type.</typeparam>
    public class PropertyContainer<TSchema> : PropertyContainer, IKnownPropertySet<TSchema>
        where TSchema : IPropertySet, new()
    {
        /// <summary>
        /// Gets empty container of desired type.
        /// </summary>
        public static new PropertyContainer<TSchema> Empty { get; } = new PropertyContainer<TSchema>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer{TSchema}"/> class.
        /// </summary>
        /// <param name="sourceValues">Source property values.</param>
        /// <param name="parentPropertySource">Parent property source.</param>
        /// <param name="searchOptions">Property search options.</param>
        public PropertyContainer(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IPropertyContainer? parentPropertySource = null,
            SearchOptions? searchOptions = null)
            : base(sourceValues, parentPropertySource, searchOptions)
        {
        }
    }
}

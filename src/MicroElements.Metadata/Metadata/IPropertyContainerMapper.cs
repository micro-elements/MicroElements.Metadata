// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents mapper to and from <see cref="IPropertyContainer"/>.
    /// </summary>
    public interface IPropertyContainerMapper
    {
    }

    /// <summary>
    /// Represents mapper to and from <see cref="IPropertyContainer"/>.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    public interface IPropertyContainerMapper<T> : IPropertyContainerMapper
    {
        /// <summary>
        /// Maps model to <see cref="IPropertyContainer"/>.
        /// </summary>
        /// <param name="model">Source model.</param>
        /// <returns><see cref="IPropertyContainer"/> for <paramref name="model"/>.</returns>
        IPropertyContainer ToContainer(T model);

        /// <summary>
        /// Maps <paramref name="container"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="container">Source property container.</param>
        /// <returns>Model.</returns>
        T ToModel(IPropertyContainer container);
    }
}

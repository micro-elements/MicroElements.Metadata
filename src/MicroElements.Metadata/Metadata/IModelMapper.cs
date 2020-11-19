// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents mapper to and from <see cref="IPropertyContainer"/>.
    /// </summary>
    public interface IModelMapper
    {
        /// <summary>
        /// Gets type that can be mapped.
        /// </summary>
        Type ModelType { get; }
    }

    /// <summary>
    /// Represents mapper to and from <see cref="IPropertyContainer"/>.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    public interface IModelMapper<T> : IModelMapper
    {
        /// <inheritdoc />
        Type IModelMapper.ModelType => typeof(T);

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

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.CodeContracts;
using MicroElements.Collections.TwoLayerCache;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Metadata.Schema;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public interface INameAlias
    {
        /// <summary>
        /// Gets an alternative name for the object.
        /// </summary>
        string? Alias { get; }
    }

    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public class NameAlias : INameAlias
    {
        /// <inheritdoc />
        public string? Alias { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameAlias"/> class.
        /// </summary>
        /// <param name="alias">Alias name.</param>
        public NameAlias(string? alias)
        {
            Alias = alias;
        }

        /// <summary>
        /// Gets cached <see cref="INameAlias"/>.
        /// </summary>
        /// <param name="alias">Description.</param>
        /// <returns>Cached description.</returns>
        public static INameAlias FromStringCached(string alias)
        {
            return TwoLayerCache
                .Instance<string, INameAlias>(nameof(INameAlias))
                .GetOrAdd(alias, s => new NameAlias(s));
        }
    }

    /// <summary>
    /// Extensions for <seealso cref="INameAlias"/>.
    /// </summary>
    public static class AliasExtensions
    {
        /// <summary>
        /// Sets alias for object.
        /// </summary>
        /// <param name="value">Target object.</param>
        /// <param name="nameAlias">Alias name for the object.</param>
        /// <returns>The same metadata provider.</returns>
        public static IMetadataProvider SetAlias(this IMetadataProvider value, INameAlias nameAlias)
        {
            return value.SetMetadata(Assertions.AssertArgumentNotNull(nameAlias, nameof(nameAlias)));
        }

        /// <summary>
        /// Gets Alias from <see cref="INameAlias"/>.
        /// </summary>
        /// <param name="value">Source value.</param>
        /// <returns>Optional alias for object.</returns>
        public static string? GetAlias(this object value)
        {
            return value.GetSelfOrComponent<INameAlias>()?.Alias;
        }
    }
}

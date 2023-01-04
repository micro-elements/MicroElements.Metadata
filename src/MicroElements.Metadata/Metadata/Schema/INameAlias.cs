// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Collections.TwoLayerCache;
using MicroElements.Metadata.ComponentModel;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public interface INameAlias : IMetadata
    {
        /// <summary>
        /// Gets an alternative name for the object.
        /// </summary>
        string? Alias { get; }
    }

    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public sealed class NameAlias : INameAlias
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
        /// Sets alias metadata for the object.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="target">Target object.</param>
        /// <param name="nameAlias">Description.</param>
        /// <returns>The same metadata provider.</returns>
        public static TSchema SetNameAliasMetadata<TSchema>(this TSchema target, INameAlias nameAlias)
            where TSchema : IMetadataProvider
        {
            return target.SetMetadata(nameAlias);
        }

        /// <summary>
        /// Creates schema copy with provided name alias.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="source">Source schema.</param>
        /// <param name="nameAlias">Description.</param>
        /// <returns>New schema instance with provided name alias.</returns>
        public static TSchema WithNameAlias<TSchema>(this TSchema source, INameAlias nameAlias)
            where TSchema : ISchemaBuilder<INameAlias>, ISchema
        {
            return source.WithSchemaComponent(nameAlias);
        }

        /// <summary>
        /// Gets Alias from <see cref="INameAlias"/>.
        /// </summary>
        /// <param name="value">Source value.</param>
        /// <returns>Optional alias for object.</returns>
        public static string? GetAlias(this IMetadataProvider value)
        {
            return value.GetSelfOrComponent<INameAlias>()?.Alias;
        }
    }
}

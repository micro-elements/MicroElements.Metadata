// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public interface IHasAlias
    {
        /// <summary>
        /// Gets an alternative name for the object.
        /// </summary>
        string? Alias { get; }
    }

    /// <summary>
    /// Represents object that has Alias.
    /// </summary>
    public class HasAlias : IHasAlias
    {
        /// <inheritdoc />
        public string? Alias { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasAlias"/> class.
        /// </summary>
        /// <param name="alias">Alias name.</param>
        public HasAlias(string? alias)
        {
            Alias = alias;
        }
    }

    /// <summary>
    /// Extensions for <seealso cref="IHasAlias"/>.
    /// </summary>
    public static class AliasExtensions
    {
        /// <summary>
        /// Sets alias for object.
        /// </summary>
        /// <param name="value">Target object.</param>
        /// <param name="hasAlias">Alias name for the object.</param>
        /// <returns>The same metadata provider.</returns>
        public static IMetadataProvider SetAlias(this IMetadataProvider value, IHasAlias hasAlias)
        {
            return value.SetMetadata(hasAlias);
        }

        /// <summary>
        /// Gets Alias from <see cref="IHasAlias"/>.
        /// </summary>
        /// <param name="value">Source value.</param>
        /// <returns>Optional alias for object.</returns>
        public static string? GetAlias(this object value)
        {
            if (value is IHasAlias { Alias: { } alias })
                return alias;
            if (value is IMetadataProvider metadataProvider)
            {
                IHasAlias? hasAlias = metadataProvider.GetMetadata<IHasAlias>();
                if (hasAlias?.Alias != null)
                    return hasAlias.Alias;
            }

            return null;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Reflection.FriendlyName;
using MicroElements.Reflection.TypeCaching;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Maps type name to <see cref="Type"/> and back.
    /// </summary>
    public interface ITypeMapper
    {
        /// <summary>
        /// Gets type name.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>Type name.</returns>
        string GetTypeName(Type type);

        /// <summary>
        /// Gets type by name.
        /// It should work with all type names generated with <see cref="GetTypeName"/>.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <returns>Type.</returns>
        Type? GetTypeByName(string typeName);
    }

    /// <summary>
    /// Uses <see cref="FriendlyName.GetFriendlyName"/>.
    /// NumericTypes: byte, short, int, long, float, double, decimal, sbyte, ushort, uint, ulong.
    /// NullableTypes: byte?, short?, int?, long?, float?, double?, decimal?, sbyte?, ushort?, uint?, ulong?.
    /// ArrayTypes: int[], string[], etc.
    /// </summary>
    public class DefaultTypeMapper : ITypeMapper
    {
        /// <summary>
        /// Gets static instance.
        /// </summary>
        public static DefaultTypeMapper Instance { get; } = new();

        private readonly ITypeCache _typeCache;

        private DefaultTypeMapper()
        {
            var stdTypeAliases = new TypeCache(typeAliases: FriendlyName.StandardTypeAliases);
            var allDomainTypes = TypeCache.AppDomainTypesUpdatable;
            var typeCache = stdTypeAliases.WithParent(allDomainTypes);
            _typeCache = typeCache;
        }

        /// <inheritdoc />
        public string GetTypeName(Type type)
        {
            return type.GetFriendlyName(_typeCache);
        }

        /// <inheritdoc />
        public Type GetTypeByName(string typeName)
        {
            return typeName.ParseFriendlyName(_typeCache) ?? typeof(object);
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using MicroElements.Shared;

namespace MicroElements.Metadata.Serialization
{
    /// <summary>
    /// Maps:
    /// NumericTypes: byte, short, int, long, float, double, decimal, sbyte, ushort, uint, ulong.
    /// NullableNumericTypes: byte?, short?, int?, long?, float?, double?, decimal?, sbyte?, ushort?, uint?, ulong?.
    /// OtherTypes: string, DateTime, DateTime?.
    /// ArrayTypes: int[], string[], etc.
    /// </summary>
    public class DefaultTypeMapper : ITypeMapper
    {
        /// <summary>
        /// Gets static instance.
        /// </summary>
        public static DefaultTypeMapper Instance { get; } = new DefaultTypeMapper();

        private readonly TypeCache _typeCache;

        private DefaultTypeMapper()
        {
            // NumericTypesWithNullable, NodaTimeTypes, string, DateTime, DateTime?
            var typeRegistrations = Enumerable.Empty<TypeRegistration>()
                .Concat(TypeCache.NumericTypesWithNullable.TypeSource.TypeRegistrations)
                .Concat(TypeCache.NodaTimeTypes.Value.TypeSource.TypeRegistrations)
                .Concat(new[]
                {
                    new TypeRegistration(typeof(string), "string"),
                    new TypeRegistration(typeof(bool), "bool"),
                    new TypeRegistration(typeof(bool?), "bool?"),
                    new TypeRegistration(typeof(DateTime), "DateTime"),
                    new TypeRegistration(typeof(DateTime?), "DateTime?"),
                })
                .ToArray();

            // Array types for each registration
            var arrayTypes = typeRegistrations
                .Where(registration => !registration.Type.IsArray && registration.Alias != null)
                .Select(registration => new TypeRegistration(registration.Type.MakeArrayType(), $"{registration.Alias}[]"));

            typeRegistrations = typeRegistrations
                .Concat(arrayTypes)
                .ToArray();

            _typeCache = TypeCache.Create(
                AssemblySource.Default,
                TypeSource.Empty.With(typeRegistrations: typeRegistrations));
        }

        /// <inheritdoc />
        public string GetTypeName(Type type)
        {
            return (_typeCache.GetAliasForType(type) ?? type.FullName)!;
        }

        /// <inheritdoc />
        public Type? GetTypeByName(string typeName)
        {
            return _typeCache.GetByAliasOrFullName(typeName);
        }
    }
}

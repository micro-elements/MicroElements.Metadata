using System;
using System.Collections;
using System.Collections.Generic;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;
using MicroElements.Reflection.TypeExtensions;

namespace MicroElements.Metadata.JsonSchema
{
    public class JsonTypeMapper : ITypeMapper
    {
        public static JsonTypeMapper Instance { get; } = new JsonTypeMapper();

        /// <inheritdoc />
        public string GetTypeName(Type type)
        {
            if (type == null)
                return SimpleType.Null;

            if (type == typeof(string))
                return SimpleType.String;

            if (type == typeof(int))
                return SimpleType.Integer;

            if (type == typeof(double) || type == typeof(decimal))
                return SimpleType.Number;

            if (type == typeof(bool))
                return SimpleType.Boolean;

            if (type.IsArray || IsSupportedCollection(type, out var itemType))
                return SimpleType.Array;

            if (type == typeof(DateTime))
                return SimpleType.String;//format=date-time

            if (type.IsNullableStruct())
            {
                type = Nullable.GetUnderlyingType(type);
                return GetTypeName(type);
            }

            return SimpleType.Object;
        }

        public ISchema GetTypeNameExt(Type? type)
        {
            //TODO: support all types.

            if (type == null)
                return new NullTypeSchema();

            if (type == typeof(string))
            {
                return new SimpleTypeSchema(SimpleType.String, type);
            }

            if (type == typeof(int))
            {
                return new SimpleTypeSchema(SimpleType.Integer, type);
            }

            if (type == typeof(double) || type == typeof(decimal))
            {
                return new SimpleTypeSchema(SimpleType.Number, type);
            }

            if (type == typeof(bool))
            {
                return new SimpleTypeSchema(SimpleType.Boolean, type);
            }

            if (IsSupportedCollection(type, out Type itemType))
            {
                return new ArraySchema()
                {
                    Items = GetTypeNameExt(type: itemType)
                };
            }

            if (type == typeof(DateTime))
            {
                return new SimpleTypeSchema(SimpleType.String, type)
                    .SetStringFormat("date-time");
            }

            if (type.FullName == "NodaTime.LocalDate")
            {
                return new SimpleTypeSchema(SimpleType.String, type)
                    .SetStringFormat("date");
            }

            if (type.FullName == "NodaTime.LocalDateTime")
            {
                return new SimpleTypeSchema(SimpleType.String, type)
                    .SetStringFormat("date-time");
            }

            if (type.IsNullableStruct())
            {
                type = Nullable.GetUnderlyingType(type);
                ISchema baseType = GetTypeNameExt(type);
                baseType.SetAllowNull();
                return baseType;
            }

            return new MutableObjectSchema();
        }

        /// <inheritdoc />
        public Type? GetTypeByName(string typeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See JsonSerializerDataContractResolver from Swagger
        /// </summary>
        public bool IsSupportedCollection(Type type, out Type itemType)
        {
            if (type.IsConstructedFrom(typeof(IEnumerable<>), out Type constructedType))
            {
                itemType = constructedType.GenericTypeArguments[0];
                return true;
            }

            //#if (!NETSTANDARD2_0)
            //            if (type.IsConstructedFrom(typeof(IAsyncEnumerable<>), out constructedType))
            //            {
            //                itemType = constructedType.GenericTypeArguments[0];
            //                return true;
            //            }
            //#endif

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                itemType = typeof(object);
                return true;
            }

            itemType = null;
            return false;
        }
    }
}

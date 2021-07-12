using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicroElements.Functional;
using MicroElements.Metadata.Schema;
using MicroElements.Metadata.Serialization;

namespace MicroElements.Metadata.JsonSchema
{
    // "simpleTypes": {"enum": [ "array", "boolean", "integer", "null", "number", "object", "string" ] }
    public static class SimpleType
    {
        public static string Array = "array";
        public static string Boolean = "boolean";
        public static string Integer = "integer";
        public static string Null = "null";
        public static string Number = "number";
        public static string Object = "object";
        public static string String = "string";
    }

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
            if (type == null)
                return new NullTypeSchema();

            if (type == typeof(string))
                return new SimpleTypeSchema(SimpleType.String, type);

            if (type == typeof(int))
                return new SimpleTypeSchema(SimpleType.Integer, type);

            if (type == typeof(double) || type == typeof(decimal))
                return new SimpleTypeSchema(SimpleType.Number, type);

            if (type == typeof(bool))
                return new SimpleTypeSchema(SimpleType.Boolean, type);

            if (IsSupportedCollection(type, out Type itemType))
            {
                return new ArraySchema()
                {
                    Items = GetTypeNameExt(type: itemType)
                };
            }

            if (type == typeof(DateTime))
                return new SimpleTypeSchema(SimpleType.String, type)
                    .SetStringFormat("date-time");

            if (type.IsNullableStruct())
            {
                type = Nullable.GetUnderlyingType(type);
                return GetTypeNameExt(type);
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

    [Obsolete("move to ME.Reflection")]
    public static class TypeExt
    {
        public static Type[] GetInheritanceChain(this Type type)
        {
            var inheritanceChain = new List<Type>();

            var current = type;
            while (current.BaseType != null)
            {
                inheritanceChain.Add(current.BaseType);
                current = current.BaseType;
            }

            return inheritanceChain.ToArray();
        }

        public static bool IsConstructedFrom(this Type type, Type genericType, out Type constructedType)
        {
            constructedType = new[] { type }
                .Union(type.GetInheritanceChain())
                .Union(type.GetInterfaces())
                .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);

            return (constructedType != null);
        }
    }
}

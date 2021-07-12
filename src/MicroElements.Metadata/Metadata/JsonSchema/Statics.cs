using System;
using System.Collections.Generic;
using System.Linq;

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

    public class JsonSimpleType
    {
        public string Name { get; }

        public JsonSimpleType(string name)
        {
            Name = name;
        }

        public static JsonSimpleType Array = new JsonSimpleType("array");
        public static JsonSimpleType Boolean = new JsonSimpleType("boolean");
        public static JsonSimpleType Integer = new JsonSimpleType("integer");
        public static JsonSimpleType Null = new JsonSimpleType("null");
        public static JsonSimpleType Number = new JsonSimpleType("number");
        public static JsonSimpleType Object = new JsonSimpleType("object");
        public static JsonSimpleType String = new JsonSimpleType("string");
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

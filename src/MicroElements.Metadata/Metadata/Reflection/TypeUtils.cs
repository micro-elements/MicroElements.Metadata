using System;
using MicroElements.Reflection.TypeCaching;

namespace MicroElements.Metadata.Reflection
{
    internal static class TypeUtils
    {
        public static Type? GetByFullName(string fullTypeName)
        {
            Type? byFullName = TypeCache.AppDomainTypes.GetType(fullTypeName);
            return byFullName;
        }
    }
}

using System;
using MicroElements.Shared;

namespace MicroElements.Metadata.Functional
{
    internal static class TypeUtils
    {
        public static Type GetByFullName(string fullTypeName)
        {
            Type byFullName = TypeCache.Default.Value.GetByFullName(fullTypeName);
            Type type = Type.GetType(fullTypeName, throwOnError: false);
            return byFullName;
        }
    }
}

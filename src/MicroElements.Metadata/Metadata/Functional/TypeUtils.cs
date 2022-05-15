using System;
using MicroElements.Reflection;

namespace MicroElements.Metadata.Functional
{
    internal static class TypeUtils
    {
        public static Type GetByFullName(string fullTypeName)
        {
            Type byFullName = TypeCache.Default.Value.GetByFullName(fullTypeName);
            return byFullName;
        }
    }
}

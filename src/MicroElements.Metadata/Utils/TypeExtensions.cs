using System;

namespace MicroElements
{
    internal static class TypeExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
                return Activator.CreateInstance(type);
            else
                return null;
        }
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MicroElements.Core
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Checks that argument of an operation is not null.
        /// </summary>
        /// <typeparam name="T">Argument type.</typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="ArgumentNullException">The argument is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertArgumentNotNull<T>(this T? arg, string name)
        {
            if (arg is null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="value"/> is assignable to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="targetType">Type to check.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is assignable to <paramref name="targetType"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableTo(this object value, Type targetType)
        {
            return value.GetType().IsAssignableTo(targetType);
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="sourceType"/> is assignable to <paramref name="targetType"/>.
        /// </summary>
        /// <param name="sourceType">Source type.</param>
        /// <param name="targetType">Type to check.</param>
        /// <returns><c>true</c> if <paramref name="sourceType"/> is assignable to <paramref name="targetType"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableTo(this Type sourceType, Type targetType)
        {
            return targetType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo());
        }

        /// <summary>
        /// Returns a value indicating whether the type is a reference type.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>True if argument is a reference type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReferenceType(this Type type)
        {
            return !type.GetTypeInfo().IsValueType;
        }

        /// <summary>
        /// Returns a value indicating whether the type is a nullable struct.
        /// </summary>
        /// <param name="type">Source type.</param>
        /// <returns>True if argument is a nullable struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullableStruct(this Type type)
        {
            return type.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Returns <c>true</c> if <c>null</c> can be assigned to type.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <returns><c>true</c> if <c>null</c> can be assigned to type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanAcceptNull(this Type targetType)
        {
            return targetType.IsReferenceType() || targetType.IsNullableStruct();
        }

        /// <summary>
        /// Returns <c>true</c> if <c>null</c> can not be assigned to type.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <returns><c>true</c> if <c>null</c> can not be assigned to type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanNotAcceptNull(this Type targetType)
        {
            return !targetType.CanAcceptNull();
        }
    }
}

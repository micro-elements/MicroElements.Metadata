// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using MicroElements.Collections.TwoLayerCache;

namespace MicroElements.Metadata.ComponentModel
{
    /// <summary>
    /// Adds shallow clone functionality.
    /// Default implementation uses cached `MemberwiseClone` expression.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    public interface ICloneable<T> : ICloneable
    {
        /// <inheritdoc />
        object ICloneable.Clone() => Clone()!;

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        new T Clone()
        {
            // Fast type check (this should be of type T).
            T @this = (T)this;
            Func<T, T> cloneFunc = Cloneable.GetMemberwiseClone<T>();
            T clone = cloneFunc(@this);
            return clone;
        }
    }

    /// <summary>
    /// Clone extensions.
    /// </summary>
    public static class Cloneable
    {
        /// <inheritdoc cref="ICloneable{T}.Clone()"/>
        public static T Clone<T>(this ICloneable<T> cloneable) => cloneable.Clone();

        /// <summary>
        /// Gets cached compiled MemberwiseClone delegate.
        /// </summary>
        /// <typeparam name="T">Type to get clone function for.</typeparam>
        /// <returns>Function that creates shallow clone of object.</returns>
        public static Func<T, T> GetMemberwiseClone<T>() =>
            (Func<T, T>)TwoLayerCache
                .Instance<Type, object>("MemberwiseClone.Generic")
                .GetOrAdd(typeof(T), type => GetCompiledMemberwiseClone<T>());

        /// <summary>
        /// Gets cached compiled MemberwiseClone delegate.
        /// </summary>
        /// <param name="type">Type to get clone function for.</param>
        /// <returns>Function that creates shallow clone of object.</returns>
        [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup", Justification = "To avoid allocation.")]
        public static Func<object, object> GetMemberwiseClone(Type type) =>
            TwoLayerCache
                .Instance<Type, Func<object, object>>("MemberwiseClone.Untyped")
                .GetOrAdd(type, t => GetCompiledMemberwiseClone(t));

        private static Func<T, T> GetCompiledMemberwiseClone<T>()
        {
            MethodInfo memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var instanceT = Expression.Parameter(typeof(T), "instance");
            var callMemberwiseClone = Expression.Call(instanceT, memberwiseCloneMethod);
            var convertToType = Expression.Convert(callMemberwiseClone, typeof(T));
            Func<T, T> cloneFunc = Expression.Lambda<Func<T, T>>(convertToType, instanceT).Compile();
            return cloneFunc;
        }

        private static Func<object, object> GetCompiledMemberwiseClone(Type type)
        {
            MethodInfo memberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var instanceObj = Expression.Parameter(typeof(object), "instance");
            var instanceT = Expression.Convert(instanceObj, type);
            var callMemberwiseClone = Expression.Call(instanceT, memberwiseCloneMethod);
            Func<object, object> cloneFunc = Expression.Lambda<Func<object, object>>(callMemberwiseClone, instanceObj).Compile();
            return cloneFunc;
        }
    }
}

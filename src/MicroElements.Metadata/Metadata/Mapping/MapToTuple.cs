// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1414 // Tuple types in signatures should have element names

namespace MicroElements.Metadata.Mapping
{
    public static partial class PropertyContainerMapper
    {
        /// <summary>
        /// Extracts property value to output var.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="container">Source property container.</param>
        /// <param name="property">Property to extract.</param>
        /// <param name="value">Output value.</param>
        /// <returns>The same container.</returns>
        public static IPropertyContainer Deconstruct<T>(this IPropertyContainer container, IProperty<T> property, out T value)
        {
            IPropertyValue<T>? propertyValue = container.GetPropertyValue(property);
            if (propertyValue.HasValue())
            {
                value = propertyValue.Value!;
            }
            else
            {
                value = default!;
            }

            return container;
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2) ToTuple<T1, T2>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            return (value1, value2);
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <typeparam name="T3">Property type 3.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <param name="p3">Property 3.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2, T3) ToTuple<T1, T2, T3>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2, IProperty<T3> p3)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            propertyContainer.Deconstruct(p3, out var value3);
            return (value1, value2, value3);
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <typeparam name="T3">Property type 3.</typeparam>
        /// <typeparam name="T4">Property type 4.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <param name="p3">Property 3.</param>
        /// <param name="p4">Property 4.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2, T3, T4) ToTuple<T1, T2, T3, T4>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2, IProperty<T3> p3, IProperty<T4> p4)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            propertyContainer.Deconstruct(p3, out var value3);
            propertyContainer.Deconstruct(p4, out var value4);

            return (value1, value2, value3, value4);
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <typeparam name="T3">Property type 3.</typeparam>
        /// <typeparam name="T4">Property type 4.</typeparam>
        /// <typeparam name="T5">Property type 5.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <param name="p3">Property 3.</param>
        /// <param name="p4">Property 4.</param>
        /// <param name="p5">Property 5.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2, T3, T4, T5) ToTuple<T1, T2, T3, T4, T5>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2, IProperty<T3> p3, IProperty<T4> p4, IProperty<T5> p5)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            propertyContainer.Deconstruct(p3, out var value3);
            propertyContainer.Deconstruct(p4, out var value4);
            propertyContainer.Deconstruct(p5, out var value5);

            return (value1, value2, value3, value4, value5);
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <typeparam name="T3">Property type 3.</typeparam>
        /// <typeparam name="T4">Property type 4.</typeparam>
        /// <typeparam name="T5">Property type 5.</typeparam>
        /// <typeparam name="T6">Property type 6.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <param name="p3">Property 3.</param>
        /// <param name="p4">Property 4.</param>
        /// <param name="p5">Property 5.</param>
        /// <param name="p6">Property 6.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2, T3, T4, T5, T6) ToTuple<T1, T2, T3, T4, T5, T6>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2, IProperty<T3> p3, IProperty<T4> p4, IProperty<T5> p5, IProperty<T6> p6)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            propertyContainer.Deconstruct(p3, out var value3);
            propertyContainer.Deconstruct(p4, out var value4);
            propertyContainer.Deconstruct(p5, out var value5);
            propertyContainer.Deconstruct(p6, out var value6);

            return (value1, value2, value3, value4, value5, value6);
        }

        /// <summary>
        /// Extracts property values to tuple.
        /// </summary>
        /// <typeparam name="T1">Property type 1.</typeparam>
        /// <typeparam name="T2">Property type 2.</typeparam>
        /// <typeparam name="T3">Property type 3.</typeparam>
        /// <typeparam name="T4">Property type 4.</typeparam>
        /// <typeparam name="T5">Property type 5.</typeparam>
        /// <typeparam name="T6">Property type 6.</typeparam>
        /// <typeparam name="T7">Property type 7.</typeparam>
        /// <param name="propertyContainer">Source property container.</param>
        /// <param name="p1">Property 1.</param>
        /// <param name="p2">Property 2.</param>
        /// <param name="p3">Property 3.</param>
        /// <param name="p4">Property 4.</param>
        /// <param name="p5">Property 5.</param>
        /// <param name="p6">Property 6.</param>
        /// <param name="p7">Property 7.</param>
        /// <returns>Result tuple.</returns>
        public static (T1, T2, T3, T4, T5, T6, T7) ToTuple<T1, T2, T3, T4, T5, T6, T7>(this IPropertyContainer propertyContainer, IProperty<T1> p1, IProperty<T2> p2, IProperty<T3> p3, IProperty<T4> p4, IProperty<T5> p5, IProperty<T6> p6, IProperty<T7> p7)
        {
            propertyContainer.Deconstruct(p1, out var value1);
            propertyContainer.Deconstruct(p2, out var value2);
            propertyContainer.Deconstruct(p3, out var value3);
            propertyContainer.Deconstruct(p4, out var value4);
            propertyContainer.Deconstruct(p5, out var value5);
            propertyContainer.Deconstruct(p6, out var value6);
            propertyContainer.Deconstruct(p7, out var value7);

            return (value1, value2, value3, value4, value5, value6, value7);
        }
    }
}

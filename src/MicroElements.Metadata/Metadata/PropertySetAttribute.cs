// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Marks <see cref="IPropertyContainer"/> with properties that it can contain.
    /// Type is required for <see cref="IPropertySet"/> evaluation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertySetAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets type that contains PropertySet.
        /// It can be instance type of typed PropertySet or static type.
        /// </summary>
        public Type? Type { get; set; }

        /// <summary>
        /// Gets or sets property with type <see cref="IPropertySet"/> or <see cref="IEnumerable{T}"/>.
        /// </summary>
        public string? Property { get; set; }

        /// <summary>
        /// Gets or sets method that returns <see cref="IEnumerable{IProperty}"/>.
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Gets <see cref="IPropertySet"/> according search conditions.
        /// </summary>
        /// <returns><see cref="IPropertySet"/> or null.</returns>
        public IPropertySet? GetPropertySet() => PropertySetEvaluator.GetPropertySet(Type, Property, Method);
    }
}

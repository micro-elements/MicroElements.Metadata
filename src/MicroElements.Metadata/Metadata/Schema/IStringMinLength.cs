// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Represents minimum allowed length for string values.
    /// It's an equivalent of JsonSchema minLength.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IStringMinLength : IMetadata
    {
        /// <summary>
        /// Gets minimum allowed length for string values.
        /// </summary>
        int? MinLength { get; }
    }

    /// <inheritdoc />
    public class StringMinLength : IStringMinLength
    {
        /// <inheritdoc />
        public int? MinLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMinLength"/> class.
        /// </summary>
        /// <param name="minLength">Minimum allowed length for string values.</param>
        /// <exception cref="ArgumentException">MinLength should be a non-negative integer.</exception>
        public StringMinLength(int? minLength)
        {
            if (minLength.HasValue && minLength.Value < 0)
                throw new ArgumentException($"MinLength should be a non-negative integer but was {minLength}");
            MinLength = minLength;
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="IStringMinLength"/> metadata to property.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="minLength">Minimum allowed length for string values.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetStringMinLength<T>(this IProperty<T> property, int? minLength)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetMetadata((IStringMinLength)new StringMinLength(minLength));
        }
    }
}

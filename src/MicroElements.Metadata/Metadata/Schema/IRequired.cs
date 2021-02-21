// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Object is valid if required property is exists.
    /// It's an equivalent of JsonSchema required.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IRequired : IMetadata
    {
    }

    /// <summary>
    /// Property required metadata.
    /// </summary>
    public class Required : IRequired
    {
        /// <summary>
        /// Gets the global instance of <see cref="Required"/> metadata.
        /// </summary>
        public static IRequired Instance { get; } = new Required();
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IRequired"/> metadata for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetRequired<T>(this IProperty<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.SetMetadata(Required.Instance);
        }

        /// <summary>
        /// Gets optional <see cref="IRequired"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IRequired"/>.</returns>
        public static IRequired? GetRequired(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IRequired>();
        }
    }

    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface INotEmpty : IMetadata
    {
        // From FluentValidation
        //protected override bool IsValid(PropertyValidatorContext context)
        //{
        //    switch (context.PropertyValue)
        //    {
        //        case null:
        //        case string s when string.IsNullOrWhiteSpace(s):
        //        case ICollection c when c.Count == 0:
        //        case Array a when a.Length == 0:
        //        case IEnumerable e when !e.Cast<object>().Any():
        //            return false;
        //    }
        //    if (Equals(context.PropertyValue, _defaultValueForType))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
    }

    // FV_NotEmpty: NotNull | NotDefault | NotEmptyCollection
    // NotNull->IAllowNull(false), NotDefault->IAllowDefault(false), NotEmptyCollection->IMinItems(>0)
}

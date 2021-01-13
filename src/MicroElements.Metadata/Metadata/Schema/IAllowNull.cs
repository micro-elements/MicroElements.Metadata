// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using MicroElements.Functional;
using MicroElements.Validation;
using MicroElements.Validation.Rules;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Describes whether the property can accept null value.
    /// It's an equivalent of JsonSchema nullable.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IAllowNull : IMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the property can accept null value.
        /// </summary>
        bool IsNullAllowed { get; }
    }

    /// <summary>
    /// Property allows null value.
    /// </summary>
    public class AllowNull : IAllowNull
    {
        /// <summary>
        /// Gets global instance of <see cref="AllowNull"/>.
        /// </summary>
        public static IAllowNull Instance { get; } = new AllowNull();

        /// <inheritdoc />
        public bool IsNullAllowed => true;
    }

    /// <summary>
    /// Property does not allows null value.
    /// </summary>
    public class DisallowNull : IAllowNull
    {
        /// <summary>
        /// Gets global instance of <see cref="DisallowNull"/>.
        /// </summary>
        public static IAllowNull Instance { get; } = new DisallowNull();

        /// <inheritdoc />
        public bool IsNullAllowed => false;
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="IAllowNull"/> metadata for property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="allowNull">Value indicating that property can contain null value.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetAllowNull<T>(this IProperty<T> property, bool allowNull = true)
        {
            property.AssertArgumentNotNull(nameof(property));

            IAllowNull allowNullMetadata = allowNull ? AllowNull.Instance : DisallowNull.Instance;
            return property.SetMetadata(allowNullMetadata);
        }

        /// <summary>
        /// Sets <see cref="IAllowNull"/> metadata to <see cref="AllowNull"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetNullable<T>(this IProperty<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));
            return property.SetMetadata(AllowNull.Instance);
        }

        /// <summary>
        /// Sets <see cref="IAllowNull"/> metadata to <see cref="DisallowNull"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> SetNotNull<T>(this IProperty<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));
            return property.SetMetadata(DisallowNull.Instance);
        }

        /// <summary>
        /// Gets optional <see cref="IAllowNull"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowNull"/>.</returns>
        [Pure]
        public static IAllowNull? GetAllowNull(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IAllowNull>();
        }

        /// <summary>
        /// Gets <see cref="IAllowNull"/> metadata.
        /// If <see cref="IAllowNull"/> metadata is not set then it will be selected according property type can accept null or not.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowNull"/>.</returns>
        [Pure]
        public static IAllowNull GetOrEvaluateAllowNull(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetAllowNull() ?? (property.Type.CanAcceptNull() ? AllowNull.Instance : DisallowNull.Instance);
        }
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class ShouldMatchNullability<T> : BasePropertyRule<T>
    {
        private readonly IAllowNull? _allowNull;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldMatchNullability{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="allowNull">Optional <see cref="IAllowNull"/>.</param>
        public ShouldMatchNullability(IProperty<T> property, IAllowNull? allowNull = null)
            : base(property, "{propertyName} should not be null.")
        {
            _allowNull = allowNull ?? Property.GetAllowNull();
        }

        /// <inheritdoc />
        protected override bool IsValid([MaybeNull] T value, IPropertyContainer propertyContainer)
        {
            if (_allowNull != null)
            {
                // AllowNull    &  null => true
                // AllowNull    & !null => true
                // DisallowNull &  null => false
                // DisallowNull & !null => true
                bool isNotValid = value.IsNull() && !_allowNull.IsNullAllowed;
                return !isNotValid;
            }

            return true;
        }
    }

    /// <summary>
    /// Validation extensions.
    /// </summary>
    public static partial class ValidationExtensions
    {
        /// <summary>
        /// Checks that property value nullability according <see cref="IAllowNull"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <param name="allowNull">Optional <see cref="IAllowNull"/>.</param>
        /// <returns>Validation rule.</returns>
        public static ShouldMatchNullability<T> ShouldMatchNullability<T>(this IProperty<T> property, IAllowNull? allowNull = null)
        {
            return new ShouldMatchNullability<T>(property, allowNull);
        }

        /// <summary>
        /// Checks that property value nullability according <see cref="IAllowNull"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <param name="allowNull">Optional <see cref="IAllowNull"/>.</param>
        /// <returns>Validation rule.</returns>
        public static TValidationRule ShouldMatchNullability<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker, IAllowNull? allowNull = null)
            where TValidationRule : IValidationRule<T>
        {
            return linker.CombineWith(ShouldMatchNullability(linker.FirstRule.Property, allowNull));
        }
    }
}

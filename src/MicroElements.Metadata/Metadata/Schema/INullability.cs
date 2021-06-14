// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using MicroElements.CodeContracts;
using MicroElements.Reflection;
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
    public interface INullability : IMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the property can accept null value.
        /// </summary>
        bool IsNullAllowed { get; }
    }

    /// <summary>
    /// Property allows null value.
    /// </summary>
    public class AllowNull : INullability
    {
        /// <summary>
        /// Gets global instance of <see cref="AllowNull"/>.
        /// </summary>
        public static INullability Instance { get; } = new AllowNull();

        /// <inheritdoc />
        public bool IsNullAllowed => true;
    }

    /// <summary>
    /// Property does not allows null value.
    /// </summary>
    public class DisallowNull : INullability
    {
        /// <summary>
        /// Gets global instance of <see cref="DisallowNull"/>.
        /// </summary>
        public static INullability Instance { get; } = new DisallowNull();

        /// <inheritdoc />
        public bool IsNullAllowed => false;
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Sets <see cref="INullability"/> metadata for the schema.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="allowNull">Value indicating that property can contain null value.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetAllowNull<TSchema>(this TSchema schema, bool allowNull = true)
            where TSchema : ISchema
        {
            schema.AssertArgumentNotNull(nameof(schema));

            INullability allowNullMetadata = allowNull ? AllowNull.Instance : DisallowNull.Instance;
            return schema.SetMetadata(allowNullMetadata);
        }

        /// <summary>
        /// Sets <see cref="INullability"/> metadata to <see cref="AllowNull"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetNullable<TSchema>(this TSchema property)
            where TSchema : ISchema
        {
            property.AssertArgumentNotNull(nameof(property));
            return property.SetMetadata(AllowNull.Instance);
        }

        /// <summary>
        /// Sets <see cref="INullability"/> metadata to <see cref="DisallowNull"/>.
        /// </summary>
        /// <typeparam name="TSchema">Schema.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetNotNull<TSchema>(this TSchema property)
            where TSchema : ISchema
        {
            property.AssertArgumentNotNull(nameof(property));
            return property.SetMetadata(DisallowNull.Instance);
        }

        /// <summary>
        /// Gets optional <see cref="INullability"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="INullability"/> metadata.</returns>
        [Pure]
        public static INullability? GetNullability(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetSchemaMetadata<INullability>();
        }

        /// <summary>
        /// Gets <see cref="INullability"/> metadata.
        /// If <see cref="INullability"/> metadata is not set then it will be selected according property type can accept null or not.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="INullability"/>.</returns>
        [Pure]
        public static INullability GetOrEvaluateNullability(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetNullability() ?? (property.Type.CanAcceptNull() ? AllowNull.Instance : DisallowNull.Instance);
        }
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    public class ShouldMatchNullability : PropertyValidationRule
    {
        private readonly INullability? _allowNull;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldMatchNullability"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="allowNull">Optional <see cref="INullability"/>.</param>
        public ShouldMatchNullability(IProperty property, INullability? allowNull = null)
            : base(property, "{propertyName} should not be null.")
        {
            _allowNull = allowNull ?? property.GetNullability();
        }

        /// <inheritdoc />
        protected override bool IsValid(IPropertyValue propertyValue)
        {
            if (_allowNull != null)
            {
                // AllowNull    &  null => true
                // AllowNull    & !null => true
                // DisallowNull &  null => false
                // DisallowNull & !null => true
                bool isNotValid = propertyValue.ValueUntyped is null && !_allowNull.IsNullAllowed;
                return !isNotValid;
            }

            return true;
        }
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class ShouldMatchNullability<T> : PropertyValidationRule<T>
    {
        private readonly INullability? _allowNull;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldMatchNullability{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="allowNull">Optional <see cref="INullability"/>.</param>
        public ShouldMatchNullability(IProperty<T> property, INullability? allowNull = null)
            : base(property, "{propertyName} should not be null.")
        {
            _allowNull = allowNull ?? Property.GetNullability();
        }

        /// <inheritdoc />
        protected override bool IsValid(T? value)
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
        /// Checks that property value nullability according <see cref="INullability"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <param name="allowNull">Optional <see cref="INullability"/>.</param>
        /// <returns>Validation rule.</returns>
        public static ShouldMatchNullability<T> ShouldMatchNullability<T>(this IProperty<T> property, INullability? allowNull = null)
        {
            return new ShouldMatchNullability<T>(property, allowNull);
        }

        /// <summary>
        /// Checks that property value nullability according <see cref="INullability"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <param name="allowNull">Optional <see cref="INullability"/>.</param>
        /// <returns>Validation rule.</returns>
        public static TValidationRule ShouldMatchNullability<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker, INullability? allowNull = null)
            where TValidationRule : IPropertyValidationRule<T>
        {
            return linker.CombineWith(ShouldMatchNullability(linker.FirstRule.Property, allowNull));
        }
    }
}

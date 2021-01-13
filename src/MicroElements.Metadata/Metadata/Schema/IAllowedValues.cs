// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Validation;
using MicroElements.Validation.Rules;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Describes allowed values that can be accepted by property.
    /// It's an equivalent of JsonSchema enum.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property)]
    public interface IAllowedValues : IMetadata
    {
        /// <summary>
        /// Gets all possible values that can be accepted by property.
        /// </summary>
        IReadOnlyList<object> ValuesUntyped { get; }
    }

    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Describes allowed values that can be accepted by property.
    /// It's an equivalent of JsonSchema enum.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public interface IAllowedValues<T> : IAllowedValues
    {
        /// <summary>
        /// Gets all possible values that can be accepted by property.
        /// </summary>
        IReadOnlyList<T> Values { get; }
    }

    /// <inheritdoc cref="IAllowedValues{T}"/>
    public class AllowedValues<T> : IAllowedValues<T>
    {
        private readonly Lazy<IReadOnlyList<object>> _lazyValuesUntyped;

        /// <inheritdoc />
        public IReadOnlyList<T> Values { get; }

        /// <inheritdoc />
        public IReadOnlyList<object> ValuesUntyped => _lazyValuesUntyped.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedValues{T}"/> class.
        /// </summary>
        /// <param name="values">All possible values that can be accepted by property.</param>
        public AllowedValues(IReadOnlyList<T> values)
        {
            values.AssertArgumentNotNull(nameof(values));

            Values = values;
            _lazyValuesUntyped = new Lazy<IReadOnlyList<object>>(() => Values.Cast<object>().ToArray());
        }
    }

    /// <summary>
    /// Property schema extensions.
    /// </summary>
    public static partial class SchemaExtensions
    {
        /// <summary>
        /// Adds <see cref="IAllowedValues{T}"/> metadata to property.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> WithAllowedValues<T>(this IProperty<T> property, params T[] allowedValues)
        {
            property.AssertArgumentNotNull(nameof(property));
            allowedValues.AssertArgumentNotNull(nameof(allowedValues));

            return property.SetMetadata((IAllowedValues)new AllowedValues<T>(allowedValues));
        }

        /// <summary>
        /// Adds <see cref="IAllowedValues{T}"/> metadata to property.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The same property.</returns>
        public static IProperty<T> WithAllowedValues<T>(this IProperty<T> property, IAllowedValues<T> allowedValues)
        {
            property.AssertArgumentNotNull(nameof(property));
            allowedValues.AssertArgumentNotNull(nameof(allowedValues));

            return property.SetMetadata((IAllowedValues)allowedValues);
        }

        /// <summary>
        /// Gets optional <see cref="IAllowedValues{T}"/> metadata.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowedValues{T}"/>.</returns>
        [Pure]
        public static IAllowedValues<T>? GetAllowedValues<T>(this IProperty<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IAllowedValues>() as IAllowedValues<T>;
        }

        /// <summary>
        /// Gets optional <see cref="IAllowedValues"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowedValues"/>.</returns>
        [Pure]
        public static IAllowedValues? GetAllowedValuesUntyped(this IProperty property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetMetadata<IAllowedValues>();
        }
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class OnlyAllowedValuesRule<T> : BasePropertyRule<T>
    {
        private readonly IAllowedValues<T>? _allowedValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyAllowedValuesRule{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="allowedValues">Optional <see cref="IAllowedValues{T}"/>.</param>
        public OnlyAllowedValuesRule(IProperty<T> property, IAllowedValues<T>? allowedValues = null)
            : base(property, "{propertyName} can not be '{value}' because it is not in allowed values list.")
        {
            _allowedValues = allowedValues ?? Property.GetAllowedValues();
        }

        /// <inheritdoc />
        protected override bool IsValid([MaybeNull] T value, IPropertyContainer propertyContainer)
        {
            if (_allowedValues != null)
            {
                return _allowedValues.Values.Contains(value);
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
        /// Checks that property value is in allowed values list.
        /// If <paramref name="allowedValues"/> is not set, then <see cref="SchemaExtensions.GetAllowedValues{T}"/> will be used.
        /// Set allowed values for property with <see cref="SchemaExtensions.WithAllowedValues{T}(IProperty{T},T[])"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="property">Property to check.</param>
        /// <param name="allowedValues">Optional <see cref="IAllowedValues{T}"/>.</param>
        /// <returns>Validation rule.</returns>
        public static OnlyAllowedValuesRule<T> OnlyAllowedValues<T>(this IProperty<T> property, IAllowedValues<T>? allowedValues = null)
        {
            return new OnlyAllowedValuesRule<T>(property, allowedValues);
        }

        /// <summary>
        /// Checks that property value is in allowed values list.
        /// If <paramref name="allowedValues"/> is not set, then <see cref="SchemaExtensions.GetAllowedValues{T}"/> will be used.
        /// Set allowed values for property with <see cref="SchemaExtensions.WithAllowedValues{T}(IProperty{T},T[])"/>.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <param name="allowedValues">Optional <see cref="IAllowedValues{T}"/>.</param>
        /// <returns>Validation rule.</returns>
        public static TValidationRule OnlyAllowedValues<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker, IAllowedValues<T>? allowedValues = null)
            where TValidationRule : IValidationRule<T>
        {
            return linker.CombineWith(OnlyAllowedValues(linker.FirstRule.Property, allowedValues));
        }
    }
}

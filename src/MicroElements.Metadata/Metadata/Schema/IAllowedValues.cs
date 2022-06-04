// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using MicroElements.CodeContracts;
using MicroElements.Collections.Extensions.NotNull;
using MicroElements.Metadata.ComponentModel;
using MicroElements.Reflection.TypeExtensions;
using MicroElements.Text.StringFormatter;
using MicroElements.Validation;
using MicroElements.Validation.Rules;

namespace MicroElements.Metadata.Schema
{
    /// <summary>
    /// Optional <see cref="IProperty"/> metadata.
    /// Describes allowed values that can be accepted by property.
    /// It's an equivalent of JsonSchema enum.
    /// </summary>
    [MetadataUsage(ValidOn = MetadataTargets.Property | MetadataTargets.SimpleSchema)]
    public interface IAllowedValues : ISchemaComponent
    {
        /// <summary>
        /// Gets all possible values that can be accepted by property.
        /// </summary>
        IReadOnlyCollection<object> ValuesUntyped { get; }
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
        IReadOnlyCollection<T> Values { get; }

        /// <summary>
        /// Gets comparer for values.
        /// </summary>
        IEqualityComparer<T> Comparer { get; }
    }

    /// <inheritdoc cref="IAllowedValues{T}"/>
    public class AllowedValues<T> : IAllowedValues<T>
    {
        private readonly Lazy<IReadOnlyCollection<object>> _lazyValuesUntyped;

        /// <inheritdoc />
        public IReadOnlyCollection<T> Values { get; }

        /// <inheritdoc />
        public IEqualityComparer<T> Comparer { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<object> ValuesUntyped => _lazyValuesUntyped.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllowedValues{T}"/> class.
        /// </summary>
        /// <param name="values">All possible values that can be accepted by property.</param>
        /// <param name="comparer">Equality comparer.</param>
        public AllowedValues(IReadOnlyCollection<T> values, IEqualityComparer<T>? comparer = null)
        {
            values.AssertArgumentNotNull(nameof(values));

            Values = values;
            Comparer = comparer ?? EqualityComparer<T>.Default;

            _lazyValuesUntyped = new Lazy<IReadOnlyCollection<object>>(() => Values.Cast<object>().ToArray());
        }

        /// <inheritdoc />
        public override string ToString() => $"{Values.FormatValue()}";
    }

    public static class AllowedValues
    {
        public static AllowedValues<T> CreateFromEnumerable<T>(IEnumerable values, IEqualityComparer<T>? comparer = null)
        {
            T[] array = values.Cast<T>().ToArray();
            return new AllowedValues<T>(array, comparer);
        }

        public static IAllowedValues CreateUntyped(Type valueType, IEnumerable values, IEqualityComparer? comparer = null)
        {
            valueType.AssertArgumentNotNull(nameof(valueType));
            values.AssertArgumentNotNull(nameof(values));

            MethodInfo methodInfo = typeof(AllowedValues)
                .GetMethod(nameof(AllowedValues.CreateFromEnumerable), BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(valueType);

            object allowedValues = methodInfo.Invoke(null, parameters: new object[] { values, comparer });
            return (IAllowedValues)allowedValues;
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
        public static TSchema SetAllowedValues<TSchema, T>(this TSchema property, params T[] allowedValues)
            where TSchema : ISchema<T>
        {
            property.AssertArgumentNotNull(nameof(property));
            allowedValues.AssertArgumentNotNull(nameof(allowedValues));

            return property.SetMetadata((IAllowedValues)new AllowedValues<T>(allowedValues));
        }

        /// <summary>
        /// Adds <see cref="IAllowedValues{T}"/> metadata to property.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetAllowedValues<TSchema, T>(this TSchema property, IAllowedValues<T> allowedValues)
            where TSchema : ISchema<T>
        {
            property.AssertArgumentNotNull(nameof(property));
            allowedValues.AssertArgumentNotNull(nameof(allowedValues));

            return property.SetMetadata((IAllowedValues)allowedValues);
        }

        /// <summary>
        /// Adds <see cref="IAllowedValues"/> metadata to property.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="allowedValues">Allowed values.</param>
        /// <param name="equalityComparer">Optional value comparer.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetAllowedValuesUntyped<TSchema>(this TSchema property, IEnumerable allowedValues, IEqualityComparer? equalityComparer = null)
            where TSchema : ISchema
        {
            property.AssertArgumentNotNull(nameof(property));
            allowedValues.AssertArgumentNotNull(nameof(allowedValues));

            return property.SetMetadata((IAllowedValues)AllowedValues.CreateUntyped(property.Type, allowedValues, equalityComparer));
        }

        /// <summary>
        /// Gets optional <see cref="IAllowedValues{T}"/> metadata.
        /// </summary>
        /// <typeparam name="T">Property value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowedValues{T}"/>.</returns>
        [Pure]
        public static IAllowedValues<T>? GetAllowedValues<T>(this ISchema<T> property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetComponent<IAllowedValues>() as IAllowedValues<T>;
        }

        /// <summary>
        /// Gets optional <see cref="IAllowedValues"/> metadata.
        /// </summary>
        /// <param name="property">Source property.</param>
        /// <returns>Optional <see cref="IAllowedValues"/>.</returns>
        [Pure]
        public static IAllowedValues? GetAllowedValuesUntyped(this ISchema property)
        {
            property.AssertArgumentNotNull(nameof(property));

            return property.GetComponent<IAllowedValues>();
        }

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from enum type.
        /// </summary>
        /// <typeparam name="TSchema">Schema type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="enumType">Enum type.</param>
        /// <param name="equalityComparer">Optional equality comparer.</param>
        /// <returns>The same property.</returns>
        public static TSchema SetAllowedValuesFromEnum<TSchema>(this TSchema property, Type enumType, IEqualityComparer? equalityComparer = null)
            where TSchema : ISchema
        {
            property.AssertArgumentNotNull(nameof(property));

            if (!enumType.IsEnum)
                throw new ArgumentException($"Type '{enumType}' should be Enum type to use in {nameof(SetAllowedValuesFromEnum)}.");

            Type underlyingType = Enum.GetUnderlyingType(enumType);

            if (property.Type == enumType || property.Type == underlyingType)
            {
                var values = Enum.GetValues(enumType);
                property.SetAllowedValuesUntyped(values, equalityComparer);
            }
            else if (property is ISchema<string> stringSchema)
            {
                string[] names = Enum.GetNames(enumType);

                var comparer = equalityComparer as IEqualityComparer<string>;
                var allowedValues = new AllowedValues<string>(names, comparer);
                stringSchema.SetAllowedValues(allowedValues);
            }
            else
            {
                throw new ArgumentException($"{nameof(TSchema)} should be typed as string or {enumType} or {underlyingType}.");
            }

            return property;
        }

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static IProperty<TEnum> SetAllowedValuesFromEnum<TEnum>(this IProperty<TEnum> property)
            => property.SetAllowedValuesFromEnum(typeof(TEnum));

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static ISchema<TEnum> SetAllowedValuesFromEnum<TEnum>(this ISchema<TEnum> property)
            => property.SetAllowedValuesFromEnum(typeof(TEnum));

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <param name="ignoreCase">Compare enum values ignore case.</param>
        /// <returns>The same property.</returns>
        public static IProperty<string> SetAllowedValuesFromEnum<TEnum>(this IProperty<string> property, bool ignoreCase = true)
            => property.SetAllowedValuesFromEnum(typeof(TEnum), ignoreCase ? StringComparer.OrdinalIgnoreCase : null);

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="schema">Source schema.</param>
        /// <param name="ignoreCase">Compare enum values ignore case.</param>
        /// <returns>The same property.</returns>
        public static ISchema<string> SetAllowedValuesFromEnum<TEnum>(this ISchema<string> schema, bool ignoreCase = true)
            => schema.SetAllowedValuesFromEnum(typeof(TEnum), ignoreCase ? StringComparer.OrdinalIgnoreCase : null);

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static ISchema<int> SetAllowedValuesFromEnum<TEnum>(this ISchema<int> property)
            => property.SetAllowedValuesFromEnum(typeof(TEnum));

        /// <summary>
        /// Sets <see cref="IAllowedValues"/> metadata from <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Enum value type.</typeparam>
        /// <param name="property">Source property.</param>
        /// <returns>The same property.</returns>
        public static IProperty<int> SetAllowedValuesFromEnum<TEnum>(this IProperty<int> property)
            => property.SetAllowedValuesFromEnum(typeof(TEnum));
    }

    /// <summary>
    /// Validation rule that checks property value is one of allowed values.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class OnlyAllowedValuesRule<T> : PropertyValidationRule<T>
    {
        private readonly IAllowedValues<T>? _allowedValues;
        private readonly bool _canAcceptNull;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyAllowedValuesRule{T}"/> class.
        /// </summary>
        /// <param name="property">Property to check.</param>
        /// <param name="allowedValues">Optional <see cref="IAllowedValues{T}"/>.</param>
        public OnlyAllowedValuesRule(IProperty<T> property, IAllowedValues<T>? allowedValues = null)
            : base(property, "{propertyName} can not be '{value}' because it is not in allowed values list. Allowed values: {allowedValues}.")
        {
            _allowedValues = allowedValues ?? Property.GetAllowedValues();
            _canAcceptNull = typeof(T).CanAcceptNull();

            Lazy<string> allowedValuesText = new (() => _allowedValues?.Values.NotNull().FormatAsTuple() ?? "()");
            this.ConfigureMessage(message => message.AddProperty("allowedValues", allowedValuesText.Value));
        }

        /// <inheritdoc />
        protected override bool IsValid(T? value)
        {
            if (_allowedValues is null)
                return true;

            // Null value should be checked with INullability
            if (_canAcceptNull && value is null)
                return true;

            return _allowedValues.Values.Contains(value, _allowedValues.Comparer);
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
        /// Set allowed values for property with one of SetAllowedValues methods.
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
        /// Set allowed values for property with one of SetAllowedValues methods.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <typeparam name="TValidationRule">Combined validation rule type.</typeparam>
        /// <param name="linker">Rule linker.</param>
        /// <param name="allowedValues">Optional <see cref="IAllowedValues{T}"/>.</param>
        /// <returns>Validation rule.</returns>
        public static TValidationRule OnlyAllowedValues<T, TValidationRule>(this IValidationRuleLinker<T, TValidationRule> linker, IAllowedValues<T>? allowedValues = null)
            where TValidationRule : IPropertyValidationRule<T>
        {
            return linker.CombineWith(OnlyAllowedValues(linker.FirstRule.Property, allowedValues));
        }
    }
}

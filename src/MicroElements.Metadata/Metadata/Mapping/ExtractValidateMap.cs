// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;
using MicroElements.Validation;
using MicroElements.Validation.Rules;

namespace MicroElements.Metadata.Mapping
{
    /// <summary>
    /// Context for extraction, validation, mapping.
    /// </summary>
    public interface IExtractValidateMapContext
    {
        /// <summary>
        /// Gets property container.
        /// </summary>
        IPropertyContainer PropertyContainer { get; }

        /// <summary>
        /// Gets validation or other messages.
        /// </summary>
        IReadOnlyCollection<Message> Messages { get; }

        /// <summary>
        /// Gets extract settings.
        /// </summary>
        IExtractValidateMapSettings Settings { get; }

        /// <summary>
        /// Adds messages to context.
        /// </summary>
        /// <param name="messages">Messages.</param>
        /// <returns>The same context.</returns>
        IExtractValidateMapContext AddMessages(IEnumerable<Message> messages);
    }

    /// <summary>
    /// ExtractValidateMap settings.
    /// </summary>
    public interface IExtractValidateMapSettings
    {
        /// <summary>
        /// Gets a value indicating whether extractor should use property validators.
        /// </summary>
        bool UsePropertyValidators { get; }

        /// <summary>
        /// Gets a value indicating whether extractor should stop on first error.
        /// </summary>
        bool StopOnFirstError { get; }
    }

    /// <summary>
    /// ExtractValidateMap settings.
    /// </summary>
    public class ExtractValidateMapSettings : IExtractValidateMapSettings
    {
        /// <inheritdoc/>
        public bool UsePropertyValidators { get; set; } = true;

        /// <inheritdoc/>
        public bool StopOnFirstError { get; set; } = false;
    }

    /// <summary>
    /// Default <see cref="IExtractValidateMapContext"/> implementation.
    /// </summary>
    public class ExtractValidateMapContext : IExtractValidateMapContext
    {
        private readonly List<Message> _messages = new List<Message>();

        /// <inheritdoc />
        public IPropertyContainer PropertyContainer { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<Message> Messages => _messages;

        /// <inheritdoc />
        public IExtractValidateMapSettings Settings { get; }

        /// <inheritdoc />
        public IExtractValidateMapContext AddMessage(Message message)
        {
            _messages.Add(message);
            return this;
        }

        /// <inheritdoc />
        public IExtractValidateMapContext AddMessages(IEnumerable<Message> messages)
        {
            _messages.AddRange(messages);
            return this;
        }

        public ExtractValidateMapContext(IPropertyContainer propertyContainer, IExtractValidateMapSettings settings)
        {
            //TODO: SearchOptions? from propertyContainer?
            SearchOptions searchOptions = SearchOptions.ExistingOnlyWithParent.CalculateValue(true);
            PropertyContainer = new PropertyContainer(propertyContainer, searchOptions: searchOptions);
            Settings = settings;
        }
    }

    public interface IPropertyExtractContext<T>
    {
        /// <summary>
        /// Gets extractor context.
        /// </summary>
        public IExtractValidateMapContext Context { get; }

        (IPropertyValue<T> PropertyValue, ICollection<Message> Messages) GetResult();
    }

    /// <summary>
    /// Extract context for single property.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public class PropertyExtractContext<T> : IPropertyExtractContext<T>
    {
        /// <summary>
        /// Gets extractor context.
        /// </summary>
        public IExtractValidateMapContext Context { get; }

        /// <summary>
        /// Gets property to extract.
        /// </summary>
        public IProperty<T> Property { get; }

        /// <summary>
        /// Gets validation rules to use before extract.
        /// </summary>
        public List<IPropertyValidationRule<T>> ValidationRules { get; } = new List<IPropertyValidationRule<T>>();

        #region IExtractValidateMapContext

        /// <inheritdoc />
        public IPropertyContainer PropertyContainer => Context.PropertyContainer;

        /// <inheritdoc />
        public IReadOnlyCollection<Message> Messages => Context.Messages;

        /// <inheritdoc />
        public IExtractValidateMapSettings Settings => Context.Settings;

        /// <inheritdoc />
        public IExtractValidateMapContext AddMessages(IEnumerable<Message> messages)
        {
            Context.AddMessages(messages);
            return Context;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExtractContext{T}"/> class.
        /// </summary>
        /// <param name="extractValidateMapContext">Extractor context.</param>
        /// <param name="property">property to extract.</param>
        /// <param name="validationRules">Optional validation rules to use before extract.</param>
        public PropertyExtractContext(
            IExtractValidateMapContext extractValidateMapContext,
            IProperty<T> property,
            IEnumerable<IPropertyValidationRule<T>>? validationRules = null)
        {
            Context = extractValidateMapContext;
            Property = property;

            if (Context.Settings.UsePropertyValidators)
            {
                var propertyValidationRules = ValidationProvider.Instance.GetValidationRules(Property).OfType<IPropertyValidationRule<T>>();
                ValidationRules.AddRange(propertyValidationRules);
            }

            if (validationRules != null)
                ValidationRules.AddRange(validationRules);
        }

        /// <inheritdoc />
        public (IPropertyValue<T> PropertyValue, ICollection<Message> Messages) GetResult()
        {
            // Get value.
            IPropertyValue<T>? propertyValue = GetPropertyValue() ?? new PropertyValue<T>(Property, default, ValueSource.NotDefined);

            // Validate value.
            Validate(propertyValue, out var messages);

            return (propertyValue, messages);
        }

        /// <summary>
        /// Final output method.
        /// Gets property value, validates it and outputs to result variable.
        /// </summary>
        /// <param name="value">Output variable.</param>
        /// <returns>Initial context.</returns>
        public IExtractValidateMapContext Output(out T value)
        {
            // Get value.
            var (propertyValue, _) = GetResult();

            // Output.
            value = propertyValue.Value!;

            return Context;
        }

        public IPropertyValue<T>? GetPropertyValue()
        {
            return PropertyContainer.GetPropertyValue(Property);
        }

        public PropertyExtractContext<T> Validate(IPropertyValue<T>? propertyValue, out Message[] messages)
        {
            messages = ValidationRules
                .SelectMany(rule => rule.Validate(propertyValue, PropertyContainer))
                .ToArray();

            if (messages.Length > 0)
                Context.AddMessages(messages);

            return this;
        }

        /// <summary>
        /// Adds required check to property validation rules.
        /// </summary>
        /// <returns>The same instance.</returns>
        public PropertyExtractContext<T> Required()
        {
            if (!ValidationRules.OfType<IRequiredPropertyValidationRule>().Any())
                ValidationRules.Add(Property.Required());
            return this;
        }

        /// <summary>
        /// Clears validation rules for property.
        /// </summary>
        /// <returns>The same instance.</returns>
        public PropertyExtractContext<T> NoValidation()
        {
            ValidationRules.Clear();
            return this;
        }

        /// <summary>
        /// Adds validation rule.
        /// </summary>
        /// <param name="createValidationRule">Rule factory.</param>
        /// <returns>The same instance.</returns>
        public PropertyExtractContext<T> WithValidation(Func<IProperty<T>, IPropertyValidationRule<T>> createValidationRule)
        {
            IPropertyValidationRule<T> propertyValidationRule = createValidationRule(Property);
            ValidationRules.Add(propertyValidationRule);
            return this;
        }

        /// <summary>
        /// Adds validation rules.
        /// </summary>
        /// <param name="validationRules">Validation rules.</param>
        /// <returns>The same instance.</returns>
        public PropertyExtractContext<T> WithValidationRules(IEnumerable<IPropertyValidationRule<T>> validationRules)
        {
            ValidationRules.AddRange(validationRules);
            return this;
        }
    }

    public class PropertyExtractContext<T1, T2> : IPropertyExtractContext<T2>
    {
        /// <inheritdoc />
        public IExtractValidateMapContext Context => ParentContext.Context;

        public IPropertyExtractContext<T1> ParentContext { get; }

        protected Func<IPropertyValue<T1>, T2> Map { get; }

        public PropertyExtractContext(IPropertyExtractContext<T1> parentContext, Func<IPropertyValue<T1>, T2> map)
        {
            ParentContext = parentContext;
            Map = map;
        }

        /// <summary>
        /// Final output method.
        /// Gets property value, validates it and outputs to result variable.
        /// </summary>
        /// <param name="value">Output variable.</param>
        /// <returns>Initial context.</returns>
        public IExtractValidateMapContext Output(out T2 value)
        {
            // Get parent value.
            var (propertyValue, messages) = ParentContext.GetResult();

            // Map value (if initial value is valid).
            if (messages is { Count: 0 } && propertyValue.HasValue())
            {
                // Output value.
                value = Map(propertyValue);
            }
            else
            {
                value = default;
            }

            return ParentContext.Context;
        }

        /// <inheritdoc />
        public (IPropertyValue<T2> PropertyValue, ICollection<Message> Messages) GetResult()
        {
            // Get parent value.
            var (propertyValue, messages) = ParentContext.GetResult();

            // Map value (if initial value is valid).
            IPropertyValue<T2> pv2;
            IProperty<T2> property2 = new Property<T2>($"Generated_{Guid.NewGuid()}");

            if (messages is { Count: 0 } && propertyValue.HasValue())
            {
                // Output value.
                var value2 = Map(propertyValue);
                pv2 = new PropertyValue<T2>(property2, value2, ValueSource.Calculated);
            }
            else
            {
                pv2 = new PropertyValue<T2>(property2, default, ValueSource.NotDefined);
            }

            return (pv2, messages);
        }
    }

    public static class ExtractorExtensions
    {
        public static IExtractValidateMapContext Extractor(this IPropertyContainer propertyContainer, Action<ExtractValidateMapSettings>? configure = null)
        {
            var settings = new ExtractValidateMapSettings();
            configure?.Invoke(settings);
            return new ExtractValidateMapContext(propertyContainer, settings);
        }

        public static PropertyExtractContext<T> Extract<T>(this IExtractValidateMapContext extractValidateMapContext, IProperty<T> property)
        {
            return new PropertyExtractContext<T>(extractValidateMapContext, property);
        }

        public static PropertyExtractContext<T, T?> Optional<T>(this PropertyExtractContext<T> context)
            where T : struct
        {
            static T? Nullify(IPropertyValue<T> propertyValue)
            {
                return propertyValue.HasValue() ? propertyValue.Value : null;
            }

            return Map<T, T?>(context, arg => Nullify(arg));
        }

        public static PropertyExtractContext<T> ExcludeValidationRule<T>(this PropertyExtractContext<T> context, Func<IPropertyValidationRule<T>, bool> excludePredicate)
        {
            var withoutExcluded = context.ValidationRules.Where(rule => !excludePredicate(rule));
            return context.WithValidationRules(withoutExcluded);
        }

        public static PropertyExtractContext<T1, T2> Map<T1, T2>(this PropertyExtractContext<T1> context, Func<T1, T2> map)
        {
            return new PropertyExtractContext<T1, T2>(context, pv => map(pv.Value));
        }

        public static PropertyExtractContext<T1, T2> Map<T1, T2>(this PropertyExtractContext<T1> context, Func<IPropertyValue<T1>, T2> map)
        {
            return new PropertyExtractContext<T1, T2>(context, map);
        }

        public static PropertyExtractContext<T2, T3> Map<T1, T2, T3>(this PropertyExtractContext<T1, T2> context, Func<IPropertyValue<T2>, T3> map)
        {
            return new PropertyExtractContext<T2, T3>(context, map);
        }

        public static PropertyExtractContext<T2, T3> Map<T1, T2, T3>(this PropertyExtractContext<T1, T2> context, Func<T2, T3> map)
        {
            return new PropertyExtractContext<T2, T3>(context, pv => map(pv.Value));
        }

        public static PropertyExtractContext<string, TEnum> MapToEnum<TEnum>(this PropertyExtractContext<string> context)
            where TEnum : struct
        {
            static TEnum Parse(string text, IExtractValidateMapContext extractValidateMapContext)
            {
                if (Enum.TryParse(typeof(TEnum), text, ignoreCase: true, out object result))
                {
                    return (TEnum)result;
                }

                extractValidateMapContext.AddMessages(new[] { new Message($"Can not convert value {text} to enum {typeof(TEnum).Name}", MessageSeverity.Error) });
                return default;
            }

            return Map<string, TEnum>(context, text => Parse(text, context.Context));
        }

        public static bool IsSuccess(this IExtractValidateMapContext extractValidateMapContext) => extractValidateMapContext.Messages.Count == 0;

        public static IExtractValidateMapContext ExtractErrors(this IExtractValidateMapContext extractValidateMapContext,
            out IReadOnlyCollection<Message>? messages)
        {
            messages = extractValidateMapContext.Messages.Count > 0 ? extractValidateMapContext.Messages : null;
            return extractValidateMapContext;
        }

        public static bool HasErrors(this IExtractValidateMapContext extractValidateMapContext, out IReadOnlyCollection<Message>? messages)
        {
            if (extractValidateMapContext.Messages.Count > 0)
            {
                messages = extractValidateMapContext.Messages;
                return true;
            }
            else
            {
                messages = null;
                return false;
            }
        }

        public static IExtractValidateMapContext Required<T>(this IExtractValidateMapContext extractValidateMapContext, IProperty<T> property, out T value)
        {
            value = default!;

            //if (validationContext.IsSuccess())
            {
                SearchOptions searchOptions = SearchOptions.ExistingOnlyWithParent.CalculateValue(true);
                PropertyContainer propertyContainer = new PropertyContainer(extractValidateMapContext.PropertyContainer, searchOptions: searchOptions);

                var messages = ValidationProvider.Instance
                    .GetValidationRules(property)
                    .SelectMany(rule => rule.Validate(propertyContainer))
                    .ToArray();

                if (messages.Length > 0)
                    extractValidateMapContext.AddMessages(messages);

                IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, searchOptions);
                if (propertyValue.HasValue())
                {
                    value = propertyValue.Value!;
                }
                else
                {
                    messages = property.Required().Validate(propertyValue, extractValidateMapContext.PropertyContainer).ToArray();
                    extractValidateMapContext.AddMessages(messages);
                }
            }

            return extractValidateMapContext;
        }

        public static IExtractValidateMapContext Required<T, T2>(this IExtractValidateMapContext extractValidateMapContext, IProperty<T> property, Func<T, T2> map, out T2 value)
        {
            value = default!;

            //if (validationContext.IsSuccess())
            {
                SearchOptions searchOptions = SearchOptions.ExistingOnlyWithParent.CalculateValue(true);
                PropertyContainer propertyContainer = new PropertyContainer(extractValidateMapContext.PropertyContainer, searchOptions: searchOptions);

                IPropertyValue<T>? propertyValue = propertyContainer.GetPropertyValue(property, searchOptions);

                var messages = ValidationProvider.Instance
                    .GetValidationRules(property)
                    .SelectMany(rule => rule is IPropertyValidationRule propertyValidationRule
                        ? propertyValidationRule.Validate(propertyValue, propertyContainer)
                        : rule.Validate(propertyContainer))
                    .ToArray();

                if (messages.Length > 0)
                    extractValidateMapContext.AddMessages(messages);

                if (propertyValue.HasValue())
                {
                    value = map(propertyValue.Value!);
                }
                else
                {
                    messages = property.Required().Validate(propertyValue, extractValidateMapContext.PropertyContainer).ToArray();
                    extractValidateMapContext.AddMessages(messages);
                }
            }

            return extractValidateMapContext;
        }

        public static IExtractValidateMapContext Optional<T>(this IExtractValidateMapContext extractValidateMapContext, IProperty<T> property, out T? value)
            where T : struct
        {
            IPropertyValue<T>? propertyValue = extractValidateMapContext.PropertyContainer.GetPropertyValue(property, SearchOptions.ExistingOnlyWithParent);
            if (propertyValue.HasValue())
            {
                value = propertyValue.Value!;
            }
            else
            {
                value = default!;
            }

            return extractValidateMapContext;
        }

        public static IProperty<TEnum> MapToEnum<TEnum>(this IProperty<string> property)
            where TEnum : struct
        {
            return property.Map(source => Enum.Parse<TEnum>(source));
        }
    }
}

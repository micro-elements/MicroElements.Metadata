// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    /// <summary>
    /// Property extract context.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    public interface IPropertyExtractContext<T>
    {
        /// <summary>
        /// Gets extractor context.
        /// </summary>
        IExtractValidateMapContext Context { get; }

        /// <summary>
        /// Gets property to extract.
        /// </summary>
        IProperty<T> Property { get; }

        /// <summary>
        /// Gets property validation rules.
        /// </summary>
        List<IPropertyValidationRule<T>> ValidationRules { get; }

        /// <summary>
        /// Gets extract result.
        /// </summary>
        /// <returns>Extract and validate result.</returns>
        ExtractResult<T> GetResult();
    }

    /// <summary>
    /// Extract and validate result.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public record ExtractResult<T>(IPropertyValue<T> PropertyValue, ICollection<Message> Messages);

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
        public ExtractResult<T> GetResult()
        {
            // Get value.
            IPropertyValue<T> propertyValue = GetPropertyValue();

            // Validate value.
            this.Validate(propertyValue, out var messages);

            return new ExtractResult<T>(propertyValue, messages);

            IPropertyValue<T> GetPropertyValue()
            {
                return Context.PropertyContainer.GetPropertyValue(Property) ?? new PropertyValue<T>(Property, default, ValueSource.NotDefined);
            }
        }
    }

    public class PropertyExtractMapContext<T1, T2> : IPropertyExtractContext<T2>
    {
        /// <inheritdoc />
        public IExtractValidateMapContext Context => PropertyContext.Context;

        /// <inheritdoc />
        public IProperty<T2> Property { get; }

        public IPropertyExtractContext<T1> PropertyContext { get; }

        protected Func<IPropertyValue<T1>, T2> MapFunc { get; }

        /// <summary>
        /// Gets validation rules to use before extract.
        /// </summary>
        public List<IPropertyValidationRule<T2>> ValidationRules { get; } = new List<IPropertyValidationRule<T2>>();

        public PropertyExtractMapContext(
            IPropertyExtractContext<T1> propertyContext,
            Func<IPropertyValue<T1>, T2> map,
            IEnumerable<IPropertyValidationRule<T2>>? validationRules = null)
        {
            PropertyContext = propertyContext;
            MapFunc = map;

            Property = new Property<T2>(PropertyContext.Property.Name);
            ValidationRules.AddRange(validationRules.NotNull());
        }

        /// <inheritdoc />
        public ExtractResult<T2> GetResult()
        {
            // Get parent value.
            var result1 = PropertyContext.GetResult();

            // Map value (if initial value is valid).
            IPropertyValue<T2> propertyValue2;

            if (result1.Messages is { Count: 0 } && result1.PropertyValue.HasValue())
            {
                // Output value.
                var value2 = MapFunc(result1.PropertyValue);

                propertyValue2 = new PropertyValue<T2>(Property, value2, ValueSource.Calculated);

                this.Validate(propertyValue2, out var messages2);

                return new ExtractResult<T2>(propertyValue2, messages2);
            }
            else
            {
                propertyValue2 = new PropertyValue<T2>(Property, default, ValueSource.NotDefined);
                return new ExtractResult<T2>(propertyValue2, result1.Messages);
            }
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

        public static IPropertyExtractContext<T> Extract<T>(this IExtractValidateMapContext extractValidateMapContext, IProperty<T> property)
        {
            return new PropertyExtractContext<T>(extractValidateMapContext, property);
        }

        public static IPropertyExtractContext<T> Validate<T>(this IPropertyExtractContext<T> propertyContext, IPropertyValue<T>? propertyValue, out Message[] messages)
        {
            messages = propertyContext.ValidationRules
                .SelectMany(rule => rule.Validate(propertyValue, propertyContext.Context.PropertyContainer))
                .ToArray();

            if (messages.Length > 0)
                propertyContext.Context.AddMessages(messages);

            return propertyContext;
        }

        public static IExtractValidateMapContext AddMessage(this IExtractValidateMapContext extractValidateMapContext, string message)
        {
            return extractValidateMapContext.AddMessages(new[] {new Message(message, severity: MessageSeverity.Error)});
        }

        public static IExtractValidateMapContext AddMessage(this IExtractValidateMapContext extractValidateMapContext, Message message)
        {
            return extractValidateMapContext.AddMessages(new[] { message });
        }

        /// <summary>
        /// Adds required check to property validation rules.
        /// </summary>
        /// <returns>The same instance.</returns>
        public static IPropertyExtractContext<T> Required<T>(this IPropertyExtractContext<T> propertyContext)
        {
            if (!propertyContext.ValidationRules.OfType<IRequiredPropertyValidationRule>().Any())
                propertyContext.ValidationRules.Add(propertyContext.Property.Required());
            return propertyContext;
        }

        /// <summary>
        /// Clears validation rules for property.
        /// </summary>
        /// <returns>The same instance.</returns>
        public static IPropertyExtractContext<T> NoValidation<T>(this IPropertyExtractContext<T> propertyContext)
        {
            propertyContext.ValidationRules.Clear();
            return propertyContext;
        }

        /// <summary>
        /// Adds validation rule.
        /// </summary>
        /// <param name="createValidationRule">Rule factory.</param>
        /// <returns>The same instance.</returns>
        public static IPropertyExtractContext<T> WithValidation<T>(this IPropertyExtractContext<T> propertyContext, Func<IProperty<T>, IPropertyValidationRule<T>> createValidationRule)
        {
            IPropertyValidationRule<T> propertyValidationRule = createValidationRule(propertyContext.Property);
            propertyContext.ValidationRules.Add(propertyValidationRule);
            return propertyContext;
        }

        /// <summary>
        /// Adds validation rules.
        /// </summary>
        /// <param name="validationRules">Validation rules.</param>
        /// <returns>The same instance.</returns>
        public static IPropertyExtractContext<T> WithValidationRules<T>(this IPropertyExtractContext<T> propertyContext, IEnumerable<IPropertyValidationRule<T>> validationRules)
        {
            propertyContext.ValidationRules.AddRange(validationRules);
            return propertyContext;
        }

        /// <summary>
        /// Final output method.
        /// Gets property value, validates it and outputs to result variable.
        /// </summary>
        /// <param name="value">Output variable.</param>
        /// <returns>Initial context.</returns>
        public static IExtractValidateMapContext Output<T>(this IPropertyExtractContext<T> propertyContext, out T value)
        {
            // Get value.
            var (propertyValue, _) = propertyContext.GetResult();

            // Output.
            value = propertyValue.Value!;

            return propertyContext.Context;
        }

        /// <summary>
        /// Final output method.
        /// Gets property value, validates it and outputs to result variable.
        /// </summary>
        /// <param name="propertyContext">Property context.</param>
        /// <param name="resultAction">Action with output result.</param>
        /// <returns>Initial context.</returns>
        public static IExtractValidateMapContext Output<T>(this IPropertyExtractContext<T> propertyContext, Action<ExtractResult<T>> resultAction)
        {
            // Get value.
            var result = propertyContext.GetResult();

            // Output.
            resultAction(result);

            return propertyContext.Context;
        }

        public static IExtractValidateMapContext Output<T, TModel>(this IPropertyExtractContext<T> propertyContext, TModel target)
        {
            if (target is null)
                return propertyContext.Context;

            // Get value.
            var result = propertyContext.GetResult();

            // Output.
            if (result.PropertyValue.HasValue())
            {
                (PropertyInfo? propertyInfo, object? value, Message? error) = PropertyContainerMapper.TryGetTargetPropertyAndValue(
                    result.PropertyValue,
                    target.GetType(),
                    null,
                    new MapToObjectSettings<TModel>()
                    {
                        SourceFilter = property => property == result.PropertyValue.PropertyUntyped,
                        //TargetName = property => propertyContext.Property.Name,
                    });

                if (propertyInfo != null)
                {
                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(target, value);
                    }
                    else
                    {
                        propertyContext.Context.AddMessage($"Property {propertyInfo.Name} is not writable");
                    }
                }
            }

            return propertyContext.Context;
        }

        public static PropertyExtractMapContext<T, T?> Optional<T>(this IPropertyExtractContext<T> context)
            where T : struct
        {
            static T? Nullify(IPropertyValue<T> propertyValue)
            {
                return propertyValue.HasValue() ? propertyValue.Value : null;
            }

            return Map<T, T?>(context, arg => Nullify(arg));
        }

        public static IPropertyExtractContext<T> ExcludeValidationRule<T>(this IPropertyExtractContext<T> context, Func<IPropertyValidationRule<T>, bool> excludePredicate)
        {
            var withoutExcluded = context.ValidationRules.Where(rule => !excludePredicate(rule));
            return context.WithValidationRules(withoutExcluded);
        }

        public static PropertyExtractMapContext<T1, T2> Map<T1, T2>(this IPropertyExtractContext<T1> context, Func<T1, T2> map)
        {
            return new PropertyExtractMapContext<T1, T2>(context, pv => map(pv.Value));
        }

        public static PropertyExtractMapContext<T1, T2> Map<T1, T2>(this IPropertyExtractContext<T1> context, Func<IPropertyValue<T1>, T2> map)
        {
            return new PropertyExtractMapContext<T1, T2>(context, map);
        }

        public static PropertyExtractMapContext<T2, T3> Map<T1, T2, T3>(this PropertyExtractMapContext<T1, T2> context, Func<IPropertyValue<T2>, T3> map)
        {
            return new PropertyExtractMapContext<T2, T3>(context, map);
        }

        public static PropertyExtractMapContext<T2, T3> Map<T1, T2, T3>(this PropertyExtractMapContext<T1, T2> context, Func<T2, T3> map)
        {
            return new PropertyExtractMapContext<T2, T3>(context, pv => map(pv.Value));
        }

        public static PropertyExtractMapContext<string, TEnum> MapToEnum<TEnum>(this IPropertyExtractContext<string> context)
            where TEnum : struct
        {
            static TEnum Parse(string text, IExtractValidateMapContext extractValidateMapContext)
            {
                if (Enum.TryParse(typeof(TEnum), text, ignoreCase: true, out object result))
                {
                    return (TEnum)result;
                }

                extractValidateMapContext.AddMessage(MappingError.NotEnumValue(typeof(TEnum), text));
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

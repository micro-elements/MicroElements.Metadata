// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Diagnostics;
using MicroElements.Functional;
using MicroElements.Metadata;
using Message = MicroElements.Diagnostics.Message;
using MessageSeverity = MicroElements.Diagnostics.MessageSeverity;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation rules customization extensions.
    /// </summary>
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Gets <see cref="ValidationMessageOptions"/> attached to <paramref name="validationRule"/>.
        /// </summary>
        /// <param name="validationRule">Validation rule to customize.</param>
        /// <returns><see cref="ValidationMessageOptions"/>.</returns>
        public static ValidationMessageOptions GetValidationMessageOptions(this IValidationRule validationRule)
        {
            return validationRule.GetMetadata<ValidationMessageOptions>(defaultValue: ValidationMessageOptions.Default)!;
        }

        /// <summary>
        /// Gets the last <see cref="IValidationRule"/> in current chain.
        /// If rule is <see cref="ICompositeValidationRule"/> then return <see cref="ICompositeValidationRule.LastRule"/>.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Validation rule to customize.</param>
        /// <returns>The same rule or last for composite rules.</returns>
        public static IValidationRule Last<TValidationRule>(this TValidationRule validationRule)
            where TValidationRule : IValidationRule
        {
            if (validationRule is ICompositeValidationRule compositeValidationRule)
                return compositeValidationRule.LastRule;
            return validationRule;
        }

        /// <summary>
        /// Configures <see cref="ValidationMessageOptions"/> metadata.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Validation rule to customize.</param>
        /// <param name="configureMetadata">Configure func.</param>
        /// <returns>The same validation rule.</returns>
        public static TValidationRule ConfigureValidationMessageOptions<TValidationRule>(
            this TValidationRule validationRule,
            Action<ValidationMessageOptions> configureMetadata)
            where TValidationRule : IValidationRule
        {
            validationRule.Last().ConfigureMetadata<ValidationMessageOptions>(configureMetadata);
            return validationRule;
        }

        /// <summary>
        /// Sets default message format.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Validation rule to customize.</param>
        /// <param name="messageFormat">Default message format.</param>
        /// <returns>The same validation rule.</returns>
        public static TValidationRule SetDefaultMessageFormat<TValidationRule>(this TValidationRule validationRule, string? messageFormat)
            where TValidationRule : IValidationRule
        {
            validationRule.ConfigureValidationMessageOptions(options => options.SetDefaultMessageFormat(messageFormat));
            return validationRule;
        }

        /// <summary>
        /// <inheritdoc cref="ValidationMessageOptions.GetConfiguredMessage"/>
        /// </summary>
        /// <param name="validationRule">Source validation rule.</param>
        /// <param name="propertyValue">Property and value to generate message.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <param name="messageFormat">Optional message format.</param>
        /// <returns>Configured message.</returns>
        public static Message GetConfiguredMessage(this IValidationRule validationRule, IPropertyValue propertyValue, IPropertyContainer propertyContainer, string? messageFormat = null)
        {
            return validationRule.GetValidationMessageOptions().GetConfiguredMessage(propertyValue, propertyContainer, messageFormat);
        }

        /// <summary>
        /// Configures validation message (Full version).
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="configureMessage">Configure message function.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule ConfigureMessage<TValidationRule>(this TValidationRule validationRule, Func<Message, IPropertyValue, IPropertyContainer, Message> configureMessage)
            where TValidationRule : IValidationRule
        {
            validationRule.ConfigureValidationMessageOptions(options => options.ConfigureMessage(configureMessage));
            return validationRule;
        }

        /// <summary>
        /// Configures validation message (Simple version).
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="configureMessage">Configure message function.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule ConfigureMessage<TValidationRule>(this TValidationRule validationRule, Func<Message, Message> configureMessage)
            where TValidationRule : IValidationRule
        {
            validationRule.ConfigureValidationMessageOptions(options => options.ConfigureMessage(configureMessage));
            return validationRule;
        }

        /// <summary>
        /// Sets validation message.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="messageFormat">New validation message format.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule WithMessage<TValidationRule>(this TValidationRule validationRule, string messageFormat)
            where TValidationRule : IValidationRule
        {
            validationRule.ConfigureValidationMessageOptions(options => options.ConfigureMessage(message => message.WithText(messageFormat)));
            return validationRule;
        }

        /// <summary>
        /// Sets validation message severity.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="severity">New validation message severity.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule WithSeverity<TValidationRule>(this TValidationRule validationRule, MessageSeverity severity)
            where TValidationRule : IValidationRule
        {
            validationRule.ConfigureValidationMessageOptions(options => options.ConfigureMessage(message => message.WithSeverity(severity)));
            return validationRule;
        }

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Error"/>.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule AsError<TValidationRule>(this TValidationRule validationRule)
            where TValidationRule : IValidationRule
        {
            return validationRule.WithSeverity(MessageSeverity.Error);
        }

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Warning"/>.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule AsWarning<TValidationRule>(this TValidationRule validationRule)
            where TValidationRule : IValidationRule
        {
            return validationRule.WithSeverity(MessageSeverity.Warning);
        }

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Information"/>.
        /// </summary>
        /// <typeparam name="TValidationRule">Validation rule type.</typeparam>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static TValidationRule AsInformation<TValidationRule>(this TValidationRule validationRule)
            where TValidationRule : IValidationRule
        {
            return validationRule.WithSeverity(MessageSeverity.Information);
        }
    }
}

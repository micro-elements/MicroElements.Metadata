// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MicroElements.Functional;

namespace MicroElements.Validation
{
    /// <summary>
    /// Validation rules customization extensions.
    /// </summary>
    public static class RuleBuilderExtensions
    {
        /// <summary>
        /// Sets validation message.
        /// </summary>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="messageFormat">New validation message format.</param>
        /// <returns>The same rule.</returns>
        public static IConfigurableValidationRule WithMessage(this IConfigurableValidationRule validationRule, string messageFormat)
        {
            validationRule.ConfigureMessage(message => message.WithText(messageFormat));
            return validationRule;
        }

        /// <summary>
        /// Sets validation message severity.
        /// </summary>
        /// <param name="validationRule">Rule to configure.</param>
        /// <param name="severity">New validation message severity.</param>
        /// <returns>The same rule.</returns>
        public static IConfigurableValidationRule WithSeverity(this IConfigurableValidationRule validationRule, MessageSeverity severity)
        {
            validationRule.ConfigureMessage(message => message.WithSeverity(severity));
            return validationRule;
        }

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Error"/>.
        /// </summary>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static IConfigurableValidationRule AsError(this IConfigurableValidationRule validationRule) =>
            validationRule.WithSeverity(MessageSeverity.Error);

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Warning"/>.
        /// </summary>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static IConfigurableValidationRule AsWarning(this IConfigurableValidationRule validationRule) =>
            validationRule.WithSeverity(MessageSeverity.Warning);

        /// <summary>
        /// Sets message severity to <see cref="MessageSeverity.Information"/>.
        /// </summary>
        /// <param name="validationRule">Rule to configure.</param>
        /// <returns>The same rule.</returns>
        public static IConfigurableValidationRule AsInformation(this IConfigurableValidationRule validationRule) =>
            validationRule.WithSeverity(MessageSeverity.Information);
    }
}

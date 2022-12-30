// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Diagnostics;
using MicroElements.Metadata;

namespace MicroElements.Validation
{
    /// <summary>
    /// Delegate for configuring validation message.
    /// </summary>
    /// <param name="builder">Message builder.</param>
    /// <param name="propertyValue">Property value (can be absent).</param>
    /// <param name="propertyContainer">Property container.</param>
    public delegate void ConfigureValidationMessage(IMessageBuilder builder, IPropertyValue? propertyValue, IPropertyContainer propertyContainer);

    /// <summary>
    /// Validation message options.
    /// Can be used as external behavior in <see cref="IValidationRule"/>.
    /// </summary>
    public class ValidationMessageOptions
    {
        /// <summary>
        /// Default validation options.
        /// </summary>
        public static readonly ValidationMessageOptions Default = new ValidationMessageOptions();

        /// <summary>
        /// Default message format.
        /// </summary>
        public const string DefaultMessageFormat = "Property {propertyName} has invalid value {value}";

        /// <summary>
        /// Gets default message format.
        /// </summary>
        public string MessageFormat { get; private set; } = DefaultMessageFormat;

        /// <summary>
        /// Optional message configuration chain.
        /// </summary>
        private readonly Lazy<List<ConfigureValidationMessage>> _configureMessageChainLazy = new();

        /// <summary>
        /// Sets default message format.
        /// </summary>
        /// <param name="defaultMessageFormat">Default message format.</param>
        public void SetDefaultMessageFormat(string? defaultMessageFormat = null)
        {
            MessageFormat = defaultMessageFormat ?? DefaultMessageFormat;
        }

        /// <summary>
        /// Adds configure message.
        /// </summary>
        /// <param name="configureMessage">Configure message function.</param>
        public void ConfigureMessage(ConfigureValidationMessage configureMessage)
        {
            _configureMessageChainLazy.Value.Add(configureMessage);
        }

        /// <summary>
        /// Adds configure message.
        /// </summary>
        /// <param name="configureMessage">Configure message function.</param>
        public void ConfigureMessage(ConfigureMessage configureMessage)
        {
            _configureMessageChainLazy.Value.Add((message, propertyValue, container) => configureMessage(message));
        }

        /// <summary>
        /// Gets configured message.
        /// 1. Creates message with <see cref="MessageFormat"/> text.
        /// 2. Adds property values {propertyName}, {propertyType}, {propertyDescription} and {value} for using in templated messages.
        /// 3. Applies ConfigureMessage chain.
        /// </summary>
        /// <param name="propertyValue">Property and value to generate message.</param>
        /// <param name="propertyContainer">Property container that holds value.</param>
        /// <param name="messageFormat">Optional message format.</param>
        /// <returns>Configured message.</returns>
        public Message GetConfiguredMessage(IPropertyValue? propertyValue, IPropertyContainer propertyContainer, string? messageFormat = null)
        {
            var messageBuilder = MessageBuilder.Error(messageFormat ?? MessageFormat, capacity: 8);

            if (propertyValue?.PropertyUntyped is { } property)
            {
                messageBuilder
                    .AddProperty("propertyName", property.Name)
                    .AddProperty("propertyType", property.Type)
                    .AddProperty("propertyDescription", property.Description ?? string.Empty)
                    .AddProperty("value", propertyValue.ValueUntyped ?? "null");
            }

            if (_configureMessageChainLazy.IsValueCreated)
            {
                foreach (var configureMessage in _configureMessageChainLazy.Value)
                {
                    configureMessage(messageBuilder, propertyValue, propertyContainer);
                }
            }

            Message configuredMessage = messageBuilder.Build();
            return configuredMessage;
        }
    }
}

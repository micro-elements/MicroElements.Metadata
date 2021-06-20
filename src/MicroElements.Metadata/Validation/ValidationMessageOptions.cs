// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Metadata;
using Message = MicroElements.Diagnostics.Message;
using MessageSeverity = MicroElements.Diagnostics.MessageSeverity;

namespace MicroElements.Validation
{
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
        private readonly Lazy<List<Func<Message, IPropertyValue, IPropertyContainer, Message>>> _configureMessageChainLazy = new ();

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
        public void ConfigureMessage(Func<Message, IPropertyValue, IPropertyContainer, Message> configureMessage)
        {
            _configureMessageChainLazy.Value.Add(configureMessage);
        }

        /// <summary>
        /// Adds configure message.
        /// </summary>
        /// <param name="configureMessage">Configure message function.</param>
        public void ConfigureMessage(Func<Message, Message> configureMessage)
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
            KeyValuePair<string, object>[]? properties = null;

            if (propertyValue != null)
            {
                IProperty property = propertyValue.PropertyUntyped;
                properties = new[]
                {
                    new KeyValuePair<string, object>("propertyName", property.Name),
                    new KeyValuePair<string, object>("propertyType", property.Type),
                    new KeyValuePair<string, object>("propertyDescription", property.Description),
                    new KeyValuePair<string, object>("value", propertyValue.ValueUntyped),
                };
            }

            Message message = new Message(messageFormat ?? MessageFormat, MessageSeverity.Error, properties: properties);

            if (_configureMessageChainLazy.IsValueCreated)
            {
                foreach (var configureMessage in _configureMessageChainLazy.Value)
                {
                    message = configureMessage(message, propertyValue, propertyContainer);
                }
            }

            return message;
        }
    }
}

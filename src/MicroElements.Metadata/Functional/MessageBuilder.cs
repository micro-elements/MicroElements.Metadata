// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Functional
{
    public delegate void ConfigureMessageRef(ref ValueMessageBuilder builder);

    public delegate void ConfigureMessage(IMessageBuilder builder);

    public interface IMessageBuilder
    {
        MessageState State { get; }
        IMessageBuilder WithOriginalMessage(string originalMessage);
        IMessageBuilder AppendToOriginalMessage(string text);
        IMessageBuilder WithSeverity(MessageSeverity severity);
        IMessageBuilder WithEventName(string eventName);
        IMessageBuilder WithTimeStamp(DateTimeOffset? timestamp);
        IMessageBuilder AddProperty(string name, object? value, bool allowNull = false);
        IMessageBuilder SetProperty(string name, object? value, bool allowNull = false);
        Message Build();
    }

    public struct MessageState
    {
        public List<KeyValuePair<string, object?>> Properties;
        public string OriginalMessage;
        public MessageSeverity Severity;
        public string? EventName;
        public DateTimeOffset? Timestamp;

        public static MessageState From(MessageStateRef state)
        {
            return new MessageState()
            {
                Properties = state.Properties,
                OriginalMessage = state.OriginalMessage,
                Severity = state.Severity,
                EventName = state.EventName,
                Timestamp = state.Timestamp,
            };
        }
    }

    public ref struct MessageStateRef
    {
        public List<KeyValuePair<string, object?>> Properties;
        public string OriginalMessage;
        public MessageSeverity Severity;
        public string? EventName;
        public DateTimeOffset? Timestamp;
    }

    public class MessageBuilder : IMessageBuilder
    {
        private MessageState _state;

        public MessageState State => _state;

        public MessageBuilder(ValueMessageBuilder builder)
        {
            _state = MessageState.From(builder._state);
        }

        public MessageBuilder(string? originalMessage, int capacity = 7)
        {
            _state = new MessageState
            {
                OriginalMessage = originalMessage ?? string.Empty,
                Properties = new List<KeyValuePair<string, object?>>(capacity),
                Severity = MessageSeverity.Information,
                EventName = null,
                Timestamp = null,
            };
        }

        public static IMessageBuilder Error(string originalMessage, int capacity = 7)
        {
            return new MessageBuilder(originalMessage, capacity).WithSeverity(MessageSeverity.Error);
        }

        /// <inheritdoc />
        public IMessageBuilder WithOriginalMessage(string originalMessage)
        {
            _state.OriginalMessage = originalMessage;
            return this;
        }

        public IMessageBuilder AppendToOriginalMessage(string text)
        {
            _state.OriginalMessage += text;
            return this;
        }

        public IMessageBuilder WithSeverity(MessageSeverity severity)
        {
            _state.Severity = severity;
            return this;
        }

        public IMessageBuilder WithEventName(string eventName)
        {
            _state.EventName = eventName;
            return this;
        }

        public IMessageBuilder WithTimeStamp(DateTimeOffset? timestamp)
        {
            _state.Timestamp = timestamp;
            return this;
        }

        public IMessageBuilder AddProperty(string name, object? value, bool allowNull = false)
        {
            _state.Properties.AddProperty(name, value, allowNull);
            return this;
        }

        public IMessageBuilder SetProperty(string name, object? value, bool allowNull = false)
        {
            _state.Properties.SetProperty(name, value, allowNull);
            return this;
        }

        /// <inheritdoc />
        public Message Build()
        {
            return new Message(
                originalMessage: _state.OriginalMessage,
                properties: _state.Properties,
                severity: _state.Severity,
                eventName: _state.EventName,
                timestamp: _state.Timestamp);
        }
    }

    public ref struct ValueMessageBuilder
    {
        internal MessageStateRef _state;

        public MessageStateRef State => _state;

        public ValueMessageBuilder(string? originalMessage, int capacity = 7)
        {
            _state = new MessageStateRef
            {
                OriginalMessage = originalMessage ?? string.Empty,
                Properties = new List<KeyValuePair<string, object?>>(capacity),
                Severity = MessageSeverity.Information,
                EventName = null,
                Timestamp = null,
            };
        }

        public ValueMessageBuilder(Message message, int addCapacity = 3)
        {
            var properties = new List<KeyValuePair<string, object>>(message.Properties.Count + addCapacity);
            properties.AddRange(message.Properties);

            _state = new MessageStateRef
            {
                OriginalMessage = message.OriginalMessage,
                Properties = properties,
                Severity = MessageSeverity.Information,
                EventName = null,
                Timestamp = null,
            };
        }

        public ValueMessageBuilder(in MessageStateRef state)
        {
            _state = state;
        }

        public static implicit operator Message(ValueMessageBuilder builder) => builder.Build();

        public static ValueMessageBuilder FromMessage(Message message, int addCapacity = 3)
        {
            return new ValueMessageBuilder(message, addCapacity);
        }

        public static ValueMessageBuilder Error(string originalMessage, int capacity = 7)
        {
            return new ValueMessageBuilder(originalMessage, capacity).WithSeverity(MessageSeverity.Error);
        }

        public ValueMessageBuilder WithOriginalMessage(string originalMessage)
        {
            _state.OriginalMessage = originalMessage;
            return this;
        }

        public ValueMessageBuilder AppendToOriginalMessage(string text)
        {
            _state.OriginalMessage += text;
            return this;
        }

        public ValueMessageBuilder WithSeverity(MessageSeverity severity)
        {
            _state.Severity = severity;
            return this;
        }

        public ValueMessageBuilder WithEventName(string eventName)
        {
            _state.EventName = eventName;
            return this;
        }

        public ValueMessageBuilder WithTimeStamp(DateTimeOffset? timestamp)
        {
            _state.Timestamp = timestamp;
            return this;
        }

        public ValueMessageBuilder AddProperty(string name, object? value, bool allowNull = false)
        {
            _state.Properties.AddProperty(name, value, allowNull);

            return this;
        }

        public ValueMessageBuilder SetProperty(string name, object? value, bool allowNull = false)
        {
            _state.Properties.SetProperty(name, value, allowNull);

            return this;
        }

        public MessageBuilderPredicate If(bool predicate)
        {
            return new MessageBuilderPredicate(predicate, ref this);
        }

        public ValueMessageBuilder If(bool predicate, ConfigureMessageRef configure)
        {
            if (predicate)
            {
                configure(ref this);
            }

            return this;
        }

        public Message Build()
        {
            return new Message(
                originalMessage: _state.OriginalMessage,
                properties: _state.Properties,
                severity: _state.Severity,
                eventName: _state.EventName,
                timestamp: _state.Timestamp);
        }
    }

    public ref struct MessageBuilderPredicate
    {
        private readonly bool _predicate;
        private MessageStateRef _state;

        public MessageBuilderPredicate(Func<bool> predicate, ref ValueMessageBuilder messageBuilder)
        {
            _predicate = predicate();
            _state = messageBuilder._state;
        }

        public MessageBuilderPredicate(bool predicate, ref ValueMessageBuilder messageBuilder)
        {
            _predicate = predicate;
            _state = messageBuilder._state;
        }

        public MessageBuilderPredicate(bool predicate, MessageStateRef messageState)
        {
            _predicate = predicate;
            _state = messageState;
        }

        public MessageBuilderPredicate AppendToOriginalMessage(string text)
        {
            if (_predicate)
                _state.OriginalMessage += text;

            return this;
        }

        public MessageBuilderPredicate AddProperty(string name, object? value, bool allowNull = false)
        {
            if (_predicate)
                _state.Properties.AddProperty(name, value, allowNull);
            return this;
        }

        public MessageBuilderPredicate SetProperty(string name, object? value, bool allowNull = false)
        {
            if (_predicate)
                _state.Properties.SetProperty(name, value, allowNull);
            return this;
        }

        public ValueMessageBuilder EndIf()
        {
            return new ValueMessageBuilder(_state);
        }
    }

    internal static class MessageBuilderUtils
    {
        public static void AddProperty(this List<KeyValuePair<string, object?>> properties, string name, object? value, bool allowNull = false)
        {
            if (value != null || allowNull)
            {
                var property = new KeyValuePair<string, object?>(name, value);
                properties.Add(property);
            }
        }

        public static void SetProperty(this List<KeyValuePair<string, object?>> properties, string name, object? value, bool allowNull = false)
        {
            if (value != null || allowNull)
            {
                var property = new KeyValuePair<string, object?>(name, value);

                for (int i = 0; i < properties.Count; i++)
                {
                    if (properties[i].Key == name)
                    {
                        properties[i] = property;
                    }
                }

                properties.Add(property);
            }
        }
    }
}

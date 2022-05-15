// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.CodeContracts;
using MicroElements.Diagnostics;
using MicroElements.Metadata;
using MicroElements.Metadata.Schema;
using PropertyAddMode = MicroElements.Metadata.PropertyAddMode;

namespace MicroElements.StructuredMessage
{
    public class MessageSchema : IStaticSchema
    {
        public static IProperty<DateTimeOffset> Timestamp = new Property<DateTimeOffset>("Timestamp");
        public static IProperty<MessageSeverity> Severity = new Property<MessageSeverity>("Severity");
        public static IProperty<string> OriginalMessage = new Property<string>("OriginalMessage");
        public static IProperty<string> FormattedMessage = new Property<string>("FormattedMessage");
        public static IProperty<string?> EventName = new Property<string?>("EventName");
    }

    public interface IMessage
    {
        /// <summary>
        /// Gets date and time of message occurred.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets message severity.
        /// </summary>
        MessageSeverity Severity { get; }

        /// <summary>
        /// Gets original message.
        /// <para>OriginalMessage can be a pattern in form of MessageTemplates.org.</para>
        /// </summary>
        string OriginalMessage { get; }

        /// <summary>
        /// Gets event name.
        /// </summary>
        string? EventName { get; }

        /// <summary>
        /// Gets message properties.
        /// </summary>
        IReadOnlyCollection<KeyValuePair<string, object?>> Properties { get; }

        /// <summary>
        /// Gets formatted message.
        /// <para>It's a result of MessageTemplate rendered with <seealso cref="Properties"/>.</para>
        /// </summary>
        string FormattedMessage { get; }
    }

    public class StructuredMessage :
        PropertyContainer2<MessageSchema>,
        IMessage,
        IReadOnlyListAdapter<StructuredMessage>,
        ICompositeBuilder<StructuredMessage, KeyValuePair<string, object?>>
    {
        private static IPropertyFactory Factory { get; } = PropertyFactory.Default.Cached(64).WithPredefinedProperties(new MessageSchema());

        public StructuredMessage(
            IEnumerable<IPropertyValue>? sourceValues = null,
            IMessageTemplateParser? messageTemplateParser = null,
            IMessageTemplateRenderer? messageTemplateRenderer = null)
            : base(sourceValues, null, null)
        {
            if (OriginalMessage is { } originalMessage)
            {
                var formattedMessage = RenderFormattedMessage(originalMessage, _propertyContainer, messageTemplateParser, messageTemplateRenderer);
                if (formattedMessage != originalMessage)
                    _propertyContainer = _propertyContainer.WithValue(MessageSchema.FormattedMessage, formattedMessage);
            }
        }

        public StructuredMessage(
            string originalMessage,
            MessageSeverity severity = MessageSeverity.Information,
            DateTimeOffset? timestamp = null,
            string? eventName = null,
            IPropertyContainer? properties = null,
            IMessageTemplateParser? messageTemplateParser = null,
            IMessageTemplateRenderer? messageTemplateRenderer = null)
            : base(CreateMessage(
                originalMessage: originalMessage,
                severity: severity,
                timestamp: timestamp,
                eventName: eventName,
                properties: properties,
                messageTemplateParser: messageTemplateParser,
                messageTemplateRenderer: messageTemplateRenderer))
        {
        }

        public static IPropertyContainer CreateMessage(
            string originalMessage,
            MessageSeverity severity = MessageSeverity.Information,
            DateTimeOffset? timestamp = null,
            string? eventName = null,
            IPropertyContainer? properties = null,
            IMessageTemplateParser? messageTemplateParser = null,
            IMessageTemplateRenderer? messageTemplateRenderer = null)
        {
            originalMessage.AssertArgumentNotNull(nameof(originalMessage));

            var formattedMessage = RenderFormattedMessage(originalMessage, properties, messageTemplateParser, messageTemplateRenderer);

            return new MutablePropertyContainer(sourceValues: properties)
                .WithValue(MessageSchema.OriginalMessage, originalMessage)
                .WithValue(MessageSchema.Severity, severity)
                .WithValue(MessageSchema.Timestamp, timestamp ?? DateTimeOffset.Now)
                .WithValue(MessageSchema.EventName, eventName)
                .WithValue(MessageSchema.FormattedMessage, formattedMessage);
        }

        private static string? RenderFormattedMessage(
            string? originalMessage,
            IPropertyContainer? properties,
            IMessageTemplateParser? messageTemplateParser,
            IMessageTemplateRenderer? messageTemplateRenderer)
        {
            string? formattedMessage = originalMessage;

            if (originalMessage != null)
            {
                bool canBeTemplatedMessage = originalMessage.Contains('{');
                if (canBeTemplatedMessage && properties != null)
                {
                    messageTemplateParser ??= MessageTemplateParser.Instance;
                    messageTemplateRenderer ??= MessageTemplateRenderer.Instance;

                    var messageTemplate = messageTemplateParser.Parse(originalMessage);
                    var propertiesForTemplate = properties.AsReadOnlyDictionary();
                    formattedMessage = messageTemplateRenderer.TryRenderToString(messageTemplate, propertiesForTemplate, originalMessage);
                }
            }

            return formattedMessage;
        }

        /// <inheritdoc />
        public DateTimeOffset Timestamp => GetValue(MessageSchema.Timestamp);

        /// <inheritdoc />
        public MessageSeverity Severity => GetValue(MessageSchema.Severity);

        /// <inheritdoc />
        public string OriginalMessage => GetValue(MessageSchema.OriginalMessage)!;

        /// <inheritdoc />
        public string? EventName => GetValue(MessageSchema.EventName);

        /// <inheritdoc />
        IReadOnlyCollection<KeyValuePair<string, object>> IMessage.Properties => this.AsReadOnlyList();

        /// <inheritdoc />
        public string FormattedMessage => GetValue(MessageSchema.FormattedMessage);

        /// <inheritdoc />
        public KeyValuePair<string, object?> this[int index] => Properties.Skip(index).First().ToKeyValuePair();

        /// <inheritdoc />
        public StructuredMessage With(KeyValuePair<string, object?> propertyValue)
        {
            if (propertyValue.Value != null)
            {
                IProperty property = Factory.Create(propertyValue.Value.GetType(), propertyValue.Key);
                IPropertyValue propertyValueUntyped = PropertyValueFactory.Default.CreateUntyped(property, propertyValue.Value);
                var propertyContainer = _propertyContainer.WithValue(propertyValueUntyped, PropertyAddMode.Set);
                return new StructuredMessage(propertyContainer);
            }

            return this;
        }
    }

    public interface IReadOnlyListAdapter<TPropertyContainer> : IReadOnlyList<KeyValuePair<string, object?>>
        where TPropertyContainer : IPropertyContainer
    {
        IPropertyContainer PropertyContainer => (TPropertyContainer)this;

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() =>
            PropertyContainer.Properties.Select(value => value.ToKeyValuePair()).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => PropertyContainer.Count;

        /// <inheritdoc />
        KeyValuePair<string, object?> IReadOnlyList<KeyValuePair<string, object?>>.this[int index]
            => PropertyContainer.Properties.Skip(index).First().ToKeyValuePair();
    }

    // TODO: Serilog uses: IEnumerable<KeyValuePair<string, object>>
    // TODO: NLog uses: IReadOnlyList<KeyValuePair<string, object>>
    public class LazyReadOnlyListAdapter : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly IPropertyContainer _propertyContainer;

        public LazyReadOnlyListAdapter(IPropertyContainer propertyContainer) =>
            _propertyContainer = propertyContainer;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() =>
            _propertyContainer.Properties.Select(value => value.ToKeyValuePair()).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _propertyContainer.Count;

        /// <inheritdoc />
        public KeyValuePair<string, object?> this[int index] => _propertyContainer.Properties.Skip(index).First().ToKeyValuePair();
    }

    public static class ListAdapterExtensions
    {
        internal static KeyValuePair<string, object?> ToKeyValuePair(this IPropertyValue propertyValue) =>
            new (propertyValue.PropertyUntyped.Name, propertyValue.ValueUntyped);

        public static IReadOnlyList<KeyValuePair<string, object?>> AsReadOnlyList(this IPropertyContainer propertyContainer)
            => new LazyReadOnlyListAdapter(propertyContainer);
    }
}

using System.Collections.Generic;

namespace MicroElements.Diagnostics
{
    public static class MessageListExtensions2
    {
        /// <summary>
        /// Creates MessageList from <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TMessage">Message type.</typeparam>
        /// <param name="messages">Messages.</param>
        /// <returns>MessageList.</returns>
        public static Diagnostics.MessageList<TMessage> ToMessageList<TMessage>(this IEnumerable<TMessage> messages)
            => Diagnostics.MessageList.FromEnumerable(messages);

        /// <summary>
        /// Creates MessageList with one message.
        /// </summary>
        /// <typeparam name="TMessage">Message type.</typeparam>
        /// <param name="message">Single message.</param>
        /// <returns>MessageList.</returns>
        public static Diagnostics.MessageList<TMessage> ToMessageList<TMessage>(this TMessage message)
            => new Diagnostics.MessageList<TMessage>(message);
    }
}

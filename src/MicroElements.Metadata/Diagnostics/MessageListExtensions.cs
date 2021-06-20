// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MicroElements.Diagnostics
{
    /// <summary>
    /// MessageListExtensions.
    /// </summary>
    public static class MessageListExtensions
    {
        /// <summary>
        /// Добавляет ошибку в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="errorMessage">Сообщение.</param>
        /// <returns>Созданный <see cref="Message"/>.</returns>
        public static IMessageList<Message>? AddError(this IMessageList<Message>? messageList, string errorMessage)
        {
            return messageList?.Add(new Message(errorMessage, MessageSeverity.Error));
        }

        /// <summary>
        /// Добавляет ошибку в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="exception">Опциональное исключение.</param>
        /// <param name="errorMessage">Сообщение.</param>
        /// <returns>Созданный <see cref="Message"/>.</returns>
        public static IMessageList<Message>? AddError(
            this IMessageList<Message>? messageList,
            Exception exception,
            string errorMessage,
            Func<Exception, IReadOnlyList<KeyValuePair<string, object>>> exceptionExtract)
        {
            Message message = new Message(errorMessage, MessageSeverity.Error)
                .WithProperties(exceptionExtract(exception));
            return messageList?.Add(message);
        }

        /// <summary>
        /// Добавляет предупреждение в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="message">Сообщение.</param>
        /// <returns>Созданный <see cref="Message"/>.</returns>
        public static IMessageList<Message>? AddWarning(this IMessageList<Message>? messageList, string message)
        {
            Message msg = new Message(message, MessageSeverity.Warning);
            return messageList?.Add(msg);
        }

        /// <summary>
        /// Добавляет информацию в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="message">Сообщение.</param>
        /// <returns>Созданный <see cref="Message"/>.</returns>
        public static IMessageList<Message>? AddInformation(this IMessageList<Message>? messageList, string message)
        {
            Message msg = new Message(message, MessageSeverity.Information);
            return messageList?.Add(msg);
        }
    }
}

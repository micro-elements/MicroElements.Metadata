// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using MicroElements.Functional;

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
        /// <returns>Созданный <see cref="Diagnostics.Message"/></returns>
        public static Diagnostics.IMessageList<Diagnostics.Message>? AddError(this Diagnostics.IMessageList<Diagnostics.Message>? messageList, string errorMessage)
        {
            return messageList?.Add(new Diagnostics.Message(errorMessage, MessageSeverity.Error));
        }

        /// <summary>
        /// Добавляет ошибку в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="exception">Опциональное исключение.</param>
        /// <param name="errorMessage">Сообщение.</param>
        /// <returns>Созданный <see cref="Diagnostics.Message"/></returns>
        public static Diagnostics.IMessageList<Diagnostics.Message>? AddError(
            this Diagnostics.IMessageList<Diagnostics.Message>? messageList,
            Exception exception,
            string errorMessage,
            Func<Exception, IReadOnlyList<KeyValuePair<string, object>>> exceptionExtract)
        {
            Diagnostics.Message message = new Diagnostics.Message(errorMessage, MessageSeverity.Error)
                .WithProperties(exceptionExtract(exception));
            return messageList?.Add(message);
        }

        /// <summary>
        /// Добавляет предупреждение в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="message">Сообщение.</param>
        /// <returns>Созданный <see cref="Diagnostics.Message"/></returns>
        public static Diagnostics.IMessageList<Diagnostics.Message>? AddWarning(this Diagnostics.IMessageList<Diagnostics.Message>? messageList, string message)
        {
            Diagnostics.Message msg = new Diagnostics.Message(message, MessageSeverity.Warning);
            return messageList?.Add(msg);
        }

        /// <summary>
        /// Добавляет информацию в список.
        /// </summary>
        /// <param name="messageList">Список сообщений.</param>
        /// <param name="message">Сообщение.</param>
        /// <returns>Созданный <see cref="Diagnostics.Message"/></returns>
        public static Diagnostics.IMessageList<Diagnostics.Message>? AddInformation(this Diagnostics.IMessageList<Diagnostics.Message>? messageList, string message)
        {
            Diagnostics.Message msg = new Diagnostics.Message(message, MessageSeverity.Information);
            return messageList?.Add(msg);
        }
    }
}

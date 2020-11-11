// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata;

namespace MicroElements.Reporting.Excel
{
    /// <summary>
    /// Extension methods for working with metadata.
    /// </summary>
    public static class ExcelMetadataExtensions
    {
        /// <summary>
        /// Takes configure action and combines action with new action.
        /// </summary>
        /// <typeparam name="TContainer">Container type.</typeparam>
        /// <typeparam name="TContext">Action type.</typeparam>
        /// <param name="metadata">Metadata container.</param>
        /// <param name="property">Property with action.</param>
        /// <param name="action">Action to add.</param>
        /// <returns>The same container.</returns>
        public static TContainer WithCombinedConfigure<TContainer, TContext>(
            this TContainer metadata,
            IProperty<Action<TContext>> property,
            Action<TContext> action)
            where TContainer : IMutablePropertyContainer
        {
            Action<TContext>? existedAction = metadata.GetPropertyValue(property)?.Value;

            return metadata.WithValue(property, context => Combine(context, existedAction, action));

            static void Combine(TContext context, Action<TContext>? action1, Action<TContext> action2)
            {
                action1?.Invoke(context);
                action2(context);
            }
        }
    }
}

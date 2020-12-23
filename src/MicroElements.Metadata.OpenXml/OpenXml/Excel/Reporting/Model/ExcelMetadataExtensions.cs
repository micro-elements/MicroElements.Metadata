// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using MicroElements.Metadata.OpenXml.Excel.Styling;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
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
        /// <param name="combineMode">Method combine mode.</param>
        /// <returns>The same container.</returns>
        public static TContainer WithCombinedConfigure<TContainer, TContext>(
            this TContainer metadata,
            IProperty<Action<TContext>> property,
            Action<TContext> action,
            CombineMode combineMode = CombineMode.AppendToEnd)
            where TContainer : IMutablePropertyContainer
        {
            Action<TContext>? existedAction = metadata.GetPropertyValue(property)?.Value;

            return metadata.WithValue(property, context => Combine(context, existedAction, action, combineMode));

            static void Combine(TContext context, Action<TContext>? action1, Action<TContext> action2, CombineMode combineMode)
            {
                switch (combineMode)
                {
                    case CombineMode.Set:
                        Set(context, action1, action2);
                        break;
                    case CombineMode.AppendToEnd:
                        AppendToEnd(context, action1, action2);
                        break;
                    case CombineMode.AppendToStart:
                        AppendToStart(context, action1, action2);
                        break;
                }
            }

            static void Set(TContext context, Action<TContext>? action1, Action<TContext> action2)
            {
                action2(context);
            }

            static void AppendToEnd(TContext context, Action<TContext>? action1, Action<TContext> action2)
            {
                action1?.Invoke(context);
                action2(context);
            }

            static void AppendToStart(TContext context, Action<TContext>? action1, Action<TContext> action2)
            {
                action2(context);
                action1?.Invoke(context);
            }
        }
    }
}

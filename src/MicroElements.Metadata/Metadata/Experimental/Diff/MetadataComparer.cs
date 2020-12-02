// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata.Diff
{
    /// <summary>
    /// MetadataComparer.
    /// </summary>
    public static class MetadataComparer
    {
        /// <summary>
        /// Gets diff of two property containers.
        /// </summary>
        public static ObjectDiff GetDiff(
            IPropertyContainer container1,
            IPropertyContainer container2,
            Func<IPropertyValue, string>? renderValue = null,
            bool compareByRenderedValues = false,
            SearchOptions? searchOptions = null)
        {
            renderValue ??= FormatForDiff;
            var search = searchOptions ?? Search.ExistingOnlyWithParent.WithPropertyComparer(PropertyComparer.ByTypeAndNameComparer);

            IEnumerable<(IPropertyValue? PropertyValue1, IPropertyValue PropertyValue2)> propertyPairs =
                container2.Select(propertyValue2 => (container1.GetPropertyValueUntyped(propertyValue2.PropertyUntyped, search), propertyValue2));

            IEnumerable<(IPropertyValue PropertyValue1, IPropertyValue? PropertyValue2)> removedPropertyPairs =
                container1
                    .Select(propertyValue1 => (propertyValue1, container2.GetPropertyValueUntyped(propertyValue1.PropertyUntyped, search)))
                    .Where(tuple => tuple.Item2 == null);

            var propertyPairsAll = propertyPairs.Concat(removedPropertyPairs);

            PropertyDiff[]? diffPairs;

            if (compareByRenderedValues)
            {
                diffPairs = propertyPairsAll
                    .Select(propertyPair => PropertyDiff(propertyPair))
                    .Where(diff => !Equals(diff.NewValue, diff.OldValue))
                    .ToArray();
            }
            else
            {
                diffPairs = propertyPairsAll
                    .Where(propertyPair => !EqualsForDiff(propertyPair.PropertyValue1?.ValueUntyped, propertyPair.PropertyValue2?.ValueUntyped))
                    .Select(propertyPair => PropertyDiff(propertyPair))
                    .ToArray();
            }

            PropertyDiff PropertyDiff((IPropertyValue? PropertyValue1, IPropertyValue? PropertyValue2) valueTuple)
            {
                string propertyName = (valueTuple.PropertyValue1?.PropertyUntyped.Name ?? valueTuple.PropertyValue2?.PropertyUntyped.Name)!;
                return new PropertyDiff(
                    propertyName,
                    valueTuple.PropertyValue1 != null ? renderValue(valueTuple.PropertyValue1) : null,
                    valueTuple.PropertyValue2 != null? renderValue(valueTuple.PropertyValue2) : null);
            }

            return new ObjectDiff(diffPairs);
        }

        private static bool EqualsForDiff(object? value1, object? value2)
        {
            if (value1 is ICollection collection1 && value2 is ICollection collection2)
                return collection1.SequenceEqualUntyped(collection2);
            return Equals(value1, value2);
        }

        private static string FormatForDiff(IPropertyValue propertyValue)
        {
            string formatAsTuple = propertyValue.ValueUntyped.DefaultFormatValue();
            return formatAsTuple;
        }
    }

    /// <summary>
    /// Object diff.
    /// </summary>
    public class ObjectDiff
    {
        /// <summary>
        /// Diff list.
        /// </summary>
        public PropertyDiff[] Diffs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDiff"/> class.
        /// </summary>
        /// <param name="key">Entity key.</param>
        /// <param name="diffs">Diff list.</param>
        public ObjectDiff(PropertyDiff[] diffs)
        {
            Diffs = diffs.AssertArgumentNotNull(nameof(diffs));
        }
    }

    /// <summary>
    /// Property diff.
    /// </summary>
    public class PropertyDiff
    {
        /// <summary>
        /// Property name.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Old value.
        /// </summary>
        public string? OldValue { get; }

        /// <summary>
        /// New value.
        /// </summary>
        public string? NewValue { get; }

        /// <summary>
        /// Creates new diff.
        /// </summary>
        /// <param name="property">Property name.</param>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        public PropertyDiff(string property, string? oldValue, string? newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Property)}: {Property}, {nameof(OldValue)}: {OldValue}, {nameof(NewValue)}: {NewValue}";
    }

    public static partial class Enumerable
    {
        public static bool SequenceEqualUntyped(this ICollection first, ICollection second)
        {
            IEnumerator e1 = first.GetEnumerator();
            IEnumerator e2 = second.GetEnumerator();

            while (e1.MoveNext())
            {
                if (!(e2.MoveNext() && Equals(e1.Current, e2.Current)))
                {
                    return false;
                }
            }

            return !e2.MoveNext();
        }
    }
}

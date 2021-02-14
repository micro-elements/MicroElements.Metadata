// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// OpenXml Cell factory.
    /// </summary>
    public interface IOpenXmlCellFactory
    {
        /// <summary>
        /// Creates cell.
        /// </summary>
        /// <param name="cellText">Cell text.</param>
        /// <param name="dataType">Data type.</param>
        /// <returns>New cell instance.</returns>
        Cell CreateCell(string? cellText, CellValues dataType);
    }

    /// <summary>
    /// Default factory that creates cell on ordinary manner.
    /// </summary>
    public class DefaultOpenXmlCellFactory : IOpenXmlCellFactory
    {
        /// <inheritdoc />
        public Cell CreateCell(string? cellText, CellValues dataType)
        {
            Cell cell = new Cell
            {
                CellValue = new CellValue(cellText),
                DataType = new EnumValue<CellValues>(dataType),
            };

            return cell;
        }
    }

    /// <summary>
    /// Uses cached <see cref="EnumValue{T}"/> and <see cref="CellValue"/> to eliminate GC small object pressure.
    /// HACK: <see cref="CellValue"/> is <see cref="OpenXmlElement"/> so <see cref="OpenXmlElement.Parent"/> will be set on add to cell. So we need to clear parent every type to reuse <see cref="CellValue"/>.
    /// </summary>
    public class CachedOpenXmlCellFactory : IOpenXmlCellFactory
    {
        private class Cache
        {
            internal readonly ConcurrentDictionary<(string, CellValues), CellValue> CellValues = new ConcurrentDictionary<(string, CellValues), CellValue>();
            internal readonly ConcurrentDictionary<CellValues, EnumValue<CellValues>> EnumValues = new ConcurrentDictionary<CellValues, EnumValue<CellValues>>();

            internal readonly Action<CellValue, OpenXmlElement> SetParent = ExpressionUtils.GetPropertySetter<CellValue, OpenXmlElement>(value => value.Parent);
        }

        private readonly Cache _cache = new Cache();

        /// <inheritdoc />
        public Cell CreateCell(string? cellText, CellValues dataType)
        {
            CellValue? cellValue = null;
            EnumValue<CellValues> dataTypeValue = null;

            if (cellText != null)
            {
                cellValue = _cache.CellValues.GetOrAdd((cellText, dataType), tuple => new CellValue(tuple.Item1));
                if (cellValue.Parent != null)
                {
                    // HACK: Parent has internal set
                    _cache.SetParent(cellValue, null!);
                }

                // Create only for not null values for more compact xml. (omits t="s")
                dataTypeValue = _cache.EnumValues.GetOrAdd(dataType, values => new EnumValue<CellValues>(values));
            }

            // Example: <x:c t="str"><x:v>CellValue</x:v></x:c>
            Cell cell = new Cell(cellValue)
            {
                DataType = dataTypeValue,
            };

            return cell;
        }
    }

    public static class ExpressionUtils
    {
        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// </summary>
        public static Action<T, TProperty> GetPropertySetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod(nonPublic: true);

            var parameterT = Expression.Parameter(typeof(T), "x");
            var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

            var setExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Call(parameterT, setMethod, parameterTProperty),
                    parameterT,
                    parameterTProperty
                );

            return setExpression.Compile();
        }
    }
}

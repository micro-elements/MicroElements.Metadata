// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Represents text table.
    /// </summary>
    public class TextTable
    {
        /// <summary>
        /// Table name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Table data.
        /// </summary>
        public string[,] Data { get; }

        /// <summary>
        /// Gets columns.
        /// </summary>
        public string[] Columns => GetRow(-1);

        /// <summary>
        /// Gets columns count.
        /// </summary>
        public int ColumnCount => Data.GetLength(1);

        /// <summary>
        /// Gets row count.
        /// </summary>
        public int RowCount => Math.Max(Data.GetLength(0) - 1, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTable"/> class.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="data">Table data.</param>
        public TextTable(string name, string[,] data)
        {
            Name = name;
            Data = data;
        }

        /// <summary>
        /// Gets row by index.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <returns>Row data.</returns>
        public string[] GetRow(int rowIndex)
        {
            string[] row = new string[ColumnCount];
            for (int col = 0; col < ColumnCount; col++)
            {
                row[col] = Data[rowIndex + 1, col];
            }

            return row;
        }

        /// <inheritdoc />
        public override string ToString() => $"{nameof(Name)}: {Name}";
    }

    /// <summary>
    /// Extensions for tables.
    /// </summary>
    public static class TableExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>[]> AsKeyValueRows(this TextTable textTable)
        {
            var columns = textTable.Columns;

            for (int i = 0; i < textTable.RowCount; i++)
            {
                string[] values = textTable.GetRow(i);
                var pairs = columns.Zip(values, (col, val) => new KeyValuePair<string, string>(col, val));
                yield return pairs.ToArray();
            }
        }

        public static IEnumerable<IReadOnlyDictionary<string, string>> AsDictionaryRows(this TextTable textTable)
        {
            var columns = textTable.Columns;

            for (int i = 0; i < textTable.RowCount; i++)
            {
                string[] values = textTable.GetRow(i);
                var pairs = columns.Zip(values, (col, val) => new KeyValuePair<string, string>(col, val));
                yield return pairs.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }
    }

    /// <summary>
    /// Represents table in terms of properties and containers.
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// Gets the table info.
        /// </summary>
        IPropertyContainer Info { get; }

        /// <summary>
        /// Gets the table column list.
        /// </summary>
        IProperty[] Columns { get; }

        /// <summary>
        /// Gets the table rows.
        /// </summary>
        IPropertyContainer[] Rows { get; }
    }

    /// <summary>
    /// Represents table in terms of properties and containers.
    /// </summary>
    public class Table : ITable
    {
        /// <summary>
        /// Gets the table info.
        /// </summary>
        public IPropertyContainer Info { get; }

        /// <summary>
        /// Gets the table column list.
        /// </summary>
        public IProperty[] Columns { get; }

        /// <summary>
        /// Gets the table rows.
        /// </summary>
        public IPropertyContainer[] Rows { get; }

        public Table(IPropertyContainer info, IProperty[] columns, IPropertyContainer[] rows)
        {
            Info = info;
            Columns = columns;
            Rows = rows;
        }
    }

    public class TableBuilder
    {
        public PropertyList Info { get; } = new PropertyList();

        public List<IProperty> Columns { get; } = new List<IProperty>();

        public List<IPropertyContainer> Rows { get; } = new List<IPropertyContainer>();

        public TableBuilder WithInfo<T>(string propertyName, T value)
        {
            Info.SetValue(new Property<T>(propertyName), value);
            return this;
        }

        public TableBuilder WithColumns(params IProperty[] columns)
        {
            Columns.AddRange(columns);
            return this;
        }

        public TableBuilder WithRows(params IPropertyContainer[] rows)
        {
            Rows.AddRange(rows);
            return this;
        }

        public TableBuilder AddRow(IPropertyContainer row)
        {
            Rows.Add(row);
            return this;
        }

        public ITable Build()
        {
            return new Table(Info, Columns.ToArray(), Rows.ToArray());
        }
    }
}

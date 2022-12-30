// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using DocumentFormat.OpenXml.Packaging;
using MicroElements.CodeContracts;

namespace MicroElements.Metadata.OpenXml.Excel.Parsing
{
    /// <summary>
    /// Represents OpenXml element wrapper with reference to OpenXml document.
    /// </summary>
    /// <typeparam name="TOpenXmlElement">OpenXmlElement type.</typeparam>
    public interface IExcelElement<TOpenXmlElement> : IMetadataProvider
    {
        /// <summary>
        /// Gets OpenXml document that contains this element.
        /// </summary>
        SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Gets OpenXml element.
        /// </summary>
        [MaybeNull]
        TOpenXmlElement Data { get; }

        /// <summary>
        /// Returns true if <see cref="Data"/> is not null.
        /// </summary>
        /// <returns>true if <see cref="Data"/> is not null.</returns>
        bool IsEmpty();
    }

    /// <summary>
    /// Represents OpenXml element wrapper with reference to OpenXml document.
    /// </summary>
    /// <typeparam name="TOpenXmlElement">OpenXmlElement type.</typeparam>
    public class ExcelElement<TOpenXmlElement> : IExcelElement<TOpenXmlElement>
    {
        /// <summary>
        /// Gets OpenXml document that contains this element.
        /// </summary>
        public SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Gets OpenXml element.
        /// </summary>
        [MaybeNull]
        public TOpenXmlElement Data { get; }

        ///// <summary>
        ///// Gets OpenXml element as <see cref="Option{A}"/>.
        ///// </summary>
        ///// <returns>Optional OpenXml element.</returns>
        //public Option<TOpenXmlElement> AsOption() => Data!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelElement{TOpenXmlElement}"/> class.
        /// </summary>
        /// <param name="doc">OpenXml document that contains this element.</param>
        /// <param name="data">OpenXml element.</param>
        public ExcelElement(SpreadsheetDocument doc, [MaybeNull] TOpenXmlElement data)
        {
            Doc = doc.AssertArgumentNotNull(nameof(doc));
            Data = data;
        }

        /// <summary>
        /// Implicit conversion to <typeparamref name="TOpenXmlElement"/>.
        /// </summary>
        /// <param name="excelElement">ExcelElement.</param>
        public static implicit operator TOpenXmlElement(ExcelElement<TOpenXmlElement> excelElement) => excelElement.Data;

        /// <summary>
        /// Returns true if <see cref="Data"/> is not null.
        /// </summary>
        /// <returns>true if <see cref="Data"/> is not null.</returns>
        public bool IsEmpty() => Data == null;

        /// <inheritdoc />
        public override string ToString() => $"{Data}";
    }

    /// <summary>
    /// Represents OpenXml element wrapper with reference to OpenXml document.
    /// </summary>
    /// <typeparam name="TOpenXmlElement">OpenXmlElement type.</typeparam>
    public readonly struct ExcelElementLight<TOpenXmlElement> : IExcelElement<TOpenXmlElement>
    {
        /// <summary>
        /// Gets OpenXml document that contains this element.
        /// </summary>
        public SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Gets OpenXml element.
        /// </summary>
        [MaybeNull]
        public TOpenXmlElement Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelElementLight{TOpenXmlElement}"/> struct.
        /// </summary>
        /// <param name="doc">OpenXml document that contains this element.</param>
        /// <param name="data">OpenXml element.</param>
        public ExcelElementLight(SpreadsheetDocument doc, [MaybeNull] TOpenXmlElement data)
        {
            Doc = doc.AssertArgumentNotNull(nameof(doc));
            Data = data;
        }

        /// <summary>
        /// Implicit conversion to <typeparamref name="TOpenXmlElement"/>.
        /// </summary>
        /// <param name="excelElement">ExcelElement.</param>
        public static implicit operator TOpenXmlElement(ExcelElementLight<TOpenXmlElement> excelElement) => excelElement.Data;

        /// <summary>
        /// Returns true if <see cref="Data"/> is not null.
        /// </summary>
        /// <returns>true if <see cref="Data"/> is not null.</returns>
        public bool IsEmpty() => Data == null;

        /// <inheritdoc />
        public override string ToString() => $"{Data}";
    }
}

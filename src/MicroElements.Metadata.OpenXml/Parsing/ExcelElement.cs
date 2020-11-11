// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml.Packaging;
using MicroElements.Functional;
using MicroElements.Metadata;

namespace MicroElements.Parsing
{
    /// <summary>
    /// Represents OpenXml element wrapper with reference to OpenXml document.
    /// </summary>
    /// <typeparam name="TOpenXmlElement">OpenXmlElement type.</typeparam>
    public class ExcelElement<TOpenXmlElement> : IMetadataProvider
    {
        /// <summary>
        /// Gets OpenXml document that contains this element.
        /// </summary>
        public SpreadsheetDocument Doc { get; }

        /// <summary>
        /// Gets OpenXml element.
        /// </summary>
        public TOpenXmlElement Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelElement{TOpenXmlElement}"/> class.
        /// </summary>
        /// <param name="doc">OpenXml document that contains this element.</param>
        /// <param name="data">OpenXml element.</param>
        public ExcelElement(SpreadsheetDocument doc, TOpenXmlElement data)
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
}

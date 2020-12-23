// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using MicroElements.Functional;

namespace MicroElements.Metadata.OpenXml.Excel.Reporting
{
    /// <summary>
    /// Represents OpenXml document context.
    /// </summary>
    public class DocumentContext
    {
        /// <summary>
        /// Gets <see cref="SpreadsheetDocument"/>.
        /// </summary>
        public SpreadsheetDocument Document { get; }

        /// <summary>
        /// Gets <see cref="WorkbookPart"/> for <see cref="Document"/>.
        /// </summary>
        public WorkbookPart WorkbookPart => Document.WorkbookPart;

        /// <summary>
        /// Gets document metadata.
        /// </summary>
        public IExcelMetadata DocumentMetadata { get; }

        /// <summary>
        /// Gets SharedStringTable for document.
        /// </summary>
        public IDictionary<string, string> SharedStringTable { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentContext"/> class.
        /// </summary>
        /// <param name="document">Excel document.</param>
        /// <param name="documentMetadata">Document configuration metadata.</param>
        public DocumentContext(SpreadsheetDocument document, IExcelMetadata documentMetadata)
        {
            document.AssertArgumentNotNull(nameof(document));
            documentMetadata.AssertArgumentNotNull(nameof(documentMetadata));

            Document = document;
            DocumentMetadata = documentMetadata;
        }
    }
}

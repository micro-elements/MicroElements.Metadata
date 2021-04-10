// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using MicroElements.CodeContracts;
using MicroElements.Metadata.OpenXml.Excel.Styling;

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
        /// Caches for document building process.
        /// </summary>
        public DocumentBuilderCache Cache { get; } = new DocumentBuilderCache();

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

    public class DocumentBuilderCache
    {
        /// <summary>
        /// Gets SharedStringTable for document.
        /// </summary>
        public ConcurrentDictionary<string, string> SharedStringTable { get; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<uint, UInt32Value> UInt32Value { get; } = new ConcurrentDictionary<uint, UInt32Value>();

        public ConcurrentDictionary<string, StringValue> StringValue { get; } = new ConcurrentDictionary<string, StringValue>();

        public ConcurrentDictionary<(int, string, MergeMode), UInt32Value> StyleMerge { get; } = new ConcurrentDictionary<(int, string, MergeMode), UInt32Value>();
    }
}

// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Provides property renderers.
    /// </summary>
    public interface IReportRenderer : IMetadataProvider
    {
        /// <summary>
        /// Gets report name.
        /// </summary>
        string ReportName { get; }

        /// <summary>
        /// Gets property renderers.
        /// </summary>
        IReadOnlyList<IPropertyRenderer> Renderers { get; }
    }
}

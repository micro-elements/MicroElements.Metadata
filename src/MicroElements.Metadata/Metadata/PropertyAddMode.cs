// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata
{
    /// <summary>
    /// Property add mode.
    /// </summary>
    public enum PropertyAddMode
    {
        /// <summary>
        /// Property will be appended to property list.
        /// </summary>
        Add,

        /// <summary>
        /// Property value should be replaced if value for the same property exists.
        /// </summary>
        Set,

        /// <summary>
        /// Property value should not be replaced if value for the same property exists.
        /// </summary>
        SetNotExisting,
    }
}

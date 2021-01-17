// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Linq;

namespace MicroElements.Metadata.Xml
{
    public static partial class XmlParser
    {
        /// <summary>
        /// Gets full element name from root parent to current element.
        /// Example: "A.B.C".
        /// </summary>
        /// <param name="element">Source element.</param>
        /// <param name="delimeter">Optional delimeter.</param>
        /// <returns>Full element name.</returns>
        public static string GetFullName(this XElement element, string delimeter = ".")
        {
            string name = element.Name.LocalName;

            XElement? parent = element.Parent;
            while (parent != null)
            {
                name = parent.Name.LocalName + delimeter + name;
                parent = parent.Parent;
            }

            return name;
        }

        /// <summary>
        /// Gets elements name as LocalName.
        /// </summary>
        /// <param name="element">Source element.</param>
        /// <returns>Local element name.</returns>
        public static string GetElementNameDefault(this XElement element)
        {
            return element.Name.LocalName;
        }
    }
}

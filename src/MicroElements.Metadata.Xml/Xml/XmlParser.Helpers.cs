// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Buffers;
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
        public static string GetFullNameSlow(this XElement element, string delimeter = ".")
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
        /// Gets full element name from root parent to current element.
        /// Example: "A.B.C".
        /// </summary>
        /// <param name="element">Source element.</param>
        /// <param name="delimeter">Optional delimeter.</param>
        /// <param name="maxBufferSize">Max buffer size for name formatting. Default: 128.</param>
        /// <returns>Full element name.</returns>
        public static string GetFullName(this XElement element, string delimeter = ".", int maxBufferSize = 128)
        {
            string localName = element.Name.LocalName;
            if (element.Parent == null)
                return localName;

            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            if (maxBufferSize < 64)
                maxBufferSize = 64;
            char[] buffer = arrayPool.Rent(maxBufferSize);
            int position = buffer.Length;

            try
            {
                position = buffer.SetBeforeEnd(localName, position);

                XElement? parent = element.Parent;
                while (parent != null)
                {
                    position = buffer.SetBeforeEnd(delimeter, position);
                    position = buffer.SetBeforeEnd(parent.Name.LocalName, position);

                    parent = parent.Parent;
                }

                if (position < 0)
                    return element.GetFullName(delimeter, maxBufferSize * 2);

                return new string(buffer, position, buffer.Length - position);
            }
            finally
            {
                arrayPool.Return(buffer);
            }
        }

        private static int SetBeforeEnd(this char[] buffer, string value, int position)
        {
            position -= value.Length;
            if (position < 0)
                return position;
            value.CopyTo(0, buffer, position, value.Length);
            return position;
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

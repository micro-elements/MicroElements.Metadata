// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MicroElements.Metadata.Formatters
{
    public static partial class Formatter
    {
        public static IValueFormatter ValueFormatter = new CompositeFormatter(new DefaultFormatProvider());

        public static IValueFormatter DefaultToStringFormatter = new CompositeFormatter(new CollectionFormatter(ValueFormatter), ValueFormatter);
    }

    public static partial class Formatter
    {

    }
}

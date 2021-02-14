// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MicroElements.Metadata.Parsing
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Ok.")]
    public static class Parser
    {
        public static ParseResult<int> ParseInt(string? text) => text.Parse<int>(int.TryParse);

        public static ParseResult<byte> ParseByte(string? text) => text.Parse<byte>(byte.TryParse);

        public static ParseResult<short> ParseShort(string? text) => text.Parse<short>(short.TryParse);

        public static ParseResult<long> ParseLong(string? text) => text.Parse<long>(long.TryParse);

        public static ParseResult<sbyte> ParseSByte(string? text) => text.Parse<sbyte>(sbyte.TryParse);

        public static ParseResult<ushort> ParseUShort(string? text) => text.Parse<ushort>(ushort.TryParse);

        public static ParseResult<uint> ParseUInt(string? text) => text.Parse<uint>(uint.TryParse);

        public static ParseResult<ulong> ParseULong(string? text) => text.Parse<ulong>(ulong.TryParse);

        public static ParseResult<float> ParseFloat(string? text) => text.ParseNumeric<float>(float.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<double> ParseDouble(string? text) => text.ParseNumeric<double>(double.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<decimal> ParseDecimal(string? text) => text.ParseNumeric<decimal>(decimal.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<int?> ParseNullableInt(string? text) => text.ParseNullable<int>(int.TryParse);

        public static ParseResult<byte?> ParseNullableByte(string? text) => text.ParseNullable<byte>(byte.TryParse);

        public static ParseResult<short?> ParseNullableShort(string? text) => text.ParseNullable<short>(short.TryParse);

        public static ParseResult<long?> ParseNullableLong(string? text) => text.ParseNullable<long>(long.TryParse);

        public static ParseResult<sbyte?> ParseNullableSByte(string? text) => text.ParseNullable<sbyte>(sbyte.TryParse);

        public static ParseResult<ushort?> ParseNullableUShort(string? text) => text.ParseNullable<ushort>(ushort.TryParse);

        public static ParseResult<uint?> ParseNullableUInt(string? text) => text.ParseNullable<uint>(uint.TryParse);

        public static ParseResult<ulong?> ParseNullableULong(string? text) => text.ParseNullable<ulong>(ulong.TryParse);

        public static ParseResult<float?> ParseNullableFloat(string? text) => text.ParseNullableNumeric<float>(float.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<double?> ParseNullableDouble(string? text) => text.ParseNullableNumeric<double>(double.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<decimal?> ParseNullableDecimal(string? text) => text.ParseNullableNumeric<decimal>(decimal.TryParse, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static ParseResult<bool> ParseBool(string? text) => text.Parse<bool>(bool.TryParse);

        public static ParseResult<bool?> ParseNullableBool(string? text) => text.ParseNullable<bool>(bool.TryParse);

        public static ParseResult<DateTime> ParseDateTime(string? text) => text.Parse<DateTime>(DateTime.TryParse);

        public static ParseResult<DateTime?> ParseNullableDateTime(string? text) => text.ParseNullable<DateTime>(DateTime.TryParse);

        public static readonly ValueParser<int> IntParser = new ValueParser<int>(ParseInt);

        public static readonly ValueParser<byte> ByteParser = new ValueParser<byte>(ParseByte);

        public static readonly ValueParser<short> ShortParser = new ValueParser<short>(ParseShort);

        public static readonly ValueParser<long> LongParser = new ValueParser<long>(ParseLong);

        public static readonly ValueParser<sbyte> SByteParser = new ValueParser<sbyte>(ParseSByte);

        public static readonly ValueParser<ushort> UShortParser = new ValueParser<ushort>(ParseUShort);

        public static readonly ValueParser<uint> UIntParser = new ValueParser<uint>(ParseUInt);

        public static readonly ValueParser<ulong> ULongParser = new ValueParser<ulong>(ParseULong);

        public static readonly ValueParser<float> FloatParser = new ValueParser<float>(ParseFloat);

        public static readonly ValueParser<double> DoubleParser = new ValueParser<double>(ParseDouble);

        public static readonly ValueParser<decimal> DecimalParser = new ValueParser<decimal>(ParseDecimal);

        public static readonly ValueParser<int?> NullableIntParser = new ValueParser<int?>(ParseNullableInt);

        public static readonly ValueParser<byte?> NullableByteParser = new ValueParser<byte?>(ParseNullableByte);

        public static readonly ValueParser<short?> NullableShortParser = new ValueParser<short?>(ParseNullableShort);

        public static readonly ValueParser<long?> NullableLongParser = new ValueParser<long?>(ParseNullableLong);

        public static readonly ValueParser<sbyte?> NullableSByteParser = new ValueParser<sbyte?>(ParseNullableSByte);

        public static readonly ValueParser<ushort?> NullableUShortParser = new ValueParser<ushort?>(ParseNullableUShort);

        public static readonly ValueParser<uint?> NullableUIntParser = new ValueParser<uint?>(ParseNullableUInt);

        public static readonly ValueParser<ulong?> NullableULongParser = new ValueParser<ulong?>(ParseNullableULong);

        public static readonly ValueParser<float?> NullableFloatParser = new ValueParser<float?>(ParseNullableFloat);

        public static readonly ValueParser<double?> NullableDoubleParser = new ValueParser<double?>(ParseNullableDouble);

        public static readonly ValueParser<decimal?> NullableDecimalParser = new ValueParser<decimal?>(ParseNullableDecimal);

        public static readonly ValueParser<bool> BoolParser = new ValueParser<bool>(ParseBool);

        public static readonly ValueParser<bool?> NullableBoolParser = new ValueParser<bool?>(ParseNullableBool);

        public static readonly ValueParser<DateTime> DateTimeParser = new ValueParser<DateTime>(ParseDateTime);

        public static readonly ValueParser<DateTime?> NullableDateTimeParser = new ValueParser<DateTime?>(ParseNullableDateTime);
    }
}

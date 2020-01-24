// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Text that can be translated to many languages.
    /// </summary>
    public class LocalizableString
    {
        private readonly LocalString[] _texts;

        /// <summary>
        /// Gets text (first text assumes as main).
        /// </summary>
        public LocalString Text => _texts[0];

        /// <summary>
        /// Gets text and text translations.
        /// </summary>
        public IReadOnlyList<LocalString> Texts => _texts;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableString"/> class.
        /// </summary>
        /// <param name="texts">Text and text translations.</param>
        public LocalizableString(params LocalString[] texts)
        {
            if (texts == null || texts.Length == 0)
                throw new ArgumentException("Should contain at least one text.", nameof(texts));
            _texts = texts;
        }

        /// <summary>
        /// Implicitly converts from string.
        /// </summary>
        /// <param name="text">text.</param>
        public static implicit operator LocalizableString(string text) => new LocalizableString(text);

        /// <summary>
        /// Adds another Text translation.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns><see cref="LocalizableString"/> to support chaining.</returns>
        public LocalizableString Add(LocalString text) =>
            new LocalizableString(_texts.Append(text).ToArray());

        /// <inheritdoc />
        public override string ToString() => Texts.FormatList();
    }

    /// <summary>
    /// Language.
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// English.
        /// </summary>
        English,

        /// <summary>
        /// Русский язык.
        /// </summary>
        Russian,
    }

    /// <summary>
    /// Localized text with language.
    /// </summary>
    public readonly struct LocalString
    {
        /// <summary>
        /// Text.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Text language.
        /// </summary>
        public readonly Language Language;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalString"/> struct.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="language">Text language.</param>
        public LocalString(string text, Language language = Language.English)
        {
            Text = text.AssertArgumentNotNull(nameof(text));
            Language = language;
        }

        /// <summary>
        /// Implicitly converts from string.
        /// </summary>
        /// <param name="value">text.</param>
        public static implicit operator LocalString(string value) => new LocalString(value);

        /// <inheritdoc />
        public override string ToString() => $"{Language}: {Text}";
    }

    /// <summary>
    /// Extensions for <see cref="LocalizableString"/> and <see cref="LocalString"/>.
    /// </summary>
    public static class LocalizableStringExtensions
    {
        public static LocalString Lang(this string text, Language language) => new LocalString(text, language);
    }

    public static class StringFormatter
    {
        public static string FormatValue(this object value)
        {
            if (value == null)
                return "null";

            //if (value is LocalDate localDate)
            //    return localDate.ToString("uuuu-MM-dd", null);

            return value.ToString();
        }

        public static string FormatList<T>(this IEnumerable<T> list)
        {
            return string.Join(", ", list.Select(value => FormatValue(value)));
        }
    }

    //public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    //{
    //    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
    //        DateTimeOffset.ParseExact(reader.GetString(), "MM/dd/yyyy", CultureInfo.InvariantCulture);

    //    public override void Write(Utf8JsonWriter writer, DateTimeOffset dateTimeValue, JsonSerializerOptions options) =>
    //        writer.WriteStringValue(dateTimeValue.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture));
    //}
}

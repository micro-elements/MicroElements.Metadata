// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MicroElements.Functional;

namespace MicroElements.Metadata
{
    /// <summary>
    /// Text that can be translated to many languages.
    /// </summary>
    [ImmutableObject(true)]
    public class LocalizableString : IEnumerable<LocalString>
    {
        private readonly List<LocalString> _texts = new List<LocalString>();

        /// <summary>
        /// Gets text (first text assumes as main).
        /// </summary>
        public LocalString Text => _texts.Count > 0 ? _texts[0] : LocalString.Empty;

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
            if (texts != null && texts.Length > 0)
                _texts.AddRange(texts);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableString"/> class.
        /// </summary>
        /// <param name="texts">Text and text translations.</param>
        public LocalizableString(IEnumerable<LocalString> texts)
        {
            if (texts != null)
                _texts.AddRange(texts);
        }

        /// <summary>
        /// Implicitly converts from string.
        /// </summary>
        /// <param name="text">text.</param>
        public static implicit operator LocalizableString(string text) => new LocalizableString(new LocalString(text));

        /// <summary>
        /// Adds another Text translation.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>New <see cref="LocalizableString"/> to support chaining.</returns>
        public LocalizableString Add(LocalString text)
        {
            if (_texts.Count == 0)
            {
                return new LocalizableString(text);
            }

            var texts = _texts.Append(text);
            return new LocalizableString(texts);
        }

        /// <summary>
        /// Adds another Text translation.
        /// Replaces if text with the same language already exists.
        /// </summary>
        /// <param name="text">Text with language to add.</param>
        /// <returns>New <see cref="LocalizableString"/> to support chaining.</returns>
        public LocalizableString AddOrReplace(LocalString text)
        {
            if (_texts.Count == 0)
            {
                return new LocalizableString(text);
            }

            var texts = _texts.Where(localString => localString.Language != text.Language).Append(text);
            return new LocalizableString(texts);
        }

        /// <summary>
        /// Gets text for specified language.
        /// </summary>
        /// <param name="language">Language to get.</param>
        /// <returns><see cref="LocalString"/> or <see cref="LocalString.Empty"/>.</returns>
        public LocalString Get(Language language) =>
            _texts.FirstOrNone(s => s.Language == language).GetValueOrDefault(LocalString.Empty);

        /// <inheritdoc />
        public override string ToString() => Texts.FormatAsTuple(formatValue: StringFormatter.FormatValue);

        /// <inheritdoc />
        public IEnumerator<LocalString> GetEnumerator() => _texts.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Language.
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// Language is undefined.
        /// </summary>
        Undefined,

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
    public readonly struct LocalString : IEquatable<LocalString>
    {
        /// <summary>
        /// Represents the empty LocalString.
        /// </summary>
        public static readonly LocalString Empty = new LocalString(string.Empty, Language.Undefined);

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

        /// <summary>
        /// Implicitly converts to string.
        /// </summary>
        /// <param name="localString">Source <see cref="LocalString"/>.</param>
        public static implicit operator string(LocalString localString) => localString.Text;

        /// <inheritdoc />
        public override string ToString() => $"{Language}: {Text}";

        /// <inheritdoc />
        public bool Equals(LocalString other)
        {
            return Text == other.Text && Language == other.Language;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is LocalString other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Text, (int) Language);
        }

        public static bool operator ==(LocalString left, LocalString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocalString left, LocalString right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Extensions for <see cref="LocalizableString"/> and <see cref="LocalString"/>.
    /// </summary>
    public static class LocalizableStringExtensions
    {
        /// <summary>
        /// Creates <see cref="LocalString"/> with specified language.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="language">Language.</param>
        /// <returns><see cref="LocalString"/> with specified language.</returns>
        public static LocalString Lang(this string text, Language language) => new LocalString(text, language);

        /// <summary>
        /// Creates <see cref="LocalString"/> with specified language.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="language">Language.</param>
        /// <returns><see cref="LocalString"/> with specified language.</returns>
        public static LocalString WithLang(this string text, Language language) => new LocalString(text, language);
    }
}

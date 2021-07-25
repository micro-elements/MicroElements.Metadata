// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MicroElements.Metadata.Schema
{
    public class SchemaEqualityComparer
    {
        public static SchemaEqualityComparer Instance = new SchemaEqualityComparer();

        public string GetSchemaDigest(IEnumerable<IProperty> properties)
        {
            //MetadataSchema.GenerateCompactSchema(properties)
            string digest = properties
                .OrderBy(property => property.Name)
                .Aggregate(
                    new StringBuilder(),
                    (builder, property) => builder.AppendFormat("{0}@{1};", property.Name, property.Type))
                .ToString();
            return digest;
        }
    }

    public static class DigestExtensions
    {
        public static string GetSchemaDigest(this IEnumerable<IProperty> properties)
        {
            return SchemaEqualityComparer.Instance.GetSchemaDigest(properties);
        }

        public static string GetSchemaDigest(this IObjectSchema objectSchema)
        {
            return objectSchema.Properties.GetSchemaDigest();
        }

        public static string GetSchemaDigestHash(this IObjectSchema objectSchema)
        {
            return objectSchema.GetSchemaDigest().Md5Hash();
        }
    }

    public static class HashGenerator
    {
        public static readonly string Symbols = "abcdefghijklmnopqrstuvwxyz";
        public static readonly string SymbolsUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static readonly string Digits = "0123456789";
        public static readonly string SymbolsAndDigits = Symbols + Digits;

        public static string GenerateRandomCode(int length = 8)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return Enumerable
                .Range(0, length)
                .Select(i => random.Next(0, Symbols.Length - 1))
                .Aggregate(new StringBuilder(capacity: length), (stringBuilder, digit) => stringBuilder.Append(Symbols[digit].ToString()))
                .ToString();
        }

        public static string GenerateMd5HashInBase58(
            this string content,
            int? length = null,
            string? alphabet = null)
        {
            byte[] hashBytes = content.Md5HashBytes();

            EncodingResult encodingResult = Base58_BigIntImpl.Instance.Encode(hashBytes, outputMaxLength: length, alphabet: alphabet);

            return encodingResult.Text;
        }

        public static string EncodeBase58(
            this byte[] data,
            int? length = null,
            string? alphabet = null)
        {
            EncodingResult encodingResult = Base58_BigIntImpl.Instance.Encode(inputBytes: data, outputMaxLength: length, alphabet: alphabet);
            return encodingResult.Text;
        }

        public static string EncodeBase58_2(this byte[] data)
        {
            int length = data.Length;
            string alphabet = Base58.Alphabet;
            int encodingBase = alphabet.Length;

            // Count zeroes.
            int zeros = 0;
            for (int i = 0; i < length && data[i] == 0; i++)
                zeros++;

            // Allocate result buffer.
            int resultLength = ((length - zeros) * 138 / 100) + 1;
            char[] result = new char[resultLength + zeros];
            int dataLength = 0;

            // Debug counter.
            int counter = 0;

            // Encode data.
            for (int dataIndex = zeros; dataIndex < length; dataIndex++)
            {
                int carry = data[dataIndex];
                int i = 0;
                for (int revIt = result.Length - 1; (carry != 0 || i < dataLength) && (revIt >= 0); revIt--, i++)
                {
                    carry += result[revIt] << 8;

                    result[revIt] = (char)(carry % encodingBase);
                    carry /= encodingBase;

                    counter++;
                }

                dataLength = i;
            }

            for (int i = 0; i < zeros; i++)
                result[i] = alphabet[0];

            for (int i = zeros; i < result.Length; i++)
                result[i] = alphabet[result[i]];

            int start = result.Length - dataLength - zeros;
            if (start != 0)
                result = result[start..];

            var encoded = new string(result);
            return encoded;
        }

        public static byte[] Md5HashBytes(this string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using var cryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] hash = cryptoServiceProvider.ComputeHash(bytes);
            return hash;
        }

        public static string Md5Hash(this string content)
        {
            return content.Md5HashBytes().AsText();
        }

        public static string AsText(this byte[] hash, int predefinedLength = 32)
        {
            var stringBuilder = new StringBuilder(predefinedLength);

            foreach (var item in hash)
            {
                stringBuilder.Append(item.ToString("X2"));
            }

            return stringBuilder.ToString();
        }
    }

    public readonly struct EncodingResult
    {
        public readonly char[] Chars;
        public readonly int CharIndex;
        public readonly int CharCount;

        public string Text => new string(Chars, CharIndex, CharCount);

        public EncodingResult(char[] chars, int charIndex, int charCount)
        {
            Chars = chars;
            CharIndex = charIndex;
            CharCount = charCount;
        }
    }

    public interface IEncodingAlgorithm
    {
        EncodingResult Encode(
            byte[] inputBytes,
            int? inputByteIndex,
            int? inputByteCount,
            int? outputMaxLength,
            char[]? outputChars,
            int? outputCharIndex,
            string? alphabet);
    }

    public class Base58
    {
        /*
        Why base-58 instead of standard base-64 encoding?
        - Don't want 0OIl characters that look the same in some fonts and
          could be used to create visually identical looking data.
        - A string with non-alphanumeric characters is not as easily accepted as input.
        - E-mail usually won't line-break if there's no punctuation to break at.
        - Double-clicking selects the whole string as one word if it's all alphanumeric.
         */
        public static readonly string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static readonly Base58Encoding Encoding = new Base58Encoding();
    }

    public class Base58_BigIntImpl : IEncodingAlgorithm
    {
        public static readonly Base58_BigIntImpl Instance = new Base58_BigIntImpl();

        /// <inheritdoc />
        public EncodingResult Encode(
            byte[] inputBytes,
            int? inputByteIndex = null,
            int? inputByteCount = null,
            int? outputMaxLength = null,
            char[]? outputChars = null,
            int? outputCharIndex = null,
            string? alphabet = null)
        {
            inputByteIndex ??= 0;
            inputByteCount ??= inputBytes.Length;

            if (inputByteIndex > 0 && inputByteCount < inputBytes.Length)
                inputBytes = inputBytes[inputByteIndex.Value..inputBytes.Length];

            int length = inputBytes.Length;
            alphabet ??= Base58.Alphabet;
            int encodingBase = alphabet.Length;

            // Represent data as BigInteger.
            var bigInt = new BigInteger(inputBytes, isUnsigned: true, isBigEndian: true);

            // Allocate result buffer.
            if (outputChars == null)
            {
                if (outputCharIndex.HasValue)
                    throw new InvalidOperationException("outputCharIndex can be used only with provided outputChars value.");

                int resultMaxLength = (length * 138 / 100) + 1;
                int resultLength = resultMaxLength;
                if (outputMaxLength.HasValue)
                {
                    // limit by outputMaxLength
                    resultLength = Math.Min(resultMaxLength, outputMaxLength.Value);
                }

                outputChars = new char[resultLength];
            }

            if (outputCharIndex is { } charIndex and > 0)
                outputChars = outputChars[charIndex..];

            int last = outputChars!.Length;
            int outputCharsLength;

            // Encode data.
            while (bigInt > 0)
            {
                last--;
                if (last < 0)
                {
                    // Output buffer can be smaller then required.
                    break;
                }

                bigInt = BigInteger.DivRem(bigInt, encodingBase, out var remainder);
                outputChars[last] = alphabet[(int)remainder];

                if (outputMaxLength.HasValue)
                {
                    outputCharsLength = outputChars.Length - last;
                    if (outputCharsLength >= outputMaxLength)
                        break;
                }
            }

            // Append `1` for each leading 0 byte
            for (int i = 0; i < length && inputBytes[i] == 0; i++)
            {
                if (outputMaxLength.HasValue)
                {
                    outputCharsLength = outputChars.Length - last;
                    if (outputCharsLength >= outputMaxLength)
                        break;
                }

                last--;
                outputChars[last] = alphabet[0];
            }

            outputCharsLength = outputChars.Length - last;
            return new EncodingResult(outputChars, last, outputCharsLength);
        }
    }

    public class Base58Encoding : Encoding
    {
        public static readonly Base58Encoding Instance = new Base58Encoding();

        /// <inheritdoc />
        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override char[] GetChars(byte[] bytes, int index, int count)
        {
            int maxCharCount = GetMaxCharCount(count);
            char[] result = new char[maxCharCount];
            int realCharCount = GetChars(bytes, index, count, result, 0);
            if (realCharCount < maxCharCount)
            {
                int start = maxCharCount - realCharCount;
                result = result[start..];
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            int byteCount = count - index;
            int resultLength = (byteCount * 138 / 100) + 1;
            return resultLength;
        }

        /// <inheritdoc />
        public override int GetChars(byte[] inputBytes, int byteIndex, int byteCount, char[] outputChars, int charIndex)
        {
            EncodingResult encodingResult = Base58_BigIntImpl.Instance.Encode(inputBytes, byteIndex, byteCount, null, outputChars, charIndex, alphabet: Base58.Alphabet);
            return encodingResult.CharCount;
        }

        /// <inheritdoc />
        public override int GetMaxByteCount(int charCount)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override int GetMaxCharCount(int byteCount)
        {
            int resultLength = (byteCount * 138 / 100) + 1;
            return resultLength;
        }
    }
}

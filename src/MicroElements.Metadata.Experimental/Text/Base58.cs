// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MicroElements.Text
{
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

        public static Base58Encoding Encoding { get; } = new Base58Encoding();
    }

    public interface IEncodingAlgorithm
    {
        EncodingResult Encode(in EncodingArgs args);
    }

    public readonly struct EncodingArgs
    {
        public readonly byte[] InputBytes;
        public readonly int? InputByteIndex;
        public readonly int? InputByteCount;

        public readonly char[]? OutputChars;
        public readonly int? OutputCharIndex;
        public readonly int? OutputMaxLength;

        public readonly string? Alphabet;

        public EncodingArgs(
            byte[] inputBytes,
            int? inputByteIndex = null,
            int? inputByteCount = null,
            int? outputMaxLength = null,
            char[]? outputChars = null,
            int? outputCharIndex = null,
            string? alphabet = null)
        {
            InputBytes = inputBytes;
            InputByteIndex = inputByteIndex;
            InputByteCount = inputByteCount;
            OutputMaxLength = outputMaxLength;
            OutputChars = outputChars;
            OutputCharIndex = outputCharIndex;
            Alphabet = alphabet;
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

    public class Base58EncodingAlgorithm : IEncodingAlgorithm
    {
        public static readonly Base58EncodingAlgorithm Instance = new Base58EncodingAlgorithm();

        /// <inheritdoc />
        public EncodingResult Encode(in EncodingArgs args)
        {
            if (args.InputBytes is null)
                throw new ArgumentNullException(nameof(args.InputBytes));

            byte[] inputBytes = args.InputBytes;
            int inputBytesLength = inputBytes.Length;

            var inputByteIndex = args.InputByteIndex.GetValueOrDefault(0);
            var inputByteCount = args.InputByteCount.GetValueOrDefault(inputBytesLength - inputByteIndex);
            int inputEndIndexInclusive = inputByteIndex + inputByteCount - 1;

            if (inputByteIndex < 0)
                throw new IndexOutOfRangeException($"{nameof(args.InputByteIndex)}: {args.InputByteIndex} is out of range.");
            if (inputEndIndexInclusive > inputBytesLength - 1)
                throw new IndexOutOfRangeException($"{nameof(args.InputByteCount)}: {args.InputByteCount} is out of range.");

            // Input sub range.
            if (inputByteIndex > 0 || inputEndIndexInclusive < inputBytes.Length - 1)
            {
                // c# range is exclusive for end index
                var inputEndIndex = inputEndIndexInclusive + 1;
                inputBytes = inputBytes[inputByteIndex..inputEndIndex];
                inputBytesLength = inputBytes.Length;
            }

            // Encoding params.
            var alphabet = args.Alphabet ?? Base58.Alphabet;
            int encodingBase = alphabet.Length;

            // Represent data as BigInteger.
            var bigInt = new BigInteger(inputBytes, isUnsigned: true, isBigEndian: true);

            // Result buffer.
            char[] outputChars;
            if (args.OutputChars != null)
            {
                // Use provided buffer.
                outputChars = args.OutputChars;
            }
            else
            {
                if (args.OutputCharIndex.HasValue)
                    throw new InvalidOperationException("outputCharIndex can be used only with provided outputChars value.");

                int resultMaxLength = (inputBytesLength * 138 / 100) + 1;
                int resultLength = resultMaxLength;
                if (args.OutputMaxLength.HasValue)
                {
                    // limit by outputMaxLength
                    resultLength = Math.Min(resultMaxLength, args.OutputMaxLength.Value);
                }

                // Allocate result buffer.
                outputChars = new char[resultLength];
            }

            // Output sub range.
            if (args.OutputCharIndex is { } outputCharIndex and > 0)
                outputChars = outputChars[outputCharIndex..];

            int outputIndex = outputChars.Length;
            int outputCharsLength;

            // Encode data.
            while (bigInt > 0)
            {
                outputIndex--;
                if (outputIndex < 0)
                {
                    // Output buffer can be smaller then required.
                    break;
                }

                bigInt = BigInteger.DivRem(bigInt, encodingBase, out var remainder);
                outputChars[outputIndex] = alphabet[(int)remainder];

                if (args.OutputMaxLength.HasValue)
                {
                    outputCharsLength = outputChars.Length - outputIndex;
                    if (outputCharsLength >= args.OutputMaxLength)
                        break;
                }
            }

            // Append `1` for each leading 0 byte
            for (int i = 0; i < inputBytesLength && inputBytes[i] == 0; i++)
            {
                if (args.OutputMaxLength.HasValue)
                {
                    outputCharsLength = outputChars.Length - outputIndex;
                    if (outputCharsLength >= args.OutputMaxLength)
                        break;
                }

                outputIndex--;
                if (outputIndex < 0)
                {
                    // Output buffer can be smaller then required.
                    break;
                }

                outputChars[outputIndex] = alphabet[0];
            }

            outputCharsLength = outputChars.Length - outputIndex;
            return new EncodingResult(outputChars, outputIndex, outputCharsLength);
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
            EncodingResult encodingResult = Base58EncodingAlgorithm.Instance.Encode(new EncodingArgs(inputBytes, byteIndex, byteCount, null, outputChars, charIndex, alphabet: Base58.Alphabet));
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

    /// <summary>
    /// DotNet Random is not ThreadSafe so we need ThreadSafeRandom.
    /// See also: https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe.
    /// Design notes:
    /// 1. Uses own Random for each thread (thread local).
    /// 2. Seed can be set in ThreadSafeRandom ctor. Note: Be careful - one seed for all threads can lead same values for several threads.
    /// 3. ThreadSafeRandom implements Random class for simple usage instead ordinary Random.
    /// 4. ThreadSafeRandom can be used by global static instance. Example: `int randomInt = ThreadSafeRandom.Global.Next()`.
    /// </summary>
    internal class ThreadSafeRandom : Random
    {
        /// <summary>
        /// Gets global static instance.
        /// </summary>
        public static ThreadSafeRandom Global { get; } = new ThreadSafeRandom();

        // Thread local Random is safe to use on that thread.
        private readonly ThreadLocal<Random> _threadLocalRandom;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeRandom"/> class.
        /// </summary>
        /// <param name="seed">Optional seed for <see cref="Random"/>. If not provided then random seed will be used.</param>
        public ThreadSafeRandom(int? seed = null)
        {
            _threadLocalRandom = new ThreadLocal<Random>(() => seed != null ? new Random(seed.Value) : new Random());
        }

        /// <inheritdoc />
        public override int Next() => _threadLocalRandom.Value.Next();

        /// <inheritdoc />
        public override int Next(int maxValue) => _threadLocalRandom.Value.Next(maxValue);

        /// <inheritdoc />
        public override int Next(int minValue, int maxValue) => _threadLocalRandom.Value.Next(minValue, maxValue);

        /// <inheritdoc />
        public override void NextBytes(byte[] buffer) => _threadLocalRandom.Value.NextBytes(buffer);

        /// <inheritdoc />
        public override void NextBytes(Span<byte> buffer) => _threadLocalRandom.Value.NextBytes(buffer);

        /// <inheritdoc />
        public override double NextDouble() => _threadLocalRandom.Value.NextDouble();
    }

    public static class HashGenerator
    {
        public static readonly string Symbols = "abcdefghijklmnopqrstuvwxyz";
        public static readonly string SymbolsUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static readonly string Digits = "0123456789";
        public static readonly string SymbolsAndDigits = Symbols + Digits;

        public static string GenerateRandomCode(string? alphabet = null, int length = 8)
        {
            alphabet ??= Symbols;
            return Enumerable
                .Range(0, length)
                .Select(i => ThreadSafeRandom.Global.Next(0, Symbols.Length - 1))
                .Aggregate(new StringBuilder(capacity: length), (stringBuilder, digit) => stringBuilder.Append(alphabet[digit].ToString()))
                .ToString();
        }

        public static string GenerateMd5HashInBase58(
            this string content,
            int? length = null,
            string? alphabet = null)
        {
            byte[] hashBytes = content.Md5HashBytes();

            EncodingResult encodingResult = Base58EncodingAlgorithm.Instance.Encode(new EncodingArgs(hashBytes, outputMaxLength: length, alphabet: alphabet));

            return encodingResult.Text;
        }

        public static string EncodeToBase58(this string content)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(content);
            EncodingResult encodingResult = Base58EncodingAlgorithm.Instance.Encode(new EncodingArgs(inputBytes, alphabet: Base58.Alphabet));
            return encodingResult.Text;
        }

        public static string EncodeToBase58(
            this byte[] data,
            int? length = null,
            string? alphabet = null)
        {
            EncodingResult encodingResult = Base58EncodingAlgorithm.Instance.Encode(new EncodingArgs(inputBytes: data, outputMaxLength: length, alphabet: alphabet));
            return encodingResult.Text;
        }

        public static byte[] Md5HashBytes(this string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            using var cryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] hash = cryptoServiceProvider.ComputeHash(bytes);
            return hash;
        }

        public static string Md5HashAsHexText(this string content)
        {
            return content.Md5HashBytes().AsHexText();
        }

        public static string AsHexText(this byte[] hash, int predefinedLength = 32)
        {
            var stringBuilder = new StringBuilder(predefinedLength);

            foreach (var item in hash)
            {
                stringBuilder.Append(item.ToString("X2"));
            }

            return stringBuilder.ToString();
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
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Multiformats.Base.Tests
{
    public class Base58DifferentialTests
    {
        private const int MaxLength = 100;
        private const int RoundsPerLength = 40;
        private const string InvalidChars = "0OIl+/=_- \u0080 ÿ";

        public static IEnumerable<object[]> Encodings()
        {
            yield return new object[] { MultibaseEncoding.Base58Btc };
            yield return new object[] { MultibaseEncoding.Base58Flickr };
        }

        private static char[] AlphabetOf(MultibaseEncoding encoding) =>
            encoding == MultibaseEncoding.Base58Btc ? LegacyBase58.BtcAlphabet : LegacyBase58.FlickrAlphabet;

        private static string EncodeCurrent(MultibaseEncoding encoding, byte[] bytes) =>
            encoding == MultibaseEncoding.Base58Btc || bytes.Length == 0
                ? Multibase.Base58.Encode(bytes)
                : Multibase.EncodeRaw(encoding, bytes);

        private static byte[] DecodeCurrent(MultibaseEncoding encoding, string input) =>
            encoding == MultibaseEncoding.Base58Btc || input.Length == 0
                ? Multibase.Base58.Decode(input)
                : Multibase.DecodeRaw(encoding, input);

        private static void AssertEncodeMatches(MultibaseEncoding encoding, byte[] bytes)
        {
            string expected = LegacyBase58.Encode(bytes, AlphabetOf(encoding));
            Assert.Equal(expected, EncodeCurrent(encoding, bytes));
        }

        private static void AssertDecodeMatches(MultibaseEncoding encoding, string input)
        {
            byte[] expected = LegacyBase58.Decode(input, AlphabetOf(encoding));
            Assert.Equal(expected, DecodeCurrent(encoding, input));
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void Encode_RandomInputs_MatchesLegacy(MultibaseEncoding encoding)
        {
            Random random = new Random(1115);
            for (int length = 0; length <= MaxLength; length++)
            {
                for (int round = 0; round < RoundsPerLength; round++)
                {
                    byte[] bytes = new byte[length];
                    random.NextBytes(bytes);
                    int zeros = round % 4 == 0 ? random.Next(length + 1) : 0;
                    Array.Clear(bytes, 0, zeros);
                    AssertEncodeMatches(encoding, bytes);
                    AssertDecodeMatches(encoding, LegacyBase58.Encode(bytes, AlphabetOf(encoding)));
                }
            }
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void Decode_RandomStrings_MatchesLegacy(MultibaseEncoding encoding)
        {
            char[] alphabet = AlphabetOf(encoding);
            Random random = new Random(511);
            StringBuilder builder = new StringBuilder(MaxLength);
            for (int length = 0; length <= MaxLength; length++)
            {
                for (int round = 0; round < RoundsPerLength; round++)
                {
                    builder.Clear();
                    int leading = round % 3 == 0 ? random.Next(length + 1) : 0;
                    bool withInvalid = round % 5 == 1;
                    for (int i = 0; i < length; i++)
                    {
                        if (i < leading)
                            builder.Append(alphabet[0]);
                        else if (withInvalid && random.Next(4) == 0)
                            builder.Append(InvalidChars[random.Next(InvalidChars.Length)]);
                        else
                            builder.Append(alphabet[random.Next(alphabet.Length)]);
                    }

                    AssertDecodeMatches(encoding, builder.ToString());
                }
            }
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void EdgeCases_MatchLegacy(MultibaseEncoding encoding)
        {
            char[] alphabet = AlphabetOf(encoding);
            char top = alphabet[alphabet.Length - 1];

            AssertEncodeMatches(encoding, new byte[0]);
            AssertEncodeMatches(encoding, new byte[] { 0 });
            AssertEncodeMatches(encoding, new byte[64]);
            AssertEncodeMatches(encoding, new byte[] { 0, 0, 1 });
            AssertEncodeMatches(encoding, new byte[] { 57 });
            AssertEncodeMatches(encoding, new byte[] { 58 });
            AssertEncodeMatches(encoding, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            AssertEncodeMatches(encoding, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
            AssertEncodeMatches(encoding, new byte[] { 0x27, 0x1F, 0x35, 0xA0 });
            byte[] large = new byte[1000];
            new Random(7).NextBytes(large);
            large[0] = 0;
            AssertEncodeMatches(encoding, large);
            byte[] ones = new byte[300];
            Array.Fill(ones, (byte)0xFF);
            AssertEncodeMatches(encoding, ones);

            AssertDecodeMatches(encoding, string.Empty);
            AssertDecodeMatches(encoding, new string(alphabet[0], 1));
            AssertDecodeMatches(encoding, new string(alphabet[0], 70));
            AssertDecodeMatches(encoding, new string(top, 1));
            AssertDecodeMatches(encoding, new string(top, 5));
            AssertDecodeMatches(encoding, new string(top, 6));
            AssertDecodeMatches(encoding, new string(top, 400));
            AssertDecodeMatches(encoding, "ÿ");
            AssertDecodeMatches(encoding, new string('ÿ', 5));
            AssertDecodeMatches(encoding, new string('ÿ', 200));
            AssertDecodeMatches(encoding, alphabet[0] + "ÿ" + alphabet[0]);
            AssertDecodeMatches(encoding, "0");
            AssertDecodeMatches(encoding, new string(alphabet[0], 3) + "00");
            AssertDecodeMatches(encoding, LegacyBase58.Encode(large, alphabet));
        }

        [Theory]
        [MemberData(nameof(Encodings))]
        public void Decode_CharAbove255_ThrowsSameExceptionAsLegacy(MultibaseEncoding encoding)
        {
            char[] alphabet = AlphabetOf(encoding);
            foreach (string input in new[] { "Ā", alphabet[0] + "中", "abcā", new string(alphabet[0], 4) + "￿" + alphabet[1] })
            {
                Assert.Throws<IndexOutOfRangeException>(() => LegacyBase58.Decode(input, alphabet));
                Assert.Throws<IndexOutOfRangeException>(() => DecodeCurrent(encoding, input));
            }
        }

        [Fact]
        public void NullInputs_ThrowSameExceptionTypeAsLegacy()
        {
            Assert.Throws<ArgumentNullException>(() => LegacyBase58.Encode(null, LegacyBase58.BtcAlphabet));
            Assert.Throws<ArgumentNullException>(() => Multibase.Base58.Encode(null));
            Assert.Throws<ArgumentNullException>(() => LegacyBase58.Decode(null, LegacyBase58.BtcAlphabet));
            Assert.Throws<ArgumentNullException>(() => Multibase.Base58.Decode(null));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Multiformats.Base.Tests
{
    public class MultibaseTests
    {
        [Fact]
        public void RoundTrip_Base2() => TestRoundTrip(Multibase.Base2.Encode);

        [Fact]
        public void RoundTrip_Base8() => TestRoundTrip(Multibase.Base8.Encode);

        [Fact]
        public void RoundTrip_Base10() => TestRoundTrip(Multibase.Base10.Encode);

        [Fact]
        public void RoundTrip_Base16Upper() => TestRoundTrip(Multibase.Base16.Encode);

        [Fact]
        public void RoundTrip_Base16Lower() => TestRoundTrip(b => Multibase.Base16.Encode(b, false));

        [Fact]
        public void RoundTrip_Base32() => TestRoundTrip(Multibase.Base32.Encode);

        [Fact]
        public void RoundTrip_Base32Padding() => TestRoundTrip(b => Multibase.Base32.Encode(b, true, false, false, false));

        [Fact]
        public void RoundTrip_Base32Hex() => TestRoundTrip(b => Multibase.Base32.Encode(b, false, true, true, false));

        [Fact]
        public void RoundTrip_Base32HexPadding() => TestRoundTrip(b => Multibase.Base32.Encode(b, true, true, true, false));

        [Fact]
        public void RoundTrip_Base32Z() => TestRoundTrip(b => Multibase.Base32.Encode(b, false, false, false, true));

        [Fact]
        public void RoundTrip_Base58Bitcoin() => TestRoundTrip(b => Multibase.Base58.Encode(b));

        [Fact]
        public void RoundTrip_Base58Flickr() => TestRoundTrip(b => Multibase.Base58.Encode(b, Base58Alphabet.Flickr));

        [Fact]
        public void RoundTrip_Base64() => TestRoundTrip(Multibase.Base64.Encode);

        [Fact]
        public void RoundTrip_Base64Padding() => TestRoundTrip(b => Multibase.Base64.Encode(b, true, false));

        [Fact]
        public void RoundTrip_Base64Url() => TestRoundTrip(b => Multibase.Base64.Encode(b, false, true));

        [Fact]
        public void RoundTrip_Base64UrlPadding() => TestRoundTrip(b => Multibase.Base64.Encode(b, true, true));

        private static void TestRoundTrip(Func<byte[], string> encode)
        {
            var rand = new Random(Environment.TickCount);
            var buf = new byte[rand.Next(16, 256)];
            rand.NextBytes(buf);

            var encoded = encode(buf);
            var decoded = Multibase.Decode(encoded);

            Assert.Equal(decoded, buf);
        }

        [Fact]
        public void EncodeDecode_Base2() => TestEncodeDecode(Multibase.Base2.Encode, "1101000 1100101 1101100 1101100 1101111 100000 1110111 1101111 1110010 1101100 1100100");

        [Fact]
        public void EncodeDecode_Base8() => TestEncodeDecode(Multibase.Base8.Encode, "150 145 154 154 157 40 167 157 162 154 144");

        [Fact]
        public void EncodeDecode_Base10() => TestEncodeDecode(Multibase.Base10.Encode, "104 101 108 108 111 32 119 111 114 108 100");

        [Fact]
        public void EncodeDecode_Base16Upper() => TestEncodeDecode(Multibase.Base16.Encode, "68656C6C6F20776F726C64");

        [Fact]
        public void EncodeDecode_Base16Lower() => TestEncodeDecode(b => Multibase.Base16.Encode(b, false), "68656c6c6f20776f726c64");

        [Fact]
        public void EncodeDecode_Base32() => TestEncodeDecode(Multibase.Base32.Encode, "NBSWY3DPEB3W64TMMQ");

        [Fact]
        public void EncodeDecode_Base32Padding() => TestEncodeDecode(b => Multibase.Base32.Encode(b, true), "NBSWY3DPEB3W64TMMQ======");

        [Fact]
        public void EncodeDecode_Base32Hex() => TestEncodeDecode(b => Multibase.Base32.Encode(b, false, hex: true), "D1IMOR3F41RMUSJCCG");

        [Fact]
        public void EncodeDecode_Base32HexPadding() => TestEncodeDecode(b => Multibase.Base32.Encode(b, true, hex: true), "D1IMOR3F41RMUSJCCG======");

        [Fact]
        public void EncodeDecode_Base32Z() => TestEncodeDecode(b => Multibase.Base32.Encode(b, false, zbase: true), "pb1sa5dxrb5s6hucco");

        [Fact]
        public void EncodeDecode_Base58Bitcoin() => TestEncodeDecode(b => Multibase.Base58.Encode(b), "StV1DL6CwTryKyV");

        [Fact]
        public void EncodeDecode_Base58Flickr() => TestEncodeDecode(b => Multibase.Base58.Encode(b, Base58Alphabet.Flickr), "rTu1dk6cWsRYjYu");

        [Fact]
        public void EncodeDecode_Base64() => TestEncodeDecode(Multibase.Base64.Encode, "aGVsbG8gd29ybGQ");

        [Fact]
        public void EncodeDecode_Base64Padding() => TestEncodeDecode(b => Multibase.Base64.Encode(b, true), "aGVsbG8gd29ybGQ=");

        [Fact]
        public void EncodeDecode_Base64Url() => TestEncodeDecode(b => Multibase.Base64.Encode(b, false, url: true), "aGVsbG8gd29ybGQ");

        [Fact]
        public void EncodeDecode_Base64UrlPadding() => TestEncodeDecode(b => Multibase.Base64.Encode(b, true, url: true), "aGVsbG8gd29ybGQ=");

        private static void TestEncodeDecode(Func<byte[], string> encode, string expected)
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = encode(bytes);

            Assert.Equal(encoded.Substring(1), expected);

            var decoded = Multibase.Decode(encoded);
            Assert.Equal(decoded, bytes);
        }

        [Fact]
        public void TestCustomSeparator()
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = Multibase.Base8.Encode(bytes, '-');

            Assert.Equal(encoded, "7150-145-154-154-157-40-167-157-162-154-144");

            var decoded = Multibase.Base8.Decode(encoded, '-');

            Assert.Equal(decoded, bytes);
            Assert.Equal(Encoding.UTF8.GetString(decoded), "hello world");
        }

        [Fact]
        public void TestBase16Mismatch_Lower()
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = Multibase.Base16.Encode(bytes, false).ToCharArray();
            encoded[0] = char.ToUpper(encoded[0]);

            Assert.Throws<Exception>(() => Multibase.Base16.Decode(new string(encoded)));
        }

        [Fact]
        public void TestBase16Mismatch_Upper()
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = Multibase.Base16.Encode(bytes, true).ToCharArray();
            encoded[0] = char.ToLower(encoded[0]);

            Assert.Throws<Exception>(() => Multibase.Base16.Decode(new string(encoded)));
        }

        [Fact]
        public void TestBase32Mismatch_Lower()
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = Multibase.Base32.Encode(bytes, true, false).ToCharArray();
            encoded[0] = char.ToUpper(encoded[0]);

            Assert.Throws<Exception>(() => Multibase.Base32.Decode(new string(encoded)));
        }

        [Fact]
        public void TestBase32Mismatch_Upper()
        {
            var bytes = Encoding.UTF8.GetBytes("hello world");
            var encoded = Multibase.Base32.Encode(bytes, true, true).ToCharArray();
            encoded[0] = char.ToLower(encoded[0]);

            Assert.Throws<Exception>(() => Multibase.Base32.Decode(new string(encoded)));
        }

        [Theory]
        [InlineData(typeof(Base32Encoding), "xhello world")]
        [InlineData(typeof(Base58Encoding), "xhello world")]
        [InlineData(typeof(Base64Encoding), "xhello world")]
        public void TestBases_NotSupportedIdentifiers(Type type, string invalid)
        {
            var encoder = (MultibaseEncoding)Activator.CreateInstance(type);

            Assert.Throws<NotSupportedException>(() => encoder.Decode(invalid));
        }

        [Fact]
        public void Encode_GivenUnsupportedEncoding_Throws()
        {
            Assert.Throws<NotSupportedException>(() => Multibase.Encode<TestEncoding>(Encoding.UTF8.GetBytes("hello world")));
        }

        private class TestEncoding : MultibaseEncoding
        {
            public override char[] Identifiers => throw new NotImplementedException();

            public override byte[] Decode(string str)
            {
                throw new NotImplementedException();
            }

            public override string Encode(byte[] data)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Decode_GivenUnknownIdentifier_Throws()
        {
            Assert.Throws<NotSupportedException>(() => Multibase.Decode("xhello world"));
        }

        [Fact]
        public void DecodeRaw_CanDecodeRawValue()
        {
            var rawEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello world")).Replace("-", "");
            var rawDecoded = Multibase.DecodeRaw(Multibase.Base64, rawEncoded);

            Assert.Equal(rawDecoded, Encoding.UTF8.GetBytes("hello world"));
        }

        [Fact]
        public void DecodeRawGeneric_CanDecodeRawValue()
        {
            var rawEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello world")).Replace("-", "");
            var rawDecoded = Multibase.DecodeRaw<Base64Encoding>(rawEncoded);

            Assert.Equal(rawDecoded, Encoding.UTF8.GetBytes("hello world"));
        }
    }
}
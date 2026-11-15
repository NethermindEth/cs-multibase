using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Multiformats.Base.Tests
{
    internal static class LegacyBase58
    {
        public static readonly char[] BtcAlphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();
        public static readonly char[] FlickrAlphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();

        public static string Encode(byte[] b, char[] alphabet)
        {
            return new string(
                b.TakeWhile(c => c == 0)
                .Select(_ => alphabet[0])
                .Concat(ParseBigInt(b.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t), alphabet).Reverse())
                .ToArray());
        }

        private static IEnumerable<char> ParseBigInt(BigInteger intData, char[] alphabet)
        {
            var len = alphabet.Length;
            while (intData > 0)
            {
                var rem = (int)(intData % len);
                intData /= len;
                yield return alphabet[rem];
            }
        }

        private static readonly ConcurrentDictionary<char[], byte[]> DecodeMap = new ConcurrentDictionary<char[], byte[]>();

        private static byte[] CreateDecodeMap(char[] alphabet)
        {
            var map = Enumerable.Range(0, 256).Select(b => (byte)0xFF).ToArray();
            for (var i = 0; i < alphabet.Length; i++)
                map[alphabet[i]] = (byte)i;
            return map;
        }

        public static byte[] Decode(string b, char[] alphabet)
        {
            lock (DecodeMap)
            {
                var decodeMap = DecodeMap.GetOrAdd(alphabet, CreateDecodeMap);
                var len = alphabet.Length;

                return b.TakeWhile(c => c == alphabet[0])
                    .Select(_ => (byte)0)
                    .Concat(b.Select(c => decodeMap[c])
                        .Aggregate<byte, BigInteger>(0, (current, c) => current * len + c)
                        .ToByteArray()
                        .Reverse()
                        .SkipWhile(c => c == 0))
                    .ToArray();
            }
        }
    }
}

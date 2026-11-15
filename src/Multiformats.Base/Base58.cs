using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;

namespace Multiformats.Base
{
    internal abstract class Base58 : Multibase
    {
        private const uint EncodeLimbBase = 656356768;
        private const int EncodeLimbDigits = 5;
        private const int DecodeChunkDigits = 5;
        private const int StackLimbs = 64;
        private const int StackChars = 256;

        protected static byte[] CreateDecodeMap(char[] alphabet)
        {
            byte[] map = new byte[256];
            map.AsSpan().Fill(0xFF);
            for (int i = 0; i < alphabet.Length; i++)
                map[alphabet[i]] = (byte)i;
            return map;
        }

        protected string Encode(byte[] b, char[] alphabet)
        {
            ArgumentNullException.ThrowIfNull(b);

            int zeros = 0;
            while (zeros < b.Length && b[zeros] == 0)
                zeros++;

            ReadOnlySpan<byte> rest = b.AsSpan(zeros);
            int maxLimbs = (int)(((long)rest.Length * 138 / 100 + 1) / EncodeLimbDigits + 2);
            uint[] rentedLimbs = null;
            Span<uint> limbs = maxLimbs <= StackLimbs
                ? stackalloc uint[StackLimbs]
                : (rentedLimbs = ArrayPool<uint>.Shared.Rent(maxLimbs));

            int used = 0;
            int pos = 0;
            int take = rest.Length % 4 == 0 ? 4 : rest.Length % 4;
            while (pos < rest.Length)
            {
                uint word = 0;
                for (int k = 0; k < take; k++)
                    word = (word << 8) | rest[pos + k];

                int shift = take * 8;
                ulong carry = word;
                for (int i = 0; i < used; i++)
                {
                    ulong value = ((ulong)limbs[i] << shift) + carry;
                    carry = value / EncodeLimbBase;
                    limbs[i] = (uint)(value - carry * EncodeLimbBase);
                }

                while (carry != 0)
                {
                    ulong next = carry / EncodeLimbBase;
                    limbs[used++] = (uint)(carry - next * EncodeLimbBase);
                    carry = next;
                }

                pos += take;
                take = 4;
            }

            int digitCount = 0;
            if (used > 0)
            {
                digitCount = (used - 1) * EncodeLimbDigits;
                for (uint top = limbs[used - 1]; top != 0; top /= 58)
                    digitCount++;
            }

            int length = zeros + digitCount;
            char[] rentedChars = null;
            Span<char> chars = length <= StackChars
                ? stackalloc char[StackChars]
                : (rentedChars = ArrayPool<char>.Shared.Rent(length));
            chars = chars.Slice(0, length);
            chars.Slice(0, zeros).Fill(alphabet[0]);

            int p = length;
            for (int i = 0; i < used - 1; i++)
            {
                uint limb = limbs[i];
                for (int k = 0; k < EncodeLimbDigits; k++)
                {
                    uint next = limb / 58;
                    chars[--p] = alphabet[(int)(limb - next * 58)];
                    limb = next;
                }
            }

            if (used > 0)
            {
                for (uint top = limbs[used - 1]; top != 0;)
                {
                    uint next = top / 58;
                    chars[--p] = alphabet[(int)(top - next * 58)];
                    top = next;
                }
            }

            string result = new string(chars);

            if (rentedLimbs != null)
                ArrayPool<uint>.Shared.Return(rentedLimbs);
            if (rentedChars != null)
                ArrayPool<char>.Shared.Return(rentedChars);

            return result;
        }

        protected byte[] Decode(string b, char[] alphabet, byte[] decodeMap)
        {
            ArgumentNullException.ThrowIfNull(b);

            char zeroChar = alphabet[0];
            int zeros = 0;
            while (zeros < b.Length && b[zeros] == zeroChar)
                zeros++;

            ReadOnlySpan<char> rest = b.AsSpan(zeros);
            int maxLimbs = (int)(((long)rest.Length * 733 / 1000 + 2) / 4 + 2);
            uint[] rentedLimbs = null;
            Span<uint> limbs = maxLimbs <= StackLimbs
                ? stackalloc uint[StackLimbs]
                : (rentedLimbs = ArrayPool<uint>.Shared.Rent(maxLimbs));

            int used = 0;
            int pos = 0;
            int take = rest.Length % DecodeChunkDigits == 0 ? DecodeChunkDigits : rest.Length % DecodeChunkDigits;
            while (pos < rest.Length)
            {
                ulong carry = 0;
                uint multiplier = 1;
                for (int k = 0; k < take; k++)
                {
                    carry = carry * 58 + decodeMap[rest[pos + k]];
                    multiplier *= 58;
                }

                for (int i = 0; i < used; i++)
                {
                    ulong value = (ulong)limbs[i] * multiplier + carry;
                    limbs[i] = (uint)value;
                    carry = value >> 32;
                }

                while (carry != 0)
                {
                    limbs[used++] = (uint)carry;
                    carry >>= 32;
                }

                pos += take;
                take = DecodeChunkDigits;
            }

            int byteCount = 0;
            if (used > 0)
                byteCount = (used - 1) * 4 + 4 - BitOperations.LeadingZeroCount(limbs[used - 1]) / 8;

            byte[] result = new byte[zeros + byteCount];
            int p = result.Length;
            for (int i = 0; i < used - 1; i++)
            {
                p -= 4;
                BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(p), limbs[i]);
            }

            if (used > 0)
            {
                for (uint top = limbs[used - 1]; top != 0; top >>= 8)
                    result[--p] = (byte)top;
            }

            if (rentedLimbs != null)
                ArrayPool<uint>.Shared.Return(rentedLimbs);

            return result;
        }
    }
}

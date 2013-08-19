using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Syndll2
{
    /// <summary>
    /// Implements the CRC algorithm from the Synel Communication Protocol user's guide.
    /// </summary>
    public static class SynelCRC
    {
        /// <summary>
        /// Calculates a CRC code for the value provided.
        /// </summary>
        /// <param name="s">The ASCII string value to calculate the CRC for.</param>
        /// <returns>A four-character ASCII string representing the CRC value.</returns>
        public static string Calculate(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            var ascii = Encoding.ASCII;
            var bytes = ascii.GetBytes(s);
            var crc = Calculate(bytes);
            return ascii.GetString(crc);
        }

        /// <summary>
        /// Calculates a CRC code for the value provided.
        /// </summary>
        /// <param name="bytes">An array of bytes of the value to calculate the CRC for.</param>
        /// <returns>A four-byte array representing the CRC value.</returns>
        public static byte[] Calculate(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var crc = CRC86(bytes);

            return new[]
                {
                    (byte) (0x30 + (crc >> 0xC & 0xF)),
                    (byte) (0x30 + (crc >> 0x8 & 0xF)),
                    (byte) (0x30 + (crc >> 0x4 & 0xF)),
                    (byte) (0x30 + (crc & 0xF))
                };
        }

        private static ushort CRC86(byte[] bytes)
        {
            ushort ax = 0;
            foreach (byte cx in bytes)
            {
                ushort dx = ax;
                ax >>= 8;
                ax ^= cx;
                ushort bx = ax;
                ax >>= 4;
                ax ^= bx;
                bx = ax;
                ax <<= 5;
                ax ^= bx;
                dx <<= 8;
                ax ^= dx;
                bx = ax;
                ax <<= 12;
                ax ^= bx;
            }

            return (ax);
        }

        /// <summary>
        /// Validates the CRC code for a given value.
        /// </summary>
        /// <param name="s">The string value.</param>
        /// <param name="crc">The four-character string CRC code to validate.</param>
        /// <returns>A value that indicates whether or not the CRC is valid.</returns>
        public static bool Verify(string s, string crc)
        {
            return Calculate(s) == crc;
        }

        /// <summary>
        /// Validates the CRC code for a given value.
        /// </summary>
        /// <param name="bytes">An array of bytes of the value.</param>
        /// <param name="crc">The four-byte array CRC code to validate.</param>
        /// <returns>A value that indicates whether or not the CRC is valid.</returns>
        public static bool Verify(byte[] bytes, byte[] crc)
        {
            if (crc.Length != 4)
                return false;

            var expected = Calculate(bytes);
            return expected.Length == 4 && NativeMethods.memcmp(expected, crc, 4) == 0;
        }

        private static class NativeMethods
        {
            /// <summary>
            /// Provides fast byte-array comparison.
            /// </summary>
            [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern int memcmp(byte[] b1, byte[] b2, long count);    
        }
    }
}


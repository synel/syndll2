using System.IO;

namespace Syndll2
{
    /// <summary>
    /// Implements the custom byte format used for fingerprint commands, described in section 4.1 of the Synel Protocol spec.
    /// </summary>
    internal static class SynelByteFormat
    {
        /// <summary>
        /// Converts a byte array to a Synel byte-formatted string.
        /// </summary>
        /// <param name="value">The bytes to convert.</param>
        /// <returns>A Synel byte-formatted string representation of the input value.</returns>
        public static string Convert(byte[] value)
        {
            if (value == null)
                return null;

            if (value.Length == 0)
                return string.Empty;

            var result = new char[value.Length * 2];

            for (int i = 0; i < value.Length; i++)
            {
                var z = value[i];
                var a = 0x60 + (z >> 4);
                var b = 0x30 + (z & 0xf);

                result[i * 2] = (char)a;
                result[(i * 2) + 1] = (char)b;
            }

            return new string(result);
        }

        /// <summary>
        /// Converts a Synel byte-formatted string to an array of bytes.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>A byte array representation of the input value.</returns>
        public static byte[] Convert(string value)
        {
            if (value == null)
                return null;

            if (value.Length == 0)
                return new byte[0];

            if (value.Length % 2 != 0)
                throw new InvalidDataException("The input string should be in pairs of ASCII characters, where two characters represents a single byte.");

            var result = new byte[value.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                var a = (byte)value[i * 2];
                var b = (byte)value[(i * 2) + 1];

                if (a < 0x60 || a > 0x6F || b < 0x30 || b > 0x3F)
                    throw new InvalidDataException("The input string contains characters that are outside of the range of allowed values.");

                result[i] = (byte)(((a & 0xf) << 4) + (b & 0xf));
            }

            return result;
        }
    }
}

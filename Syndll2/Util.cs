using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Syndll2
{
    internal class Util
    {
        public static string ByteArrayToString(byte[] bytes)
        {
            // http://stackoverflow.com/a/14333437

            var c = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        public static byte[] StringToByteArray(string hex)
        {
            // http://stackoverflow.com/a/14335533

            if (hex.Length % 2 == 1)
                throw new Exception("The hex string cannot have an odd number of digits");

            var array = new byte[hex.Length >> 1];

            for (int i = 0; i < array.Length; i++)
            {
                var hi = hex[i << 1];
                var lo = hex[(i << 1) + 1];
                array[i] = (byte)((GetHexVal(hi) << 4) + GetHexVal(lo));
            }

            return array;
        }

        private static byte GetHexVal(char hex)
        {
            if ((hex > 47 && hex < 58) || (hex > 64 && hex < 71))
                return (byte)(hex - (hex < 58 ? 48 : 55));

            throw new ArgumentException("A hexadecimal string can only contain characters 0-9 and A-F.");
        }

        public static byte CharToTerminalId(char c)
        {
            if (c < '0' || c == '@' || c > 'P')
                throw new ArgumentOutOfRangeException("c",
                                                      "Valid Terminal ID characters are '0' through 'P', skipping the '@' character.");
            // Skip over the @ character
            if (c > '@')
                c--;

            return (byte)(c - '0');
        }

        public static char TerminalIdToChar(int terminalId)
        {
            if (terminalId > 31)
                throw new ArgumentOutOfRangeException("terminalId", terminalId,
                                                      "The Terminal ID can only be 0 through 31.");

            // Skip over the @ character
            if (terminalId >= 16)
                terminalId++;

            return (char)('0' + terminalId);
        }

        public static void Log(string message)
        {
            var s = message.Length == 0
                        ? "(NO DATA)"
                        : message.Replace(ControlChars.EOT.ToString(CultureInfo.InvariantCulture), "(EOT)")
                                 .Replace(ControlChars.SOH.ToString(CultureInfo.InvariantCulture), "(SOH)")
                                 .Replace(ControlChars.ACK.ToString(CultureInfo.InvariantCulture), "(ACK)")
                                 .Replace(ControlChars.NACK.ToString(CultureInfo.InvariantCulture), "(NACK)");

            Trace.WriteLine(s);
        }
    }
}

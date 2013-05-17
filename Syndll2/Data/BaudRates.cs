using System;
using System.Linq;

namespace Syndll2.Data
{
    internal static class BaudRates
    {
        private static readonly int[] ValidRates = new[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };

        public static int Get(byte i)
        {
            if (i > ValidRates.Length)
                throw new ArgumentException("Invalid baud rate index " + i);

            return ValidRates[i];
        }

        public static byte IndexOf(int baudRate)
        {
            for (int i = 0; i < ValidRates.Length; i++)
                if (ValidRates[i] == baudRate)
                    return (byte)i;

            throw new ArgumentException("Invalid baud rate " + baudRate);
        }

        public static bool IsValid(int baudRate)
        {
            return ValidRates.Contains(baudRate);
        }
    }
}

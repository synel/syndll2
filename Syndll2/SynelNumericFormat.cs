using System;
using System.Globalization;

namespace Syndll2
{
    /// <summary>
    /// Implements the custom numeric format described in appendix A.6 of the Synel Protocol spec.
    /// </summary>
    internal static class SynelNumericFormat
    {
        /// <summary>
        /// Converts an integer to a Synel numeric format string.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <param name="chars">The number of characters to use in the output.</param>
        /// <returns>A Synel numeric format string representation of the input value.</returns>
        public static string Convert(int value, int chars)
        {
            if (chars < 1)
                throw new ArgumentOutOfRangeException("chars", "Must pass a valid number of characters to output.");

            if (value < 0)
                throw new ArgumentOutOfRangeException("value", "Cannot convert negative numbers.");

            // If it fits in pure decimal format, no conversion is necessary.
            var fit = (int)Math.Pow(10, chars);
            if (value < fit)
                return value.ToString(CultureInfo.InvariantCulture).PadLeft(chars, '0');


            var hi = (char) (48 + value/(fit/10));
            if (hi > 126) // Only supports the lower ascii printable set
                throw new ArgumentOutOfRangeException("value",
                                                      string.Format("Value {0} is too large to fit in {1} characters.",
                                                                    value, chars));
            var histr = hi.ToString(CultureInfo.InvariantCulture);
            if (chars == 1)
                return histr;

            var lo = value % (fit / 10);
            var lostr = chars == 1 ? "" : lo.ToString(CultureInfo.InvariantCulture).PadLeft(chars - 1, '0');
            return histr + lostr;
        }

        /// <summary>
        /// Converts a Synel numeric format string to an integer.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <returns>An integer representation of the input value.</returns>
        public static int Convert(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (value.Length == 0)
                throw new ArgumentException("Value cannot be empty.", "value");

            int i;
            if (int.TryParse(value, out i))
                return i;

            var hi = value[0] - 48;
            if (value.Length == 1)
                return hi;

            if (int.TryParse(value.Substring(1), out i))
                return hi * ((int)Math.Pow(10, value.Length - 1)) + i;

            throw new FormatException(string.Format("The value \"{0}\" is not valid!", value));
        }
    }
}

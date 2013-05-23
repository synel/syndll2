using System;
using System.Text;

namespace Syndll2
{
    internal static class Extensions
    {
        /// <summary>
        /// Removes a portion of a StringBuilder, and returns it as a string.
        /// </summary>
        public static string Cut(this StringBuilder sb, int startIndex, int length)
        {
            if (sb.Length < startIndex + length)
                throw new ArgumentException();

            var s = sb.ToString(startIndex, length);
            sb.Remove(startIndex, length);

            return s;
        }
    }
}

using System;
using System.ComponentModel;
using System.Reflection;
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

        /// <summary>
        /// Gets the value of any [Description] attribute attached to an enum.
        /// Returns null if it doesn't exist.
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute))
                            as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}

namespace System.Threading
{
    /// <summary>
    /// Contains constants that specify infinite time-out intervals. This class cannot be inherited.
    /// </summary>
    internal static class Timeout
    {
        /// <summary>
        /// A constant used to specify an infinite waiting period, for methods that accept a <see cref="T:System.TimeSpan"/> parameter.
        /// </summary>
        public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

        /// <summary>
        /// A constant used to specify an infinite waiting period.
        /// </summary>
        public const int Infinite = -1;
    }
}

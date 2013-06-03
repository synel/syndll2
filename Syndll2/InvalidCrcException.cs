using System;

namespace Syndll2
{
    /// <summary>
    /// Exception that is thrown when a CRC could not be verified
    /// </summary>
    [Serializable]
    public class InvalidCrcException : Exception
    {
        public InvalidCrcException(string message)
            : base(message)
        {
        }
    }
}

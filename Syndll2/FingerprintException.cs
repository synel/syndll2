using System;
using Syndll2.Data;

namespace Syndll2
{
    /// <summary>
    /// Exception that is thrown when a fingerprint command returns an error.
    /// </summary>
    [Serializable]
    public class FingerprintException : Exception
    {
        internal FingerprintException(FingerprintStatusCode statusCode)
            : base(statusCode.GetDescription())
        {
            _statusCode = statusCode;
        }

        private readonly FingerprintStatusCode _statusCode;

        /// <summary>
        /// The status code associated with this exception.
        /// </summary>
        public FingerprintStatusCode StatusCode
        {
            get { return _statusCode; }
        }
    }
}

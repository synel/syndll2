using System;

namespace Syndll2
{
    /// <summary>
    /// Represents a message that is received from a terminal.
    /// </summary>
    internal class ReceivedMessage
    {
        /// <summary>
        /// Gets the interpreted response received from the terminal.
        /// </summary>
        public Response Response { get; set; }
        
        /// <summary>
        /// Gets the raw response string returned from the terminal.
        /// Can be used for debugging purposes.
        /// </summary>
        public string RawResponse { get; set; }

        /// <summary>
        /// If there was an exception while parsing the response it will be contained here.
        /// </summary>
        public Exception Exception { get; set; }
    }
}

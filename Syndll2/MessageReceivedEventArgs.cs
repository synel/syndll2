using System;

namespace Syndll2
{
    /// <summary>
    /// Event arguments, used for events when a message is received from the terminal.
    /// </summary>
    internal class MessageReceivedEventArgs : EventArgs
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

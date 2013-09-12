using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Syndll2
{
    internal class Receiver
    {
        private readonly byte[] _rawReceiveBuffer = new byte[SynelClient.PacketOverheadSize]; // intentionally small
        private readonly List<byte> _receiveBuffer = new List<byte>(SynelClient.MaxPacketSize * 2);
        private readonly Stream _stream;
        private readonly Func<bool> _connected;

        public Receiver(Stream stream, Func<bool> connected)
        {
            _stream = stream;
            _connected = connected;
        }

        public Action<ReceivedMessage> MessageHandler { get; set; }

        public void WatchStream()
        {
            // Make sure we can still read from the stream.
            if (!_stream.CanRead)
                return;

            // Begin an async read operation on the stream.
            try
            {
                _stream.BeginRead(_rawReceiveBuffer, 0, _rawReceiveBuffer.Length, OnDataReceived, null);
            }
            catch
            {
                // Swallow any exceptions.  The stream is probably closed.
            }
        }

        private void OnDataReceived(IAsyncResult asyncResult)
        {
            // Make sure we're still connected.
            if (!_connected())
                return;

            // Make sure we can still read from the stream.
            if (!_stream.CanRead)
                return;

            // Conclude the async read operation.
            int bytesRead;
            try
            {
                bytesRead = _stream.EndRead(asyncResult);
            }
            catch
            {
                // Swallow any exceptions.  The stream is probably closed.
                return;
            }

            // Make sure the data is still good.
            // (We should never get back zeros at the start of the buffer, but it can happen during a forced disconnection.)
            if (bytesRead > 0 && _rawReceiveBuffer[0] == 0)
                return;

            // Copy the raw data read into the full receive buffer.
            _receiveBuffer.AddRange(_rawReceiveBuffer.Take(bytesRead));

            // Read packets from the buffer, unless there's still more on the stream to read.
            try
            {
                var networkStream = _stream as NetworkStream;
                if (networkStream == null || !networkStream.DataAvailable)
                    ReadFromBuffer();
            }
            catch (ObjectDisposedException ex)
            {
                // this can happen due to dropped connection
                Util.Log(ex.Message);
                return;
            }

            // Repeat, to continually watch the stream for incoming data.
            WatchStream();
        }

        private void ReadFromBuffer()
        {
            // See if there is an EOT in the read buffer
            int eotPosition;
            while ((eotPosition = _receiveBuffer.IndexOf((byte)ControlChars.EOT)) >= 0)
            {
                // see if this is the last one in the current buffer
                var lastInBuffer = eotPosition == _receiveBuffer.LastIndexOf((byte)ControlChars.EOT);

                // Pull out all before and including the EOT
                var size = eotPosition + 1;
                var data = _receiveBuffer.Take(size).ToArray();
                _receiveBuffer.RemoveRange(0, size);

                // Get a string representation of the packet data
                var packet = Encoding.ASCII.GetString(data);

                // Try to parse it
                var message = new ReceivedMessage { RawResponse = packet, LastInBuffer = lastInBuffer };
                try
                {
                    message.Response = Response.Parse(packet);
                }
                catch (InvalidCrcException)
                {
                    // Don't fire when the message fails the CRC check
                    continue;
                }
                catch (Exception ex)
                {
                    // pass any other exception into the event arguments
                    message.Exception = ex;
                }

                // Handle the message
                if (MessageHandler != null && _connected())
                    MessageHandler(message);
            }
        }
    }
}

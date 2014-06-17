using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Syndll2
{
    internal class AsyncReceiver
    {
        private readonly byte[] _rawReceiveBuffer = new byte[SynelClient.PacketOverheadSize]; // intentionally small
        private readonly List<byte> _receiveBuffer = new List<byte>(SynelClient.MaxPacketSize * 2);
        private readonly Stream _stream;

        public AsyncReceiver(Stream stream)
        {
            _stream = stream;
        }

        public async Task<ReceivedMessage> ReceiveMessageAsync(CancellationToken ct)
        {
            // Make sure we can still read from the stream.
            if (!_stream.CanRead)
                return null;


            while (!ct.IsCancellationRequested)
            {
                var bytesRead = await _stream.ReadAsync(_rawReceiveBuffer, 0, _rawReceiveBuffer.Length, ct);

                // Make sure the data is still good.
                // (We should never get back zeros at the start of the buffer, but it can happen during a forced disconnection.)
                if (bytesRead > 0 && _rawReceiveBuffer[0] == 0)
                    return null;

                // Copy the raw data read into the full receive buffer.
                _receiveBuffer.AddRange(_rawReceiveBuffer.Take(bytesRead));

                // See if there's still more on the stream to read.
                var networkStream = _stream as NetworkStream;
                if (networkStream != null && networkStream.DataAvailable)
                    continue;

                // Read and return the message from the buffer
                var message = ReadMessageFromBuffer();
                return message;
            }

            return null;
        }

        private ReceivedMessage ReadMessageFromBuffer()
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

                return message;
            }

            return null;
        }
    }
}

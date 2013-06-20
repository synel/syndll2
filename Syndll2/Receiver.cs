using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syndll2
{
    internal class Receiver
    {
        private readonly byte[] _rawReceiveBuffer = new byte[SynelClient.MaxPacketSize];
        private readonly List<byte> _receiveBuffer = new List<byte>(SynelClient.MaxPacketSize * 2);
        private readonly Stream _stream;

        public Receiver(Stream stream)
        {
            _stream = stream;
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            var handler = MessageReceived;
            if (handler != null)
                handler(this, args);
        }

        //public static Receiver Watch(Stream stream)
        //{
        //    var receiver = new Receiver(stream);
        //    Task.Factory.StartNew(receiver.WatchStream);
        //    return receiver;
        //}

        public void WatchStream()
        {
            // Make sure we can still read from the stream.
            if (!_stream.CanRead)
                return;

            // Begin an async read operation on the stream.
            _stream.BeginRead(_rawReceiveBuffer, 0, SynelClient.MaxPacketSize, OnDataReceived, null);
        }

        private void OnDataReceived(IAsyncResult asyncResult)
        {
            // Make sure we can still read from the stream.
            if (!_stream.CanRead)
                return;

            // Conclude the async read operation.
            var bytesRead = _stream.EndRead(asyncResult);

            // Make sure the data is still good.
            // (We should never get back zeros at the start of the buffer, but it can happen during a forced disconnection.)
            if (bytesRead > 0 && _rawReceiveBuffer[0] == 0)
                return;

            // Copy the raw data read into the full receive buffer.
            _receiveBuffer.AddRange(_rawReceiveBuffer.Take(bytesRead));

            // See if there is an EOT in the read buffer
            int eotPosition;
            while ((eotPosition = _receiveBuffer.IndexOf((byte)ControlChars.EOT)) >= 0)
            {
                // Pull out all before and including the EOT
                var size = eotPosition + 1;
                var data = new byte[size];
                _receiveBuffer.CopyTo(0, data, 0, size);
                _receiveBuffer.RemoveRange(0, size);

                // Get a string representation of the packet data
                var packet = Encoding.ASCII.GetString(data);

                // Try to parse it
                var args = new MessageReceivedEventArgs { RawResponse = packet };
                try
                {
                    args.Response = Response.Parse(packet);
                }
                catch (Exception ex)
                {
                    // pass any exception into the event arguments
                    args.Exception = ex;
                }

                // Raise the event
                OnMessageReceived(args);
            }

            // Repeat, to continually watch the stream for incoming data.
            WatchStream();
        }
    }
}

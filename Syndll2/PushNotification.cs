using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Syndll2.Data;

namespace Syndll2
{
    public class PushNotification
    {
        private readonly Stream _stream;
        private readonly Response _message;
        private readonly IPEndPoint _remoteEndPoint;

        internal PushNotification(Stream stream, Response message, IPEndPoint remoteEndPoint)
        {
            _stream = stream;
            _message = message;
            _remoteEndPoint = remoteEndPoint;
        }

        public string RawMessage
        {
            get { return _message.RawResponse; }
        }

        public int TerminalId
        {
            get { return _message.TerminalId; }
        }

        public string Data
        {
            get { return _message.Data; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _remoteEndPoint; }
        }

        public NotificationType Type
        {
            get
            {
                return (NotificationType)_message.Command;
            }
        }

        public void Acknowledege()
        {
            if (Type != NotificationType.Data)
                throw new InvalidOperationException("Acknowledge is only valid for data notifications.");

            var command = SynelClient.CreateCommand(RequestCommand.AcknowledgeLastRecord, TerminalId);

            SendResponse(command);
        }

        public void Reply(bool allowed, string message, TimeSpan displayTime = default(TimeSpan), TextAlignment alignment = TextAlignment.Left)
        {
            if (Type != NotificationType.Query)
                throw new InvalidOperationException("Reply is only valid for query notifications.");

            var displayTimeInSeconds = (int)displayTime.TotalSeconds;
            if (displayTimeInSeconds < -1 || displayTimeInSeconds > 9)
                throw new ArgumentOutOfRangeException("displayTime",
                    "Display time must be between 0 and 9 seconds, or pass -1 seconds to send a # to the terminal program.");

            message = message.TrimEnd();
            if (message.Length >= 100)
                message = message.Substring(0, 99);

            const int screenWidth = 16;
            if (message.Length < screenWidth)
            {
                switch (alignment)
                {
                    case TextAlignment.Left:
                        message = message.PadRight(screenWidth, ' ');
                        break;

                    case TextAlignment.Center:
                        message = message.PadLeft((message.Length + screenWidth) / 2).PadRight(screenWidth);
                        break;

                    case TextAlignment.Right:
                        message = message.PadLeft(screenWidth, ' ');
                        break;
                }
            }

            message = message.PadRight(32); // at least 32 chars to clear the display
            message += " "; // extra char to avoid truncation behavior on the terminal

            var data = string.Format(CultureInfo.InvariantCulture, "L{0}{1:D2}{2}{3}",
                allowed ? "Y" : "N",
                message.Length,
                displayTimeInSeconds < 0 ? "#" : displayTimeInSeconds.ToString(CultureInfo.InvariantCulture),
                message);
            var command = SynelClient.CreateCommand(RequestCommand.QueryReply, TerminalId, data);

            SendResponse(command);
        }

        private void SendResponse(string command)
        {
            if (!_stream.CanWrite)
            {
                Util.Log(string.Format("Couldn't Send: {0}", command));
                return;
            }

            Util.Log(string.Format("Sending: {0}", command));

            var bytes = Encoding.ASCII.GetBytes(command);
            _stream.Write(bytes, 0, bytes.Length);

            if (!_stream.CanRead)
                return;

            // Discard ACK replies to responses.  It keeps the line clean, and we don't need them.
            var recieveBuffer = new byte[SynelClient.MaxPacketSize];
            var bytesRecieved = _stream.Read(recieveBuffer, 0, SynelClient.MaxPacketSize);
            if (bytesRecieved > 0)
            {
                // At least log them for debugging.
                var s = Encoding.ASCII.GetString(recieveBuffer, 0, bytesRecieved);
                Util.Log(string.Format("Received: {0}", s));
            }
        }
    }
}

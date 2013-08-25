using System;
using System.Globalization;
using Syndll2.Data;

namespace Syndll2
{
    public class PushNotification
    {
        private readonly Response _message;
        private readonly SynelClient _client;

        internal PushNotification(SynelClient client, Response message)
        {
            _client = client;
            _message = message;
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

        public NotificationType Type
        {
            get
            {
                return (NotificationType)_message.Command;
            }
        }

        public SynelClient Client
        {
            get
            {
                return _client;
            }
        }

        public void Acknowledege()
        {
            if (Type != NotificationType.Data)
                throw new InvalidOperationException("Acknowledge is only valid for data notifications.");

            _client.SendAndReceive(RequestCommand.AcknowledgeLastRecord, null, ACK);
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

            _client.SendAndReceive(RequestCommand.QueryReply, data, ACK);
        }

        
        // ReSharper disable InconsistentNaming
        private readonly string ACK = ControlChars.ACK.ToString(CultureInfo.InvariantCulture);
        // ReSharper restore InconsistentNaming
    }
}

using System;
using System.Globalization;
using System.Threading.Tasks;
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
            var task = AcknowledegeAsync();
            task.Wait();
            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
        }

        public async Task AcknowledegeAsync()
        {
            if (Type != NotificationType.Data)
                throw new InvalidOperationException("Acknowledge is only valid for data notifications.");

            await _client.SendAndReceiveAsync(RequestCommand.AcknowledgeLastRecord, null, ACK);
        }

        public void Reply(bool allowed, int code, string message, TextAlignment alignment = TextAlignment.Left)
        {
            var task = ReplyAsync(allowed, code, message, alignment);
            task.Wait();
            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
        }

        public async Task ReplyAsync(bool allowed, int code, string message, TextAlignment alignment = TextAlignment.Left)
        {
            if (Type != NotificationType.Query)
                throw new InvalidOperationException("Reply is only valid for query notifications.");

            if (code < -1 || code > 9)
                throw new ArgumentOutOfRangeException("code",
                    "Code must be between 0 and 9, or pass -1 seconds to send a # to the terminal program.");

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
                code < 0 ? "#" : code.ToString(CultureInfo.InvariantCulture),
                message);

            await _client.SendAndReceiveAsync(RequestCommand.QueryReply, data, 3, 300, ACK);
        }

        
        // ReSharper disable InconsistentNaming
        private readonly string ACK = ControlChars.ACK.ToString(CultureInfo.InvariantCulture);
        // ReSharper restore InconsistentNaming
    }
}

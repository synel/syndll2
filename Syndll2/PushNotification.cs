using System.IO;
using System.Text;

namespace Syndll2
{
    public class PushNotification
    {
        private readonly Stream _stream;
        private readonly Response _message;

        internal PushNotification(Stream stream, Response message)
        {
            _stream = stream;
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
            get { return _message.Command == PrimaryResponseCommand.DataRecord ? _message.Data : null; }
        }

        public void Acknowledege()
        {
            var command = SynelClient.CreateCommand(RequestCommand.AcknowledgeLastRecord, TerminalId);

            var bytes = Encoding.ASCII.GetBytes(command);
            _stream.Write(bytes, 0, bytes.Length);
        }
    }
}

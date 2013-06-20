using System.IO;
using System.Net;
using System.Text;

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
            get { return _message.Command == PrimaryResponseCommand.DataRecord ? _message.Data : null; }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return _remoteEndPoint; }
        }

        public void Acknowledege()
        {
            var command = SynelClient.CreateCommand(RequestCommand.AcknowledgeLastRecord, TerminalId);

            var bytes = Encoding.ASCII.GetBytes(command);
            _stream.Write(bytes, 0, bytes.Length);
        }
    }
}

using System;
using System.IO;

namespace Syndll2
{
    internal class Response : IEquatable<Response>
    {
        public string RawResponse { get; private set; }
        public PrimaryResponseCommand Command { get; private set; }
        public string Data { get; private set; }
        
        public Response(string rawResponse, PrimaryResponseCommand command, string data = null)
        {
            RawResponse = rawResponse;
            Command = command;
            Data = data;
        }

        /// <summary>
        /// Parses the response string returned from the terminal, ensuring that the response is valid.
        /// </summary>
        /// <param name="s">The entire response string, including ACK, NACK, CRC, and EOT where appropriate.</param>
        /// <param name="expectedTerminalId">The terminal ID that is expected in the response.</param>
        /// <returns>A validated <see cref="Response"/> object.</returns>
        internal static Response Parse(string s, int expectedTerminalId)
        {
            // check valid input
            if (s == null)
                throw new ArgumentNullException("s");

            // check for too small or too large packet size, which should also be reported as a bad CRC.
            if (s.Length < SynelClient.PacketOverheadSize || s.Length > SynelClient.MaxPacketSize)
                throw new InvalidCrcException("Invalid CRC received from the terminal.");

            // check crc
            var packet = s.Substring(0, s.Length - 5);
            var crc = s.Substring(s.Length - 5, 4);
            if (!SynelCRC.Verify(packet, crc))
                throw new InvalidCrcException("Invalid CRC received from the terminal.");

            // Get command
            var cmd = packet[0];
            if (!Enum.IsDefined(typeof(PrimaryResponseCommand), (int)cmd))
                throw new InvalidDataException("Unknown command received: " + cmd);
            var command = (PrimaryResponseCommand)cmd;

            // Get terminal
            byte terminalId = Util.CharToTerminalId(packet[1]);
            if (terminalId != expectedTerminalId)
                throw new InvalidOperationException(
                    string.Format("Got data for terminal {0} while talking to terminal {1}", terminalId, expectedTerminalId));

            // Return when there is no data
            if (packet.Length == 2)
                return new Response(s, command);

            // Return when there IS data
            var data = packet.Substring(2);
            return new Response(s, command, data);
        }

        public bool Equals(Response other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RawResponse == other.RawResponse;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Response)obj);
        }

        public override int GetHashCode()
        {
            return RawResponse.GetHashCode();
        }

        public static bool operator ==(Response left, Response right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Response left, Response right)
        {
            return !Equals(left, right);
        }
    }
}

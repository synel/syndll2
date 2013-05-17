using System;

namespace Syndll2
{
    internal class Response : IEquatable<Response>
    {
        public PrimaryResponseCommand Command { get; private set; }
        public string Data { get; private set; }

        public Response(PrimaryResponseCommand command, string data = null)
        {
            Command = command;
            Data = data;
        }

        public bool Equals(Response other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Command == other.Command && string.Equals(Data, other.Data);
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
            unchecked
            {
                return ((int)Command * 397) ^ (Data != null ? Data.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Response left, Response right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Response left, Response right)
        {
            return !Equals(left, right);
        }

        public static Response Acknowledged
        {
            get { return new Response(PrimaryResponseCommand.Acknowledged); }
        }

        public static Response NotAcknowledged
        {
            get { return new Response(PrimaryResponseCommand.NotAcknowledged); }
        }
    }
}

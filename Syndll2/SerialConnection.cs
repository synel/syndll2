using System;
using System.IO;
using System.Threading.Tasks;

namespace Syndll2
{
    /// <summary>
    /// Implements the details of communicating with a terminal over a serial-port connection.
    /// </summary>
    internal class SerialConnection : IConnection
    {
        public Stream Stream
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Connected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private SerialConnection()
        {
            throw new NotImplementedException();
        }

        public static NetworkConnection Connect() // TODO: pass serial port parameters
        {
            throw new NotImplementedException();
        }

#if NET_45
        public static async Task<NetworkConnection> ConnectAsync() // TODO: pass serial port parameters
        {
            throw new NotImplementedException();
        }
#endif
    }
}

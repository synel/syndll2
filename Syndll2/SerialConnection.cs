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
        private bool _disposed;

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
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                throw new NotImplementedException();
            }

            _disposed = true;
        }

        private SerialConnection()
        {
            throw new NotImplementedException();
        }

        public static NetworkConnection Connect() // TODO: pass serial port parameters
        {
            throw new NotImplementedException();
        }

        public static async Task<NetworkConnection> ConnectAsync() // TODO: pass serial port parameters
        {
            throw new NotImplementedException();
        }
    }
}

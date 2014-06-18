using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Syndll2.FunctionalTests
{
    public static class TestSettings
    {
        public static string HostAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["HostAddress"];
            }
        }

        public static int TcpPort
        {
            get
            {
                int port;
                return int.TryParse(ConfigurationManager.AppSettings["TcpPort"], out port)
                           ? port
                           : 3734; // Synel Default Port
            }
        }

        public static int TerminalId
        {
            get
            {
                int terminalId;
                return int.TryParse(ConfigurationManager.AppSettings["TerminalId"], out terminalId)
                           ? terminalId
                           : 0;
            }
        }

        public static SynelClient Connect()
        {
            return SynelClient.Connect(HostAddress, TcpPort, TerminalId, TimeSpan.FromSeconds(5));
        }

        public static Task<SynelClient> ConnectAsync()
        {
            return SynelClient.ConnectAsync(HostAddress, TcpPort, TerminalId, TimeSpan.FromSeconds(5));
        }
    }
}

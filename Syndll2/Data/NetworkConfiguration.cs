using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace Syndll2.Data
{
    public class NetworkConfiguration
    {
        private readonly NetworkCardTypes _networkCardType;
        private readonly bool _enableSendMAC;
        private readonly bool _enablePolling;
        private readonly TimeSpan _pollingInterval;
        private readonly TransportType _transportType;
        private readonly PhysicalAddress _terminalMACAddress;
        private readonly IPAddress _terminalIPAddress;
        private readonly IPAddress _gatewayIPAddress;
        private readonly IPAddress _remoteIPAddress;
        private readonly IPAddress _subnetMask;
        private readonly int _terminalPort;
        private readonly int _remotePort;
        private readonly TimeSpan _disconnectTime;
        private readonly bool _enableDHCP;
        private readonly int _networkCardFirmwareVersion;
        
        public NetworkCardTypes NetworkCardType
        {
            get { return _networkCardType; }
        }

        public bool EnableSendMAC
        {
            get { return _enableSendMAC; }
        }

        public bool EnablePolling
        {
            get { return _enablePolling; }
        }

        public TimeSpan PollingInterval
        {
            get { return _pollingInterval; }
        }

        public TransportType TransportType
        {
            get { return _transportType; }
        }

        public PhysicalAddress TerminalMACAddress
        {
            get { return _terminalMACAddress; }
        }

        public IPAddress TerminalIPAddress
        {
            get { return _terminalIPAddress; }
        }

        public IPAddress GatewayIPAddress
        {
            get { return _gatewayIPAddress; }
        }

        public IPAddress RemoteIPAddress
        {
            get { return _remoteIPAddress; }
        }

        public IPAddress SubnetMask
        {
            get { return _subnetMask; }
        }

        public int TerminalPort
        {
            get { return _terminalPort; }
        }

        public int RemotePort
        {
            get { return _remotePort; }
        }

        public TimeSpan DisconnectTime
        {
            get { return _disconnectTime; }
        }

        public bool EnableDHCP
        {
            get { return _enableDHCP; }
        }

        public int NetworkCardFirmwareVersion
        {
            get { return _networkCardFirmwareVersion; }
        }
        

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // S0HNF0000000EE30284860A0A0B800A0A0A0100000000FFFFFE000373403734000000504574
        // CT012345678901234567890123456789012345678901234567890123456789012345678CCCC
        //   0         1         2         3         4         5         6

        // Data Field Breakdown
        // HN F 0 00 0 000EE3028486 0A0A0B80 0A0A0A01 00000000 FFFFFE00 03734 03734 00000 0 50

        internal static NetworkConfiguration Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 69;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Network configuration data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new NetworkConfiguration(data);
        }

        private NetworkConfiguration(string data)
        {
            var subcode = data.Substring(0, 2);
            if (subcode != "HN")
                throw new InvalidDataException(
                    string.Format("Expected sub-code of \"{0}\" but got \"{1}\".", "HN", subcode));

            _networkCardType = (NetworkCardTypes)data[2];
            _enableSendMAC = data[3] == '2' || data[3] == '3';
            _enablePolling = data[3] == '1' || data[3] == '3';

            int i;
            if (!int.TryParse(data.Substring(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i))
                throw new InvalidDataException("Couldn't parse the polling interval from network configuration data.");
            _pollingInterval = TimeSpan.FromSeconds(i);

            _transportType = data[6] == '0' ? TransportType.Tcp : data[6] == '1' ? TransportType.Udp : 0;

            _terminalMACAddress = PhysicalAddress.Parse(data.Substring(7, 12));
            _terminalIPAddress = new IPAddress(Util.StringToByteArray(data.Substring(19, 8)));
            _gatewayIPAddress = new IPAddress(Util.StringToByteArray(data.Substring(27, 8)));
            _remoteIPAddress = new IPAddress(Util.StringToByteArray(data.Substring(35, 8)));
            _subnetMask = new IPAddress(Util.StringToByteArray(data.Substring(43, 8)));

            if (!int.TryParse(data.Substring(51, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _terminalPort))
                throw new InvalidDataException("Couldn't parse the terminal port number from network configuration data.");

            if (!int.TryParse(data.Substring(56, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _remotePort))
                throw new InvalidDataException("Couldn't parse the remote port number from network configuration data.");

            if (!int.TryParse(data.Substring(61, 5), NumberStyles.None, CultureInfo.InvariantCulture, out i))
                throw new InvalidDataException("Couldn't parse the disconnect time from network configuration data.");
            _disconnectTime = TimeSpan.FromSeconds(i);

            _enableDHCP = data[66] == '1';

            if (!int.TryParse(data.Substring(67, 2), NumberStyles.None, CultureInfo.InvariantCulture, out _networkCardFirmwareVersion))
                throw new InvalidDataException("Couldn't parse the network card firmware version from network configuration data.");
        }

        
    }
}

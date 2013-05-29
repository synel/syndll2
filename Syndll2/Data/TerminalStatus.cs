using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace Syndll2.Data
{
    public class TerminalStatus
    {
        private readonly byte _hardwareModel;
        private readonly byte _hardwareRevision;
        private readonly int _firmwareVersion;
        private readonly DateTime _timestamp;
        private readonly char _activeFunction;
        private readonly TerminalTypes _terminalType;
        private readonly bool _poweredOn;
        private readonly int _buffersFull;
        private readonly int _buffersFaulty;
        private readonly int _buffersTransmitted;
        private readonly int _buffersEmpty;
        private readonly int _memoryUsed;
        private readonly int _userDefinedField;
        private readonly TimeSpan _pollingInterval;
        private readonly TransportType _transportType;
        private readonly FingerprintUnitModes _fingerprintUnitMode;

        public byte HardwareModel
        {
            get { return _hardwareModel; }
        }

        public byte HardwareRevision
        {
            get { return _hardwareRevision; }
        }

        public int FirmwareVersion
        {
            get { return _firmwareVersion; }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public char ActiveFunction
        {
            get { return _activeFunction; }
        }

        public TerminalTypes TerminalType
        {
            get { return _terminalType; }
        }

        public bool PoweredOn
        {
            get { return _poweredOn; }
        }

        public int BuffersFull
        {
            get { return _buffersFull; }
        }

        public int BuffersFaulty
        {
            get { return _buffersFaulty; }
        }

        public int BuffersTransmitted
        {
            get { return _buffersTransmitted; }
        }

        public int BuffersEmpty
        {
            get { return _buffersEmpty; }
        }

        public int MemoryUsed
        {
            get { return _memoryUsed; }
        }

        public int UserDefinedField
        {
            get { return _userDefinedField; }
        }

        public TimeSpan PollingInterval
        {
            get { return _pollingInterval; }
        }

        public TransportType TransportType
        {
            get { return _transportType; }
        }

        public FingerprintUnitModes FingerprintUnitMode
        {
            get { return _fingerprintUnitMode; }
        }


        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // s082824341304291524I000000000m280078KV1400000T00S??FpPPPAPPAP0;67
        // CT01234567890123456789012345678901234567890123456789012345678CCCC
        //   0         1         2         3         4         5         

        // Data Field Breakdown
        // 8 2 82434 1304291524 I 000 000 000 m28 0078K V 1 40 0000 T 00 S ??FpPPPAPPAP

        internal static TerminalStatus Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 59;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Status data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new TerminalStatus(data);
        }

        private TerminalStatus(string data)
        {
            if (!byte.TryParse(data.Substring(0, 1), NumberStyles.None, CultureInfo.InvariantCulture, out _hardwareModel))
                throw new InvalidDataException("Couldn't parse hardware model from terminal status data.");

            if (!byte.TryParse(data.Substring(1, 1), NumberStyles.None, CultureInfo.InvariantCulture, out _hardwareRevision))
                throw new InvalidDataException("Couldn't parse hardware revision from terminal status data.");

            if (!int.TryParse(data.Substring(2, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _firmwareVersion))
                throw new InvalidDataException("Couldn't parse firmware version from terminal status data.");

            if (!DateTime.TryParseExact(data.Substring(7, 10), "yyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _timestamp))
                throw new InvalidDataException("Couldn't parse timestamp from terminal status data.");

            _activeFunction = data[17];

            _buffersFull = SynelNumericFormat.Convert(data.Substring(18, 3));
            _buffersFaulty = SynelNumericFormat.Convert(data.Substring(21, 3));
            _buffersTransmitted = SynelNumericFormat.Convert(data.Substring(24, 3));
            _buffersEmpty = SynelNumericFormat.Convert(data.Substring(27, 3));

            if (data[34] == 'K')
            {
                int mem;
                if (!int.TryParse(data.Substring(30, 4), NumberStyles.None, CultureInfo.InvariantCulture, out mem))
                    throw new InvalidDataException("Couldn't parse memory used from terminal status data.");
                _memoryUsed = mem * 1024; // assuming 'K' means KiB rather than KB
            }
            else
            {
                if (!int.TryParse(data.Substring(30, 4), NumberStyles.None, CultureInfo.InvariantCulture, out _memoryUsed))
                    throw new InvalidDataException("Couldn't parse memory used from terminal status data.");
            }

            _terminalType = (TerminalTypes)data[35];
            _poweredOn = data[36] == '1';

            // TODO: what does 37-38 represent?

            if (!int.TryParse(data.Substring(39, 4), NumberStyles.None, CultureInfo.InvariantCulture, out _userDefinedField))
                throw new InvalidDataException("Couldn't parse user defined field from terminal status data.");

            _transportType = data[43] == 'T' ? TransportType.Tcp : data[43] == 'U' ? TransportType.Udp : 0;

            int i;
            if (!int.TryParse(data.Substring(44, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i))
                throw new InvalidDataException("Couldn't parse the polling interval from terminal status data.");
            _pollingInterval = TimeSpan.FromSeconds(i);

            _fingerprintUnitMode = (FingerprintUnitModes)data[46];
        }
    }
}

using System;
using System.Globalization;
using System.IO;

namespace Syndll2.Data
{
    public class HardwareConfiguration
    {
        private readonly int _firmwareVersion;
        private readonly DateTime _firmwareDate;
        private readonly TerminalTypes _terminalType;
        private readonly KeyboardTypes _keyboardType;
        private readonly DisplayTypes _displayType;
        private readonly string _hostSerialParameters;
        private readonly int _hostSerialBaudRate;
        private readonly byte _terminalId;
        private readonly byte _fingerprintUnitType;
        private readonly FingerprintUnitModes _fingerprintUnitMode;
        private readonly int _userDefinedField;

        public int FirmwareVersion
        {
            get { return _firmwareVersion; }
        }

        public DateTime FirmwareDate
        {
            get { return _firmwareDate; }
        }

        public TerminalTypes TerminalType
        {
            get { return _terminalType; }
        }

        public KeyboardTypes KeyboardType
        {
            get { return _keyboardType; }
        }

        public DisplayTypes DisplayType
        {
            get { return _displayType; }
        }

        public string HostSerialParameters
        {
            get { return _hostSerialParameters; }
        }

        public int HostSerialBaudRate
        {
            get { return _hostSerialBaudRate; }
        }

        public byte TerminalId
        {
            get { return _terminalId; }
        }

        public byte FingerprintUnitType
        {
            get { return _fingerprintUnitType; }
        }

        public FingerprintUnitModes FingerprintUnitMode
        {
            get { return _fingerprintUnitMode; }
        }

        public int UserDefinedField
        {
            get { return _userDefinedField; }
        }
        

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // S0HG82434@17.09.2011-VBT8n140F3S0000<8:4
        // CT0123456789012345678901234567890123CCCC
        //   0         1         2         3

        // Data Field Breakdown
        // HG 82434 @17.09.2011 - V B T 8n14 0 F 3 S 0000

        internal static HardwareConfiguration Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 34;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Hardware configuration data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new HardwareConfiguration(data);
        }

        private HardwareConfiguration(string data)
        {
            var subcode = data.Substring(0, 2);
            if (subcode != "HG")
                throw new InvalidDataException(
                    string.Format("Expected sub-code of \"{0}\" but got \"{1}\".", "HG", subcode));

            if (!int.TryParse(data.Substring(2, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _firmwareVersion))
                throw new InvalidDataException("Couldn't parse firmware version from hardware configuration data.");

            if (!DateTime.TryParseExact(data.Substring(8, 10), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _firmwareDate))
                throw new InvalidDataException("Couldn't parse firmware date from hardware configuration data.");

            // TODO: what does 18 represent?

            _terminalType = (TerminalTypes)data[19];
            _keyboardType = (KeyboardTypes)data[20];
            _displayType = (DisplayTypes)data[21];
            _hostSerialParameters = data.Substring(22, 3);

            byte b;
            if (!byte.TryParse(data.Substring(25, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse host serial baud rate from hardware configuration data.");
            _hostSerialBaudRate = BaudRates.Get(b);

            _terminalId = Util.CharToTerminalId(data[26]);

            // TODO: what does 27 represent?  Maybe the network card type?

            if (!byte.TryParse(data.Substring(28, 1), NumberStyles.None, CultureInfo.InvariantCulture, out _fingerprintUnitType))
                throw new InvalidDataException("Couldn't parse fingerprint unit type from hardware configuration data.");

            _fingerprintUnitMode = (FingerprintUnitModes)data[29];

            if (!int.TryParse(data.Substring(30, 4), NumberStyles.None, CultureInfo.InvariantCulture, out _userDefinedField))
                throw new InvalidDataException("Couldn't parse user defined field from hardware configuration data.");
        }
    }
}

using System;
using System.Globalization;
using System.IO;

namespace Syndll2.Data
{
    public class TechnicianModeSettings
    {
        private readonly byte _terminalId;
        private readonly SerialPortModes _serialPort0Mode;
        private readonly SerialPortModes _serialPort1Mode;
        private readonly SerialPortModes _serialPort2Mode;
        private readonly int _serialPort0BaudRate;
        private readonly int _serialPort1BaudRate;
        private readonly int _serialPort2BaudRate;
        private readonly bool _hasModem;
        private readonly byte _modemRingsToAnswer;
        private readonly NetworkCardTypes _networkCardType;

        public byte TerminalId
        {
            get { return _terminalId; }
        }

        public SerialPortModes SerialPort0Mode
        {
            get { return _serialPort0Mode; }
        }

        public SerialPortModes SerialPort1Mode
        {
            get { return _serialPort1Mode; }
        }

        public SerialPortModes SerialPort2Mode
        {
            get { return _serialPort2Mode; }
        }

        public int SerialPort0BaudRate
        {
            get { return _serialPort0BaudRate; }
        }

        public int SerialPort1BaudRate
        {
            get { return _serialPort1BaudRate; }
        }

        public int SerialPort2BaudRate
        {
            get { return _serialPort2BaudRate; }
        }

        public bool HasModem
        {
            get { return _hasModem; }
        }

        public byte ModemRingsToAnswer
        {
            get { return _modemRingsToAnswer; }
        }

        public NetworkCardTypes NetworkCardType
        {
            get { return _networkCardType; }
        }


        public override string ToString()
        {
            return string.Format("HPS{0:D2}{1}{2}{3}{4}{5}{6}{7}{8}",
                                 _terminalId,
                                 (byte)_serialPort0Mode,
                                 (byte)_serialPort1Mode,
                                 (byte)_serialPort2Mode,
                                 BaudRates.IndexOf(_serialPort0BaudRate),
                                 BaudRates.IndexOf(_serialPort1BaudRate),
                                 BaudRates.IndexOf(_serialPort2BaudRate),
                                 _hasModem ? _modemRingsToAnswer.ToString(CultureInfo.InvariantCulture) : "N",
                                 (char)_networkCardType
                );
        }

        public TechnicianModeSettings(byte terminalId = 0,
                                      SerialPortModes serialPort0Mode = SerialPortModes.Host,
                                      SerialPortModes serialPort1Mode = SerialPortModes.FingerprintUnit,
                                      SerialPortModes serialPort2Mode = SerialPortModes.Printer,
                                      int serialPort0BaudRate = 19200,
                                      int serialPort1BaudRate = 57600,
                                      int serialPort2BaudRate = 19200,
                                      bool hasModem = false,
                                      byte modemRingsToAnswer = 0,
                                      NetworkCardTypes networkCardType = NetworkCardTypes.F_Ethernet100Mbps)
        {
            if (terminalId > 31)
                throw new ArgumentException("Invalid Terminal ID.  Terminal ID should be 0 to 31.", "terminalId");
            _terminalId = terminalId;

            if (serialPort0Mode  != SerialPortModes.Host)
                throw new ArgumentException("Invalid mode for serial port 0.", "serialPort0Mode");
            _serialPort0Mode = serialPort0Mode;

            if (serialPort1Mode != SerialPortModes.FingerprintUnit && serialPort1Mode != SerialPortModes.CardReader)
                throw new ArgumentException("Invalid mode for serial port 1.", "serialPort1Mode");
            _serialPort1Mode = serialPort1Mode;

            if (serialPort2Mode != SerialPortModes.Printer && serialPort2Mode != SerialPortModes.CardReader)
                throw new ArgumentException("Invalid mode for serial port 2.", "serialPort2Mode");
            _serialPort2Mode = serialPort2Mode;

            if (!BaudRates.IsValid(serialPort0BaudRate))
                throw new ArgumentException("Invalid baud rate for serial port 0.", "serialPort0BaudRate");
            _serialPort0BaudRate = serialPort0BaudRate;

            if (!BaudRates.IsValid(serialPort1BaudRate))
                throw new ArgumentException("Invalid baud rate for serial port 1.", "serialPort1BaudRate");
            _serialPort1BaudRate = serialPort1BaudRate;

            if (!BaudRates.IsValid(serialPort2BaudRate))
                throw new ArgumentException("Invalid baud rate for serial port 2.", "serialPort2BaudRate");
            _serialPort2BaudRate = serialPort2BaudRate;

            _hasModem = hasModem;
            if (hasModem)
            {
                if (modemRingsToAnswer > 9)
                    throw new ArgumentException("Modem rings to answer must be 0 through 9", "modemRingsToAnswer");
                _modemRingsToAnswer = modemRingsToAnswer;
            }

            if (!Enum.IsDefined(typeof (NetworkCardTypes), networkCardType))
                throw new ArgumentException("Invalid network card type.", "networkCardType");
            _networkCardType = networkCardType;
        }


        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // S0HPG00012464NF0<:3>
        // CT01234567890123CCCC
        //   0         1

        // Data Field Breakdown
        // HPG 00 0 1 2 4 6 4 N F 0

        internal static TechnicianModeSettings Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 14;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Technician mode settings data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new TechnicianModeSettings(data);
        }

        private TechnicianModeSettings(string data)
        {
            var subcode = data.Substring(0, 3);
            if (subcode != "HPG")
                throw new InvalidDataException(
                    string.Format("Expected sub-code of \"{0}\" but got \"{1}\".", "HPG", subcode));

            if (!byte.TryParse(data.Substring(3, 2), NumberStyles.None, CultureInfo.InvariantCulture, out _terminalId))
                throw new InvalidDataException("Couldn't parse terminal id from technician mode settings data.");

            byte b;
            if (!byte.TryParse(data.Substring(5, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 0 mode from technician mode settings data.");
            _serialPort0Mode = (SerialPortModes)b;

            if (!byte.TryParse(data.Substring(6, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 1 mode from technician mode settings data.");
            _serialPort1Mode = (SerialPortModes)b;

            if (!byte.TryParse(data.Substring(7, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 2 mode from technician mode settings data.");
            _serialPort2Mode = (SerialPortModes)b;

            if (!byte.TryParse(data.Substring(8, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 0 baud rate from technician mode settings data.");
            _serialPort0BaudRate = BaudRates.Get(b);

            if (!byte.TryParse(data.Substring(9, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 1 baud rate from technician mode settings data.");
            _serialPort1BaudRate = BaudRates.Get(b);

            if (!byte.TryParse(data.Substring(10, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse serial port 2 baud rate from technician mode settings data.");
            _serialPort2BaudRate = BaudRates.Get(b);

            _hasModem = data[11] != 'N';
            if (_hasModem)
            {
                if (!byte.TryParse(data.Substring(11, 1), NumberStyles.None, CultureInfo.InvariantCulture, out _modemRingsToAnswer))
                    throw new InvalidDataException("Couldn't parse modem rings to answer from technician mode settings data.");
            }

            _networkCardType = (NetworkCardTypes)data[12];
        }
    }
}

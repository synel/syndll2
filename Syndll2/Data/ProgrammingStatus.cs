using System;

namespace Syndll2.Data
{
    public class ProgrammingStatus
    {
        private readonly ProgrammingOperationStatus _operationStatus;
        private readonly char _operationType;           // TODO: Need an enum for this
        private readonly string _fileName;
        private readonly char _terminalMode;            // TODO: Need an enum for this
        private readonly char _programmingModeState;    // TODO: Need an enum for this
        private readonly int _previousBlockNumber;
        private readonly int _currentBlockNumber;
        private readonly string _debugInfo;

        public ProgrammingOperationStatus OperationStatus
        {
            get { return _operationStatus; }
        }

        public char OperationType
        {
            get { return _operationType; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public char TableType
        {
            get { return TableId == -1 ? ' ' : _fileName[0]; }
        }

        public int TableId
        {
            get
            {
                int i;
                if (_fileName != null && _fileName.Length == 4 && int.TryParse(_fileName.Substring(1), out i))
                    return i;

                return -1;
            }
        }

        public char TerminalMode
        {
            get { return _terminalMode; }
        }

        public char ProgrammingModeState
        {
            get { return _programmingModeState; }
        }

        public int PreviousBlockNumber
        {
            get { return _previousBlockNumber; }
        }

        public int CurrentBlockNumber
        {
            get { return _currentBlockNumber; }
        }

        public string DebugInfo
        {
            get { return _debugInfo; }
        }

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // (ACK)0MP1536PH000000L593<
        // CCCCCT012345678901234CCCC
        //       0         1

        // Data Field Breakdown
        // M P 1536 P H 00 00 00L   (from halt)
        // C N Run_ N R 00 00 00L   (from run)
        // W R V800 P 3 01 01 00L   (from replace table)
        // W D V800 P 2 41 41 00L   (from delete table)

        internal static ProgrammingStatus Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Length != 1 && data.Length != 15)
                throw new ArgumentException(
                    string.Format(
                        "Program debug status data should be either {0} or {1} characters, and you passed {2} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", 1, 15, data.Length),
                    "data");

            return new ProgrammingStatus(data);
        }

        private ProgrammingStatus(string data)
        {
            _operationStatus = (ProgrammingOperationStatus) data[0];

            if (data.Length == 1)
                return;

            _operationType = data[1];
            _fileName = data.Substring(2, 4);
            _terminalMode = data[6];
            _programmingModeState = data[7];
            _previousBlockNumber = SynelNumericFormat.Convert(data.Substring(8, 2));
            _currentBlockNumber = SynelNumericFormat.Convert(data.Substring(10, 2));
            _debugInfo = data.Substring(12, 3);
        }
    }
}

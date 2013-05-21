using System;

namespace Syndll2.Data
{
    public class ProgramDebugStatus
    {
        private readonly char _operationType;
        private readonly string _fileName;
        private readonly char _terminalMode;
        private readonly char _programmingModeState;
        private readonly int _previousBlockNumber;
        private readonly int _currentBlockNumber;
        private readonly string _debuggingArea;

        public char OperationType
        {
            get { return _operationType; }
        }

        public string FileName
        {
            get { return _fileName; }
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

        public string DebuggingArea
        {
            get { return _debuggingArea; }
        }

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // (ACK)0MP1536PH000000L593<
        // CCCCCT012345678901234CCCC
        //       0         1
        
        // Data Field Breakdown
        // M P153 6 P H 00 00 00L   (from halt)
        // C NRun _ N R 00 00 00L   (from run)

        internal static ProgramDebugStatus Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 15;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Program debug status data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new ProgramDebugStatus(data);
        }

        private ProgramDebugStatus(string data)
        {
            // Waiting on Synel to clarify breakdown
            throw new NotImplementedException();
        }
    }
}

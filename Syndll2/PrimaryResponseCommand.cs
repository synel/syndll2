namespace Syndll2
{
    // From the Synel Communications Protocol User's Manual, Section 2.2.1.

    internal enum PrimaryResponseCommand
    {
        /// <summary>
        /// The terminal is currently busy and temporary unable to respond the host command.
        /// </summary>
        Busy = 'b',

        /// <summary>
        /// The command is accompanied by a data record.
        /// </summary>
        DataRecord = 'd',

        /// <summary>
        /// Data stored on the terminal that meets the host request parameters is not found.
        /// </summary>
        NoData = 'n',

        /// <summary>
        /// There is a terminal query directed to the host.
        /// </summary>
        QueryForHost = 'q',

        /// <summary>
        /// The command is accompanied by a data that represents the terminal current status.
        /// </summary>
        TerminalStatus = 's',

        /// <summary>
        /// Memory commands from the host to the terminal.
        /// </summary>
        SystemCommands = 'S',

        /// <summary>
        /// The terminal acknowledges receiving the transmitted host block.
        /// </summary>
        BlockReceived = 't',

        /// <summary>
        /// The command is accompanied by a data that represents the last received command from the host and whether it is comprehensible to terminal.
        /// The same code is also used as a response to any fingerprint commands.
        /// </summary>
        LastCommand_Or_Fingerprint = 'v',

        /// <summary>
        /// Terminal acknowledges that the host command was received clearly.
        /// </summary>
        Acknowledged = ControlChars.ACK,

        /// <summary>
        /// Terminal notifies that the host command was not received clearly.
        /// </summary>
        NotAcknowledged = ControlChars.NACK
    }
}

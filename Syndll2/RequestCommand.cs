namespace Syndll2
{
    // From the Synel Communications Protocol User's Manual, Section 2.1.

    internal enum RequestCommand
    {
        /// <summary>
        /// Initiates a sequence for terminal transmission of a full data block of up to 128 bytes.
        /// </summary>
        GetFullDataBlock = 'A', // called "SendData" in the spec

        /// <summary>
        /// Initiates sequence for a terminal transmission of a partly-full data block.
        /// </summary>
        GetData = 'B',  // called "SendAll" in the spec

        /// <summary>
        /// Directs terminal to clear all transmitted and acknowledged records stored at the memory buffer.
        /// </summary>
        ClearBuffer = 'C',

        /// <summary>
        /// Directs terminal to clear all transmitted and acknowledged records stored at the memory buffer according to the specified date.
        /// </summary>
        ClearByDate = 'c',

        /// <summary>
        /// Initiates a sequence receiving data on the terminal's status.
        /// </summary>
        GetStatus = 'D',

        /// <summary>
        /// Initiates a sequence that sets the terminal's internal clock and active function.
        /// </summary>
        SetStatus = 'E',

        /// <summary>
        /// The host has received the terminal’s last data record transmission and it forwards a notification.
        /// </summary>
        AcknowledgeLastRecord = 'F',

        /// <summary>
        /// Directs the terminal to reset all transmitted and acknowledged records and mark them as unsent stored records.
        /// </summary>
        ResetBuffer = 'G',

        /// <summary>
        /// Tells the terminal to display a specific message,
        /// </summary>
        DisplayMessage = 'H', // section 3.5 in the spec
        
        /// <summary>
        /// Initiates a sequence for table operation execution as in load, delete, replace.
        /// </summary>
        TableOperation = 'I',

        /// <summary>
        /// Commands the terminal to terminate normal operation mode and proceed to programming mode.
        /// </summary>
        Halt = 'K',

        /// <summary>
        /// Commands the terminal to terminate programming mode and proceed to normal operation mode.
        /// </summary>
        Run = 'L',

        /// <summary>
        /// Sends an existing query, not sent if data was not found. An N is sent if QUERY does not exist.
        /// </summary>
        SendOnlyQuery = 'O',

        /// <summary>
        /// The terminal produces the query response. There are three responses: T=Timeout, O=Offline, B=Busy, L=Long query.
        /// </summary>
        QueryReply = 'Q',

        /// <summary>
        /// Directs the terminal to terminate transmission, reestablish communication and waits for the next SEND DATA for SEND ALL command.
        /// </summary>
        ResetLine = 'R',

        /// <summary>
        /// Memory commands from the terminal to the host.
        /// </summary>
        SystemCommands = 'S',

        /// <summary>
        /// Fingerprint commands for SY780 terminals.
        /// </summary>
        Fingerprint = 'V'
    }
}

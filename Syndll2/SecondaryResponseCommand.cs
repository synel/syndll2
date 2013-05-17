namespace Syndll2
{
    // From the Synel Communications Protocol User's Manual, Section 2.2.2.

    internal enum SecondaryResponseCommand
    {
        /// <summary>
        /// The terminal is currently ready to receive programming tables and denies accepting transaction input.
        /// </summary>
        InProgrammingMode = 'C',

        /// <summary>
        /// A table with the same ID already exists in the terminal's memory.
        /// </summary>
        TableAlreadyExists = 'E',

        /// <summary>
        /// The terminal's memory is full.
        /// </summary>
        MemoryFull = 'F',

        /// <summary>
        /// A data block was received and processed to be stored at the terminal's memory.
        /// </summary>
        BlockReceivedAndBeingStored = 'G',

        /// <summary>
        /// The command execution is in process by the terminal.
        /// </summary>
        PerformingLastCommand = 'L',

        /// <summary>
        /// A transaction input is currently in progress at the terminal.
        /// </summary>
        TransactionInProgress = 'R',

        /// <summary>
        /// A data block was received and already stored at the terminal's memory.
        /// </summary>
        BlockReceivedAndStored = 'W',

        
    }
}

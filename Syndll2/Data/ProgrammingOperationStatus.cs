namespace Syndll2.Data
{
    public enum ProgrammingOperationStatus
    {
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
        /// A data block was received and already stored at the terminal's memory.
        /// </summary>
        BlockReceivedAndStored = 'W',

        /// <summary>
        /// The terminal is currently running the program.
        /// </summary>
        /// <remarks>
        /// This is the normal response from a a Run command.
        /// </remarks>
        InRunMode = 'C',

        /// <summary>
        /// The terminal is halted and ready to receive programming.
        /// </summary>
        /// <remarks>
        /// This is the normal response from a a Halt command.
        /// </remarks>
        InProgrammingMode = 'M',
    }
}

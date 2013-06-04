using System;

namespace Syndll2
{
    /// <summary>
    /// Event arguments, used for reporting progress status when uploading data to the terminal.
    /// </summary>
    public class UploadProgressChangedEventArgs : EventArgs
    {
        private readonly int _currentBlock;
        private readonly int _totalBlocks;
        private readonly string _filename;

        internal UploadProgressChangedEventArgs(int currentBlock, int totalBlocks, string filename)
        {
            _currentBlock = currentBlock;
            _totalBlocks = totalBlocks;
            _filename = filename;
        }

        /// <summary>
        /// Gets the current block number.
        /// </summary>
        public int CurrentBlock { get { return _currentBlock; } }

        /// <summary>
        /// Gets the total blocks to upload.
        /// </summary>
        public int TotalBlocks { get { return _totalBlocks; } }

        /// <summary>
        /// Gets the current percentage completed.
        /// </summary>
        public double ProgressPercentage
        {
            get { return TotalBlocks == 0 ? 0 : (double) CurrentBlock/TotalBlocks; }
        }

        /// <summary>
        /// Gets the name of the file being uploaded, if available.
        /// </summary>
        public string Filename
        {
            get { return _filename; }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Syndll2.Data;

namespace Syndll2
{
    /// <summary>
    /// Provides methods that perform programming operations on the terminal.
    /// </summary>
    public class ProgrammingOperations : IDisposable
    {
        const int MaxBlockSize = 119; // per spec section 3.3.2 and 3.3.4.

        private readonly SynelClient _client;
        private bool _disposed;

        internal ProgrammingOperations(SynelClient client)
        {
            _client = client;
            _client.Terminal.Halt();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _client.Terminal.Run();

            _disposed = true;
        }

        #region UploadTable
        /// <summary>
        /// Uploads an RDY file to the terminal.
        /// </summary>
        /// <param name="path">The path to the RDY file.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public void UploadTableFromFile(string path, bool replace = true)
        {
            if (!string.Equals(Path.GetExtension(path), ".rdy", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Pass the full or relative path to a .RDY file.");

            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Could not find a file at {0}", Path.GetFullPath(path)));

            var fileName = Path.GetFileName(path);  
            
            // Read the file into an RDY
            RdyFile rdy;
            Util.Log("Reading " + fileName);
            using (var stream = File.OpenRead(path))
            {
                rdy = RdyFile.Read(stream);
            }

            // Check for directory files
            if (rdy.IsDirectoryFile)
            {
                // Upload each file referenced separately
                foreach (var record in rdy.Records)
                {
                    // find the new full path
                    var p = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + record.Data;

                    // recurse to upload each file
                    UploadTableFromFile(p, replace);
                }
            }
            else
            {
                // Just upload the single RDY
                Util.Log("Uploading " + fileName);
                UploadTableFromRdy(rdy, replace);
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads an RDY file to the terminal.
        /// </summary>
        /// <param name="path">The path to the RDY file.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public async Task UploadTableFromFileAsync(string path, bool replace = true)
        {
            if (!string.Equals(Path.GetExtension(path), ".rdy", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Pass the full or relative path to a .RDY file.");

            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Could not find a file at {0}", Path.GetFullPath(path)));

            var fileName = Path.GetFileName(path);  

            // Read the file into an RDY
            RdyFile rdy;
            Util.Log("Reading " + fileName);
            using (var stream = File.OpenRead(path))
            {
                rdy = await RdyFile.ReadAsync(stream);
            }

            // Check for directory files
            if (rdy.IsDirectoryFile)
            {
                // Upload each file referenced separately
                foreach (var record in rdy.Records)
                {
                    // find the new full path
                    var p = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + record.Data;

                    // recurse to upload each file
                    await UploadTableFromFileAsync(p, replace);
                }
            }
            else
            {
                // Just upload the single RDY
                Util.Log("Uploading " + fileName);
                await UploadTableFromRdyAsync(rdy, replace);
            }
        }
#endif

        /// <summary>
        /// Uploads a stream containing an RDY file to the terminal.
        /// </summary>
        /// <param name="stream">The stream containing the RDY file content.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public void UploadTableFromStream(Stream stream, bool replace = true)
        {
            var rdy = RdyFile.Read(stream);
            UploadTableFromRdy(rdy, replace);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads a stream containing an RDY file to the terminal.
        /// </summary>
        /// <param name="stream">The stream containing the RDY file content.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public async Task UploadTableFromStreamAsync(Stream stream, bool replace = true)
        {
            var rdy = RdyFile.Read(stream);
            await UploadTableFromRdyAsync(rdy, replace);
        }
#endif

        /// <summary>
        /// Uploads an RDY file object to the terminal.
        /// </summary>
        /// <param name="rdy">The RDY file object.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public void UploadTableFromRdy(RdyFile rdy, bool replace = true)
        {
            // make sure we're not uploading a directory file
            if (rdy.IsDirectoryFile)
                throw new InvalidOperationException("Cannot upload a directory file to the terminal.");

            // calculate how many blocks we'll need to send
            var totalBlocks = (int)Math.Ceiling(rdy.Header.TotalCharacters / (double)MaxBlockSize);
            var blockNumber = 1;

            // create a buffer and append the header
            var buffer = new StringBuilder(MaxBlockSize * 2);
            buffer.Append(rdy.Header);

            // get the table information from the header
            var tableType = rdy.Header.TableType;
            var tableId = rdy.Header.TableId;

            foreach (var record in rdy.Records)
            {
                // append the record to the buffer
                buffer.Append(record.Data);

                // don't send the buffer until we have a full block
                if (buffer.Length < MaxBlockSize)
                    continue;

                // cut a block from the buffer and send it
                var block = buffer.Cut(0, MaxBlockSize);
                SendBlock(tableType, tableId, blockNumber, totalBlocks, block, replace);
                blockNumber++;
            }

            // send a partial block if there is any data remaining in the buffer
            if (buffer.Length > 0)
            {
                var block = buffer.ToString();
                SendBlock(tableType, tableId, blockNumber, totalBlocks, block, replace);
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads an RDY file object to the terminal.
        /// </summary>
        /// <param name="rdy">The RDY file object.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        public async Task UploadTableFromRdyAsync(RdyFile rdy, bool replace = true)
        {
            // make sure we're not uploading a directory file
            if (rdy.IsDirectoryFile)
                throw new InvalidOperationException("Cannot upload a directory file to the terminal.");

            // calculate how many blocks we'll need to send
            var totalBlocks = (int)Math.Ceiling(rdy.Header.TotalCharacters / (double)MaxBlockSize);
            var blockNumber = 1;

            // create a buffer and append the header
            var buffer = new StringBuilder(MaxBlockSize * 2);
            buffer.Append(rdy.Header);

            // get the table information from the header
            var tableType = rdy.Header.TableType;
            var tableId = rdy.Header.TableId;

            foreach (var record in rdy.Records)
            {
                // append the record to the buffer
                buffer.Append(record.Data);

                // don't send the buffer until we have a full block
                if (buffer.Length < MaxBlockSize)
                    continue;

                // cut a block from the buffer and send it
                var block = buffer.Cut(0, MaxBlockSize);
                await SendBlockAsync(tableType, tableId, blockNumber, totalBlocks, block, replace);
                blockNumber++;
            }

            // send a partial block if there is any data remaining in the buffer
            if (buffer.Length > 0)
            {
                var block = buffer.ToString();
                await SendBlockAsync(tableType, tableId, blockNumber, totalBlocks, block, replace);
            }
        }
#endif

        private void SendBlock(char tableType, int tableId, int blockNumber, int totalBlocks, string block, bool replace)
        {
            var data = tableType +
                       tableId.ToString("D3") +
                       (replace ? 'R' : 'S') +
                       SynelNumericFormat.Convert(totalBlocks, 2) +
                       SynelNumericFormat.Convert(blockNumber, 2) +
                       block;

            var response = _client.SendAndReceive(RequestCommand.TableOperation, data, "t");

            ValidateSendBlockResult(response);
        }

#if NET_45
        private async Task SendBlockAsync(char tableType, int tableId, int blockNumber, int totalBlocks, string block, bool replace)
        {
            var data = tableType +
                       tableId.ToString("D3") +
                       (replace ? 'R' : 'S') +
                       SynelNumericFormat.Convert(totalBlocks, 2) +
                       SynelNumericFormat.Convert(blockNumber, 2) +
                       block;

            var response = await _client.SendAndReceiveAsync(RequestCommand.TableOperation, data, "t");

            ValidateSendBlockResult(response);
        }
#endif

        private static void ValidateSendBlockResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.BlockReceived)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.BlockReceived, response.Command));

            var status = ProgrammingStatus.Parse(response.Data);

            switch (status.OperationStatus)
            {
                case ProgrammingOperationStatus.MemoryFull:
                    throw new InvalidOperationException("The terminal's memory is full!  Cannot proceed with programming.");

                case ProgrammingOperationStatus.TableAlreadyExists:
                    throw new InvalidOperationException("The table already exists.  If you want to replace the table, set replace=true.");

                case ProgrammingOperationStatus.BlockReceivedAndStored:
                case ProgrammingOperationStatus.BlockReceivedAndBeingStored:  // TODO? should we do anything special for this case?
                    return; // OK
            }

            throw new InvalidOperationException(string.Format("Unknown return status '{0}'.", (char)status.OperationStatus));
        }

        #endregion

        #region DeleteTable
        /// <summary>
        /// Deletes a specific table from the terminal.
        /// </summary>
        /// <param name="tableType">The table type.</param>
        /// <param name="tableId">The table ID.</param>
        public void DeleteTable(char tableType, int tableId)
        {
            if (tableId < 0 || tableId > 999)
                throw new ArgumentOutOfRangeException("tableId", "Table ID must be between 0 and 999.");

            var data = string.Format("{0}{1:D3}D", tableType, tableId);

            var response = _client.SendAndReceive(RequestCommand.TableOperation, data, "t");
            ValidateDeleteTableResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that deletes a specific table from the terminal.
        /// </summary>
        /// <param name="tableType">The table type.</param>
        /// <param name="tableId">The table ID.</param>
        public async Task DeleteTableAsync(char tableType, int tableId)
        {
            if (tableId < 0 || tableId > 999)
                throw new ArgumentOutOfRangeException("tableId", "Table ID must be between 0 and 999.");

            var data = string.Format("{0}{1:D3}D", tableType, tableId);

            var response = await _client.SendAndReceiveAsync(RequestCommand.TableOperation, data, "t");
            ValidateDeleteTableResult(response);
        }
#endif

        private static void ValidateDeleteTableResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.BlockReceived)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.BlockReceived, response.Command));

            var status = ProgrammingStatus.Parse(response.Data);

            switch (status.OperationStatus)
            {
                case ProgrammingOperationStatus.BlockReceivedAndStored:
                case ProgrammingOperationStatus.BlockReceivedAndBeingStored:  // TODO? should we do anything special for this case?
                    return; // OK
            }

            throw new InvalidOperationException(string.Format("Unknown return status '{0}'.", (char)status.OperationStatus));
        }

        #endregion

        #region DeleteAllTables
        /// <summary>
        /// Deletes ALL tables from the terminal.
        /// </summary>
        public void DeleteAllTables()
        {
            var response = _client.SendAndReceive(RequestCommand.TableOperation, "@@@@D", "t");
            ValidateDeleteTableResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that deletes ALL tables from the terminal.
        /// </summary>
        public async Task DeleteAllTablesAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.TableOperation, "@@@@D", "t");
            ValidateDeleteTableResult(response);
        }
#endif
        #endregion

        #region EraseAllMemoryFromTerminal
        /// <summary>
        /// ERASES ALL MEMORY FROM THE TERMINAL!
        /// </summary>
        public void EraseAllMemoryFromTerminal()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "F", ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that ERASES ALL MEMORY FROM THE TERMINAL!
        /// </summary>
        public async Task EraseAllMemoryFromTerminalAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "F", ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }
#endif
        #endregion

        private readonly string ACK = ControlChars.ACK.ToString(CultureInfo.InvariantCulture);
    }
}

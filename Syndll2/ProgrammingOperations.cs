using System;
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
        private readonly Lazy<FingerprintOperations> _fingerprintOperations;
        private bool _disposed;

        /// <summary>
        /// Provides access to fingerprint methods.
        /// </summary>
        public FingerprintOperations Fingerprint
        {
            get { return _fingerprintOperations.Value; }
        }

        internal ProgrammingOperations(SynelClient client)
        {
            _fingerprintOperations = new Lazy<FingerprintOperations>(() => new FingerprintOperations(client));
            _client = client;
            try
            {
                _client.Terminal.Halt();
            }
            catch
            {
                _client.Terminal.Run();
                throw;
            }
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
        
        /// <summary>
        /// Event that is raised repeatedly to indicate progress while programming the terminal.
        /// </summary>
        public event EventHandler<UploadProgressChangedEventArgs> ProgressChanged;

        private void OnProgressChanged(UploadProgressChangedEventArgs args)
        {
            var handler = ProgressChanged;
            if (handler != null)
                handler(this, args);
        }

        #region UploadTable
        /// <summary>
        /// Uploads an RDY file to the terminal.
        /// </summary>
        /// <param name="path">The path to the RDY file.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public void UploadTableFromFile(string path, bool replace = true, bool force = false)
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
                rdy = RdyFile.Read(stream, force);
                rdy.Filename = fileName;
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
                    UploadTableFromFile(p, replace, force);
                }
            }
            else
            {
                // Just upload the single RDY
                Util.Log("Uploading " + fileName);
                UploadTableFromRdy(rdy, replace, force);
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads an RDY file to the terminal.
        /// </summary>
        /// <param name="path">The path to the RDY file.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public async Task UploadTableFromFileAsync(string path, bool replace = true, bool force = false)
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
                rdy = await RdyFile.ReadAsync(stream, force);
                rdy.Filename = fileName;
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
                    await UploadTableFromFileAsync(p, replace, force);
                }
            }
            else
            {
                // Just upload the single RDY
                Util.Log("Uploading " + fileName);
                await UploadTableFromRdyAsync(rdy, replace, force);
            }
        }
#endif

        /// <summary>
        /// Uploads a stream containing an RDY file to the terminal.
        /// </summary>
        /// <param name="stream">The stream containing the RDY file content.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public void UploadTableFromStream(Stream stream, bool replace = true, bool force = false)
        {
            var rdy = RdyFile.Read(stream, force);
            UploadTableFromRdy(rdy, replace, force);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads a stream containing an RDY file to the terminal.
        /// </summary>
        /// <param name="stream">The stream containing the RDY file content.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public async Task UploadTableFromStreamAsync(Stream stream, bool replace = true, bool force = false)
        {
            var rdy = RdyFile.Read(stream, force);
            await UploadTableFromRdyAsync(rdy, replace, force);
        }
#endif

        /// <summary>
        /// Uploads an RDY file object to the terminal.
        /// </summary>
        /// <param name="rdy">The RDY file object.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public void UploadTableFromRdy(RdyFile rdy, bool replace = true, bool force = false)
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

            // reset the progress info
            OnProgressChanged(new UploadProgressChangedEventArgs(0, totalBlocks, rdy.Filename));

            foreach (var record in rdy.Records)
            {
                // append the record to the buffer
                buffer.Append(record.Data.PadRight(rdy.Header.RecordSize));

                // don't send the buffer until we have a full block
                if (buffer.Length < MaxBlockSize)
                    continue;

                // cut a block from the buffer and send it
                var block = buffer.Cut(0, MaxBlockSize);
                SendBlock(tableType, tableId, blockNumber, totalBlocks, block, replace, rdy.Filename);
                blockNumber++;
            }

            // send a partial block if there is any data remaining in the buffer
            if (buffer.Length > 0)
            {
                var block = buffer.ToString();
                SendBlock(tableType, tableId, blockNumber, totalBlocks, block, replace, rdy.Filename);
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that uploads an RDY file object to the terminal.
        /// </summary>
        /// <param name="rdy">The RDY file object.</param>
        /// <param name="replace">True to replace any existing table.  False to throw an exception if the table exists already. (True by default.)</param>
        /// <param name="force">True to upload the file even if it fails validation. (False by default.)</param>
        public async Task UploadTableFromRdyAsync(RdyFile rdy, bool replace = true, bool force = false)
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

            // reset the progress info
            OnProgressChanged(new UploadProgressChangedEventArgs(0, totalBlocks, rdy.Filename));

            foreach (var record in rdy.Records)
            {
                // append the record to the buffer
                buffer.Append(record.Data.PadRight(rdy.Header.RecordSize));

                // don't send the buffer until we have a full block
                if (buffer.Length < MaxBlockSize)
                    continue;

                // cut a block from the buffer and send it
                var block = buffer.Cut(0, MaxBlockSize);
                await SendBlockAsync(tableType, tableId, blockNumber, totalBlocks, block, replace, rdy.Filename);
                blockNumber++;
            }

            // send a partial block if there is any data remaining in the buffer
            if (buffer.Length > 0)
            {
                var block = buffer.ToString();
                await SendBlockAsync(tableType, tableId, blockNumber, totalBlocks, block, replace, rdy.Filename);
            }
        }
#endif

        private void SendBlock(char tableType, int tableId, int blockNumber, int totalBlocks, string block, bool replace, string filename)
        {
            var data = tableType +
                       tableId.ToString("D3") +
                       (replace ? 'R' : 'S') +
                       SynelNumericFormat.Convert(totalBlocks, 2) +
                       SynelNumericFormat.Convert(blockNumber, 2) +
                       block;

            var response = _client.SendAndReceive(RequestCommand.TableOperation, data, "t");

            ValidateSendBlockResult(response);

            OnProgressChanged(new UploadProgressChangedEventArgs(blockNumber, totalBlocks, filename));
        }

#if NET_45
        private async Task SendBlockAsync(char tableType, int tableId, int blockNumber, int totalBlocks, string block, bool replace, string filename)
        {
            var data = tableType +
                       tableId.ToString("D3") +
                       (replace ? 'R' : 'S') +
                       SynelNumericFormat.Convert(totalBlocks, 2) +
                       SynelNumericFormat.Convert(blockNumber, 2) +
                       block;

            var response = await _client.SendAndReceiveAsync(RequestCommand.TableOperation, data, "t");

            ValidateSendBlockResult(response);

            OnProgressChanged(new UploadProgressChangedEventArgs(blockNumber, totalBlocks, filename));
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

        #region FixMemCrash
        /// <summary>
        /// Erases all terminal memory, returning a terminal in the "Mem Crash" state to "No Prog".
        /// </summary>
        public void FixMemCrash()
        {
            _client.SendOnly(RequestCommand.SystemCommands, "F");
            // no ack
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that erases all terminal memory, returning a terminal in the "Mem Crash" state to "No Prog".
        /// </summary>
        public async Task FixMemCrashAsync()
        {
            await _client.SendOnlyAsync(RequestCommand.SystemCommands, "F");
            // no ack
        }
#endif
        #endregion
    }
}

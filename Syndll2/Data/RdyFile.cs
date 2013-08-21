using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syndll2.Data
{
    /// <summary>
    /// Provides an in-memory representation of an RDY formatted file.
    /// </summary>
    public class RdyFile
    {
        private RdyFile()
        {
        }

        private readonly StringBuilder _buffer = new StringBuilder();
        private IList<RdyRecord> _records;

        /// <summary>
        /// Gets the header information for the RDY file.
        /// </summary>
        public RdyHeader Header { get; private set; }

        /// <summary>
        /// Gets the name of this file if it was loaded from disk.
        /// </summary>
        public string Filename { get; internal set; }

        /// <summary>
        /// Gets a list of records in the RDY file.
        /// </summary>
        public ReadOnlyCollection<RdyRecord> Records
        {
            get { return new ReadOnlyCollection<RdyRecord>(_records); }
        }

        /// <summary>
        /// Gets a value that indicates whether or not this is a "directory" file.
        /// Directory files (such as dir001.rdy) do not get uploaded to the terminal.
        /// </summary>
        public bool IsDirectoryFile { get; private set; }

        /// <summary>
        /// Gets a dictionary representing the records.
        /// Valid for keyed records only. Returns null for non-keyed records.
        /// </summary>
        public ReadOnlyDictionary<string, string> RecordsDictionary
        {
            get
            {
                // this only makes sense when the data has keys
                if (Header.KeyLength == 0)
                    return null;

                // return the records as a dictionary
                var d = _records.OfType<KeyedRdyRecord>().ToDictionary(x => x.Key, x => x.Value);
                return new ReadOnlyDictionary<string, string>(d);
            }
        }

        /// <summary>
        /// Creates a new RDY file object.
        /// </summary>
        public static RdyFile Create(char tableType, int tableId, int recordSize, int keyLength = 0, int keyOffset = 0, bool sorted = false, bool packed = false, char tableVersion = 'A')
        {
            var header = RdyHeader.Create(tableType, tableId, recordSize, keyLength, keyOffset, sorted, packed, tableVersion);
            var rdy = new RdyFile {Header = header, _records = new List<RdyRecord>()};
            return rdy;
        }

        public void AddRecord(string value)
        {
            if (Header.KeyLength != 0)
                throw new InvalidOperationException("Use AddRecord(key, value) to add records to keyed files.");

            _records.Add(new RdyRecord(value));
            Header.IncrementRecordCount();
        }

        public void AddRecord(string key, string value)
        {
            if (Header.KeyLength == 0)
                throw new InvalidOperationException("Use AddRecord(value) to add records to non-keyed files.");
            
            _records.Add(new KeyedRdyRecord(key, value, Header.KeyLength, Header.KeyOffset, true));
            Header.IncrementRecordCount();
        }

        /// <summary>
        /// Reads an RDY file from an input stream.
        /// </summary>
        internal static RdyFile Read(Stream stream, bool force = false)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "The input stream cannot be null.");

            if (!stream.CanRead)
                throw new InvalidOperationException("Could not read from the input stream.");

            var rdy = new RdyFile();

            using (var reader = new StreamReader(stream))
            {
                // Read and parse the header
                var header = rdy.ReadHeader(reader);
                rdy.Header = RdyHeader.Parse(header, force);

                // See if we are working with a directory file
                if (rdy.Header.TableType == 'z')
                {
                    rdy.IsDirectoryFile = true;

                    // Put data back in the buffer
                    rdy._buffer.Insert(0, header.Substring(9));

                    // Read the body as directory data
                    rdy.ReadDirBody(reader);
                }
                else
                {
                    // Read and load the records in the body
                    rdy.ReadBody(reader, force);
                }
            }

            return rdy;
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that reads an RDY file from an input stream.
        /// </summary>
        internal static async Task<RdyFile> ReadAsync(Stream stream, bool force = false)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "The input stream cannot be null.");

            if (!stream.CanRead)
                throw new InvalidOperationException("Could not read from the input stream.");

            var rdy = new RdyFile();

            using (var reader = new StreamReader(stream))
            {
                // Read and parse the header
                var header = await rdy.ReadHeaderAsync(reader);
                rdy.Header = RdyHeader.Parse(header, force);

                // See if we are working with a directory file
                if (rdy.Header.TableType == 'z')
                {
                    rdy.IsDirectoryFile = true;

                    // Put data back in the buffer
                    rdy._buffer.Insert(0, header.Substring(9));

                    // Read the body as directory data
                    await rdy.ReadDirBodyAsync(reader);
                }
                else
                {
                    // Read and load the records in the body
                    await rdy.ReadBodyAsync(reader, force);
                }
            }

            return rdy;
        }
#endif

        private void ReadBody(StreamReader reader, bool force)
        {
            // Initialize a new list of records
            _records = new List<RdyRecord>(Header.RecordCount);

            // Read while there is data
            while (!reader.EndOfStream)
            {
                // read a line to the buffer
                ReadLineToBuffer(reader);

                // extract any whole records that are in the buffer
                while (_buffer.Length >= Header.RecordSize)
                {
                    // cut the record from the buffer
                    var rawValue = _buffer.Cut(0, Header.RecordSize);

                    // load the record data
                    var record = Header.KeyLength == 0
                                     ? new RdyRecord(rawValue)
                                     : new KeyedRdyRecord(rawValue, Header.KeyLength, Header.KeyOffset, force);

                    // add it to the results
                    _records.Add(record);
                }
            }

            // Make sure the record count matched the value in the header
            if (Records.Count != Header.RecordCount && !force)
                throw new InvalidDataException(string.Format("The RDY header reported {0} records, but {1} records were in the RDY file.", Header.RecordCount,
                                                             Records.Count));
        }

#if NET_45
        private async Task ReadBodyAsync(StreamReader reader, bool force)
        {
            // Initialize a new list of records
            _records = new List<RdyRecord>(Header.RecordCount);

            // Read while there is data
            while (!reader.EndOfStream)
            {
                // read a line to the buffer
                await ReadLineToBufferAsync(reader);

                // extract any whole records that are in the buffer
                while (_buffer.Length >= Header.RecordSize)
                {
                    // cut the record from the buffer
                    var rawValue = _buffer.Cut(0, Header.RecordSize);

                    // load the record data
                    var record = Header.KeyLength == 0
                                     ? new RdyRecord(rawValue)
                                     : new KeyedRdyRecord(rawValue, Header.KeyLength, Header.KeyOffset, force);

                    // add it to the results
                    _records.Add(record);
                }
            }

            // Make sure the record count matched the value in the header
            if (Records.Count != Header.RecordCount && !force)
                throw new InvalidDataException(string.Format("The RDY header reported {0} records, but {1} records were in the RDY file.", Header.RecordCount,
                                                             Records.Count));
        }
#endif

        private void ReadDirBody(StreamReader reader)
        {
            // Initialize a new list of records
            _records = new List<RdyRecord>();

            // file records are followed with a string of 16 Y characters
            var terminator = new string('Y', 16);

            // Read while there is data
            while (!reader.EndOfStream)
            {
                // read a line to the buffer
                ReadLineToBuffer(reader);

                // see if the buffer contains a terminator yet.
                var b = _buffer.ToString();
                var i = b.LastIndexOf(terminator, StringComparison.Ordinal);
                if (i == -1) continue;

                // cut the data before the terminator and write it to a new record, ignoring any trailing underscores or spaces
                var s = _buffer.Cut(0, i).TrimEnd('_', ' ');
                _records.Add(new RdyRecord(s));

                // remove the terminator from the buffer.
                _buffer.Remove(0, terminator.Length);
            }
        }

#if NET_45
        private async Task ReadDirBodyAsync(StreamReader reader)
        {
            // Initialize a new list of records
            _records = new List<RdyRecord>();

            // file records are followed with a string of 16 Y characters
            var terminator = new string('Y', 16);

            // Read while there is data
            while (!reader.EndOfStream)
            {
                // read a line to the buffer
                await ReadLineToBufferAsync(reader);

                // see if the buffer contains a terminator yet.
                var b = _buffer.ToString();
                var i = b.IndexOf(terminator, StringComparison.Ordinal);
                if (i == -1) continue;

                // cut the data before the terminator and write it to a new record, ignoring any trailing underscores or spaces
                var s = _buffer.Cut(0, i).TrimEnd('_', ' ');
                _records.Add(new RdyRecord(s));

                // remove the terminator from the buffer.
                _buffer.Remove(0, terminator.Length);
            }
        }
#endif

        private void ReadLineToBuffer(StreamReader reader)
        {
            // read a line
            var line = reader.ReadLine();

            // skip empty lines, or lines that are commented with a SOH character.
            if (string.IsNullOrEmpty(line) || line[0] == ControlChars.SOH)
                return;

            // add the data to the buffer
            _buffer.Append(line);
        }

#if NET_45
        private async Task ReadLineToBufferAsync(StreamReader reader)
        {
            // read a line
            var line = await reader.ReadLineAsync();

            // skip empty lines, or lines that are commented with a SOH character.
            if (string.IsNullOrEmpty(line) || line[0] == ControlChars.SOH)
                return;

            // add the data to the buffer
            _buffer.Append(line);
        }
#endif

        private string ReadHeader(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                // Read a line
                ReadLineToBuffer(reader);

                // Cut the header from the start of the buffer and return it.
                if (_buffer.Length >= RdyHeader.HeaderSize)
                    return _buffer.Cut(0, RdyHeader.HeaderSize);
            }

            throw new InvalidDataException(string.Format("Premature end of stream at position {0}.", _buffer.Length));
        }

#if NET_45
        private async Task<string> ReadHeaderAsync(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                // Read a line
                await ReadLineToBufferAsync(reader);

                // Cut the header from the start of the buffer and return it.
                if (_buffer.Length >= RdyHeader.HeaderSize)
                    return _buffer.Cut(0, RdyHeader.HeaderSize);
            }

            throw new InvalidDataException(string.Format("Premature end of stream at position {0}.", _buffer.Length));
        }
#endif

        public override string ToString()
        {
            var sb = new StringBuilder(Header.TotalCharacters);
            sb.AppendLine(Header.ToString());
            foreach (var record in _records)
                sb.AppendLine(record.Data.PadRight(Header.RecordSize));
            return sb.ToString();
        }


        /// <summary>
        /// Provides an in-memory representation of the header of an RDY formatted file.
        /// </summary>
        public class RdyHeader
        {
            public const int HeaderSize = 23;
            public char TableType { get; private set; }
            public int TableId { get; private set; }
            public char TableVersion { get; private set; }
            public int TotalCharacters { get; private set; }
            public int RecordSize { get; private set; }
            public int RecordCount { get; private set; }
            public int KeyLength { get; private set; }
            public int KeyOffset { get; private set; }
            public bool Sorted { get; private set; }
            public bool Packed { get; private set; }

            private RdyHeader()
            {
            }

            internal static RdyHeader Create(char tableType, int tableId, int recordSize, int keyLength, int keyOffset, bool sorted, bool packed, char tableVersion)
            {
                return new RdyHeader
                    {
                        TableType = tableType,
                        TableId = tableId,
                        TableVersion = tableVersion,
                        TotalCharacters = HeaderSize,
                        RecordSize = recordSize,
                        KeyLength = keyLength,
                        KeyOffset = keyOffset,
                        Sorted = sorted,
                        Packed = packed
                    };
            }

            internal void IncrementRecordCount()
            {
                RecordCount++;
                TotalCharacters += RecordSize;
            }

            internal static RdyHeader Parse(string data, bool force)
            {
                var header = new RdyHeader();

                if (data == null)
                    throw new ArgumentNullException("data");

                if (data.Length != HeaderSize && !force)
                    throw new ArgumentException(string.Format("The header must consist of exactly {0} characters.", HeaderSize));

                header.TableType = data[0];

                int i;
                if (!int.TryParse(data.Substring(1, 3), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the table ID from the RDY header.");
                header.TableId = i;

                if (!int.TryParse(data.Substring(4, 5), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the total number of characters from the RDY header.");
                header.TotalCharacters = i;

                if (header.TableType == 'z')
                {
                    // z indicates a "directory file", which has a truncated header.  Exit early.
                    return header;
                }

                header.TableVersion = data[9];

                if (!int.TryParse(data.Substring(10, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the header size from the RDY header.");
                if (i != HeaderSize && !force)
                    throw new InvalidDataException(string.Format("The header size in the RDY file should state {0} but it was {1} instead.", HeaderSize, i));

                if (!int.TryParse(data.Substring(12, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the record size from the RDY header.");
                header.RecordSize = i;

                // work around a known issue with v001 data (from JPR001.RDY)
                if (header.TableType == 'v' && header.TableId == 1 && header.RecordSize == 96)
                    header.RecordSize = 6;

                header.RecordCount = SynelNumericFormat.Convert(data.Substring(14, 3));

                if (!int.TryParse(data.Substring(17, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the key length from the RDY header.");
                header.KeyLength = i;

                if (!int.TryParse(data.Substring(19, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the key offset from the RDY header.");
                header.KeyOffset = i;

                if (!int.TryParse(data.Substring(21, 2), NumberStyles.None, CultureInfo.InvariantCulture, out i) && !force)
                    throw new InvalidDataException("Couldn't parse the sorted/packed flags from the RDY header.");
                header.Sorted = i == 2 || i == 3;
                header.Packed = i == 1 || i == 3;

                // Make sure the character count makes sense
                var expectedChars = HeaderSize + header.RecordCount * header.RecordSize;
                if (header.TotalCharacters != expectedChars && !force)
                    throw new InvalidDataException(string.Format("The total character count doesn't match.  It was reported as {0}, but calculated as {1}.",
                                                                 header.TotalCharacters, expectedChars));

                return header;
            }

            public override string ToString()
            {
                // work around a known issue with v001 data (from JPR001.RDY)
                var recordSize = TableType == 'v' && TableId == 1 && RecordSize == 6 ? 96 : RecordSize;

                return string.Format("{0}{1:D3}{2:D5}{3}{4:D2}{5:D2}{6}{7:D2}{8:D2}{9:D2}",
                                     TableType,
                                     TableId,
                                     TotalCharacters,
                                     TableVersion,
                                     HeaderSize,
                                     recordSize,
                                     SynelNumericFormat.Convert(RecordCount, 3),
                                     KeyLength,
                                     KeyOffset,
                                     (Packed ? 1 : 0) + (Sorted ? 2 : 0));
            }
        }

        /// <summary>
        /// Provides an in-memory representation of a record in an RDY formatted file.
        /// </summary>
        public class RdyRecord
        {
            protected readonly string _data;

            public string Data
            {
                get { return _data; }
            }

            internal RdyRecord(string data)
            {
                if (data == null)
                    throw new ArgumentNullException("data");

                _data = data;
            }
        }

        /// <summary>
        /// Provides an in-memory representation of a keyed record in an RDY formatted file.
        /// </summary>
        public class KeyedRdyRecord : RdyRecord
        {
            private readonly int _keyLength;
            private readonly int _keyOffset;

            public string Key
            {
                get { return _data.Substring(_keyOffset, _keyLength); }
            }

            public string Value
            {
                get { return _data.Substring(_keyOffset + _keyLength); }
            }

            internal KeyedRdyRecord(string key, string value, int keyLength, int keyOffset, bool force)
                : base(new string(' ', keyOffset) + key.PadRight(keyLength) + value)
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                if (value == null)
                    throw new ArgumentNullException("value");

                if (key.Length == 0 && !force)
                    throw new ArgumentException("The key cannot be empty.");

                _keyLength = keyLength;
                _keyOffset = keyOffset;
            }

            internal KeyedRdyRecord(string data, int keyLength, int keyOffset, bool force)
                : base(data)
            {
                if (keyOffset + keyLength > data.Length && !force)
                    throw new ArgumentException("The data must be at least the size of the key.");

                if (keyLength == 0 && !force)
                    throw new ArgumentException("The key cannot be empty.");

                _keyLength = keyLength;
                _keyOffset = keyOffset;
            }
        }
    }
}

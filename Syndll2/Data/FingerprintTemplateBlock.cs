using System;
using System.IO;

namespace Syndll2.Data
{
    /// <summary>
    /// Contains a block of data which is part of a fingerprint template.
    /// It may be that the whole template is in a single block, but not necessarily.
    /// </summary>
    internal class FingerprintTemplateBlock
    {
        private readonly uint _templateId;
        private readonly int _blockNumber;
        private readonly int _totalBlocks;
        private readonly int _dataSize;
        private readonly byte[] _data;

        public uint TemplateId
        {
            get { return _templateId; }
        }

        public int BlockNumber
        {
            get { return _blockNumber; }
        }

        public int TotalBlocks
        {
            get { return _totalBlocks; }
        }

        public int DataSize
        {
            get { return _dataSize; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public static FingerprintTemplateBlock Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            // Check for error response
            if (data.StartsWith("F0"))
            {
                int num;
                if (!int.TryParse(data.Substring(2, 3), out num))
                    throw new InvalidDataException("Could not parse the fingerprint error code!");

                var code = (FingerprintStatusCode)num;
                throw new FingerprintException(code);
            }

            return new FingerprintTemplateBlock(data);
        }

        /*
         Example Raw Data
         
         v0B100000000010101384d5a:a0a6h>b:e5d6`7d3i0f6c1a>d3k0d?`<a?h4k0j>h7`7d4l0f?b8a8h4n0e5h<b1`6l0d6h>a:h8a0`3a<b0`9d0j5i0`7`:i0b;h4`5`:j1h3`3a:l:m0c>m2a9l:n0c0e3a1d;a1h3`:b8d<a1c=`9a:d<b1c7h?c5`<b0i4i>a>h<f1i9h4a8h=k0c8`:`5l>c0c1h4b?`>j0i3`>b<h?a0i9h;b7`?k0i<`:b?e0e0i>c=a2m0k0c<h7b>i0n0i?e>b>i1f0k1f0o7g?o?o?o?o?o?o?g7h=n>o:j9o?o?o?h8k=n>`0`0o?o?g?h9l=n>`1a2o?o?h7h:k<m>`1b2c?o8g7i:k<m>`1b2c4o7g8i:k;m>a2c3c4d6g8i:j;l>a2c4d4d6g7i9i:k=a3d4d4d5h8h8h9j<`3d5e5e5g7h8h8h9f4e5e5e5h8h8g7h7f5e5e5e6h8h7g7g7f6e5e5e6h8g7g7g6f5e5e5f7h8g7g7f6f5e5e5f7g?g7f6f6e5d4d4e7o?h7f6f5e4d3c4f?o?h7f6e5e4c3b3h8h?h8h7f5d4c2b?o8g8h8h?o?o?o?o?o8g8h8h?o?o?o?o?o7g?o8o?o?o?o?o?o?o?`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`0`04;8>(EOT)
           01234567890123456789...
           0         1
         
           B 1 0000000001 01 01 384 ....
         
         */

        private FingerprintTemplateBlock(string s)
        {
            if (!s.StartsWith("B1"))
                throw new InvalidDataException("Expected fingerprint data to start with \"B1\".");

            if (!uint.TryParse(s.Substring(2, 10), out _templateId))
                throw new InvalidDataException("Could not parse the template id from the fingerprint data.");

            if (!int.TryParse(s.Substring(12, 2), out _blockNumber))
                throw new InvalidDataException("Could not parse the block number from the fingerprint data.");

            if (!int.TryParse(s.Substring(14, 2), out _totalBlocks))
                throw new InvalidDataException("Could not parse the total blocks from the fingerprint data.");

            if (!int.TryParse(s.Substring(16, 3), out _dataSize))
                throw new InvalidDataException("Could not parse the data size from the fingerprint data.");

            var chars = s.Substring(19);
            if (chars.Length != _dataSize * 2)
                throw new InvalidDataException("Could not parse the template from the fingerprint data.");

            _data = SynelByteFormat.Convert(chars);
        }
    }
}

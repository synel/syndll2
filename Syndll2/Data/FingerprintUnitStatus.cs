using System;
using System.Globalization;
using System.IO;

namespace Syndll2.Data
{
    public class FingerprintUnitStatus
    {
        private readonly FingerprintComparisonModes _comparisonMode;
        private readonly string _kernelVersion;
        private readonly int _loadedTemplates;
        private readonly int _maximumTemplates;
        private readonly FingerprintUnitModes _fingerprintUnitMode;
        private readonly FingerprintThreshold _globalThreshold;

        public FingerprintComparisonModes ComparisonMode
        {
            get { return _comparisonMode; }
        }

        public string KernelVersion
        {
            get { return _kernelVersion; }
        }

        public int LoadedTemplates
        {
            get { return _loadedTemplates; }
        }

        public int MaximumTemplates
        {
            get { return _maximumTemplates; }
        }

        public FingerprintUnitModes FingerprintUnitMode
        {
            get { return _fingerprintUnitMode; }
        }

        public FingerprintThreshold GlobalThreshold
        {
            get { return _globalThreshold; }
        }

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // v1M0UB15C0511110002520/09090S27809
        // CT0123456789012345678901234567CCCC
        //   0         1         2         3

        // Data Field Breakdown
        // M0 U B15C05111100 02520 / 09090 S 2

        internal static FingerprintUnitStatus Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            const int expectedLength = 27;
            if (data.Length != expectedLength)
                throw new ArgumentException(
                    string.Format(
                        "Status data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", expectedLength, data.Length),
                    "data");

            return new FingerprintUnitStatus(data);
        }

        private FingerprintUnitStatus(string data)
        {
            var subcode = data.Substring(0, 2);
            if (subcode != "M0")
                throw new InvalidDataException(
                    string.Format("Expected sub-code of \"{0}\" but got \"{1}\".", "MO", subcode));

            _comparisonMode = (FingerprintComparisonModes)data[2];

            _kernelVersion = data.Substring(3, 12);

            if (!int.TryParse(data.Substring(15, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _loadedTemplates))
                throw new InvalidDataException("Couldn't parse number of templates loaded from fingerprint status data.");

            // character 20 is a hardcoded slash that we can ignore

            if (!int.TryParse(data.Substring(21, 5), NumberStyles.None, CultureInfo.InvariantCulture, out _maximumTemplates))
                throw new InvalidDataException("Couldn't parse maximum number of templates from fingerprint status data.");

            _fingerprintUnitMode = (FingerprintUnitModes)data[26];

            byte b;
            if (!byte.TryParse(data.Substring(27, 1), NumberStyles.None, CultureInfo.InvariantCulture, out b))
                throw new InvalidDataException("Couldn't parse global threshold from fingerprint status data.");
            if (b < 1 || b > 5)
                throw new InvalidDataException("Fingerprint Global Threshold should be between 1 and 5.");
            _globalThreshold = (FingerprintThreshold)b;
        }
    }
}

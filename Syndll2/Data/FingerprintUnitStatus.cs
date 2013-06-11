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
        private readonly FingerprintEnrollModes _enrollMode;

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

        public FingerprintEnrollModes EnrollMode
        {
            get { return _enrollMode; }
        }

        // Sample Raw Data  C = command, T = terminal id, CCCC = CRC code
        // Only the Data field should be passed in.
        // v0M0SB16F0608080000004/09090S30<63>(EOT)
        // CT01234567890123456789012345678CCCC
        //   0         1         2

        // Data Field Breakdown
        // M0 S B16F06080800 00004 / 09090 S 3 0

        internal static FingerprintUnitStatus Parse(string data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Length != 29)
                throw new ArgumentException(
                    string.Format(
                        "Status data should be exactly {0} characters, and you passed {1} characters.  " +
                        "Do not pass the command, terminal id, or CRC here.", 29, data.Length),
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

            _enrollMode = (FingerprintEnrollModes) data[28];
        }
    }
}

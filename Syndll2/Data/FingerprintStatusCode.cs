using System.ComponentModel;

namespace Syndll2.Data
{
    /// <summary>
    /// Error codes returned from the fingerprint unit.
    /// </summary>
    public enum FingerprintStatusCode
    {

#pragma warning disable 1591

        [Description("Process has successfully completed.")]
        Success = 097,

        [Description("Fingerprint input has succeeded.")]
        ScanSuccess = 098,

        [Description("Sensor or fingerprint input has failed.")]
        ScanFail = 099,

        [Description("The requested data was not found.")]
        NotFound = 105,

        [Description("Fingerprint does not match.")]
        NotMatch = 106,

        [Description("Fingerprint image is not good.")]
        TryAgain = 107,

        [Description("Timeout for fingerprint input.")]
        Timeout = 108,

        [Description("Maximum template capacity exceeded.")]
        MemoryFull = 109,

        [Description("The requested user ID has been found.")]
        ExistsId = 110,

        [Description("Adding more fingerprints to a current existing user ID.")]
        AddNew = 113,

        [Description("The number of fingerprint templates per user ID has exceeded its limit.")]
        FingerLimit = 114,

        [Description("There is more data to be sent.")]
        Continue = 116,

        [Description("The command is not supported.")]
        Unsupported = 117,

        [Description("The requested user ID is invalid or missing.")]
        InvalidId = 118,

        [Description("Automatically assign user ID in enrollment.")]
        AutoId = 121,

        [Description("Timeout for matching in identification.")]
        TimeoutMatch = 122,

        [Description("Module is processing another command.")]
        Busy = 128,

        [Description("The command has been canceled.")]
        Canceled = 129,

        [Description("The checksum of a data packet is incorrect.")]
        DataError = 130,

        [Description("The checksum of a data packet is correct.")]
        DataOk = 131,

        [Description("The finger is already enrolled.")]
        ExistFinger = 132,

        [Description("A duress finger has been detected.")]
        DuressFinger = 133

#pragma warning restore 1591

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Syndll2.Data;

namespace Syndll2
{
    /// <summary>
    /// Provides methods that perform fingerprint operations on the terminal.
    /// </summary>
    public class FingerprintOperations
    {
        private readonly SynelClient _client;

        internal FingerprintOperations(SynelClient client)
        {
            _client = client;
        }

        #region GetUnitStatus
        /// <summary>
        /// Gets the fingerprint unit status from the terminal.
        /// </summary>
        public FingerprintUnitStatus GetUnitStatus()
        {
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, "M0", "vM0");
            return GetFingerprintUnitStatusResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets the fingerprint unit status from the terminal.
        /// </summary>
        public async Task<FingerprintUnitStatus> GetUnitStatusAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, "M0", "vM0");
            return GetFingerprintUnitStatusResult(response);
        }
#endif

        private static FingerprintUnitStatus GetFingerprintUnitStatusResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.LastCommand_Or_Fingerprint)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.LastCommand_Or_Fingerprint, response.Command));

            return FingerprintUnitStatus.Parse(response.Data);
        }
        #endregion

        #region TemplateExists
        /// <summary>
        /// Checks to see if a particular fingerprint template exists on the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="fingerIndex">The finger index (0-9) associated with the template.</param>
        /// <returns>True if the template exists, false otherwise.</returns>
        public bool TemplateExists(long templateId, int fingerIndex)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (fingerIndex < 0 || fingerIndex > 9)
                throw new ArgumentOutOfRangeException("fingerIndex", fingerIndex, "The finger index must be between 0 and 9");

            var data = string.Format("K{0:D1}{1:D10}", fingerIndex, templateId);
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vK");

            return response.Data[11] == '1';
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that checks to see if a particular fingerprint template exists on the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="fingerIndex">The finger index (0-9) associated with the template.</param>
        /// <returns>An awaitable task that yields true if the template exists, false otherwise.</returns>
        public async Task<bool> TemplateExistsAsync(long templateId, int fingerIndex)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (fingerIndex < 0 || fingerIndex > 9)
                throw new ArgumentOutOfRangeException("fingerIndex", fingerIndex, "The finger index must be between 0 and 9");

            var data = string.Format("A{0:D1}{1:D10}", fingerIndex, templateId);
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vB1", "vF0");

            return response.Data[11] == '1';
        }
#endif
        #endregion

        #region GetTemplate
        /// <summary>
        /// Gets a specific fingerprint template from the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="fingerIndex">The finger index (0-9) associated with the template.</param>
        /// <returns>The fingerprint template.</returns>
        public byte[] GetTemplate(long templateId, int fingerIndex)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (fingerIndex < 0 || fingerIndex > 9)
                throw new ArgumentOutOfRangeException("fingerIndex", fingerIndex, "The finger index must be between 0 and 9");

            var data = string.Format("A{0:D1}{1:D10}", fingerIndex, templateId);
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vB1", "vF0");
            var block = FingerprintTemplateBlock.Parse(response.Data);

            // most of the time there will be just one block
            if (block.TotalBlocks == 1)
                return block.Data;

            // but the spec allows for a single template to be split across multiple blocks
            var result = new List<byte>(block.DataSize * block.TotalBlocks);
            result.AddRange(block.Data);
            for (int i = 2; i <= block.TotalBlocks; i++)
            {
                data = string.Format("b0{0:D1}{1:D2}{2:D2}", templateId, block.BlockNumber, block.TotalBlocks);
                response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vB1", "vF0");
                block = FingerprintTemplateBlock.Parse(response.Data);
                result.AddRange(block.Data);
            }
            return result.ToArray();
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets a specific fingerprint template from the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="fingerIndex">The finger index (0-9) associated with the template.</param>
        /// <returns>An awaitable task that yields the fingerprint template.</returns>
        public async Task<byte[]> GetTemplateAsync(long templateId, int fingerIndex)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (fingerIndex < 0 || fingerIndex > 9)
                throw new ArgumentOutOfRangeException("fingerIndex", fingerIndex, "The finger index must be between 0 and 9");

            var data = string.Format("A{0:D1}{1:D10}", fingerIndex, templateId);
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vB1", "vF0");
            var block = FingerprintTemplateBlock.Parse(response.Data);

            // most of the time there will be just one block
            if (block.TotalBlocks == 1)
                return block.Data;

            // but the spec allows for a single template to be split across multiple blocks
            var result = new List<byte>(block.DataSize * block.TotalBlocks);
            result.AddRange(block.Data);
            for (int i = 2; i <= block.TotalBlocks; i++)
            {
                data = string.Format("b0{0:D1}{1:D2}{2:D2}", templateId, block.BlockNumber, block.TotalBlocks);
                response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vB1", "vF0");
                block = FingerprintTemplateBlock.Parse(response.Data);
                result.AddRange(block.Data);
            }
            return result.ToArray();
        }
#endif
        #endregion
    }
}

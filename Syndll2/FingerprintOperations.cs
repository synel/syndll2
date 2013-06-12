using System;
using System.Collections.Generic;
using System.Globalization;
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

        #region ListTemplates
        /// <summary>
        /// Lists the templates available on the terminal.
        /// </summary>
        /// <returns>A dictionary where the key is the template id, and the value is the count of fingerprints in the template.</returns>
        public Dictionary<long, int> ListTemplates()
        {
            const int batchSize = 50;
            var currentBatch = 1;
            var templatesRetrieved = 0;

            var data = string.Format("L0{0:D2}{1:D5}{2:D5}", batchSize, 1, 0);
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vL0", "vF0");

            // v0L00200001000020000000001000000000118:1(EOT)
            //   01234567890123

            var totalTemplates = int.Parse(response.Data.Substring(9, 5));
            var result = new Dictionary<long, int>();

            while (true)
            {
                var count = int.Parse(response.Data.Substring(2, 2));
                templatesRetrieved += count;

                for (int i = 0; i < count; i++)
                {
                    var j = i * 10 + 14;
                    var templateId = long.Parse(response.Data.Substring(j, 10));

                    if (result.ContainsKey(templateId))
                        result[templateId]++;
                    else
                        result.Add(templateId, 1);
                }

                if (templatesRetrieved >= totalTemplates)
                    return result;

                currentBatch++;
                data = string.Format("L0{0:D2}{1:D5}{2:D5}", batchSize, currentBatch, totalTemplates);
                response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vL0", "vF0");
            }
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that lists the templates available on the terminal.
        /// </summary>
        /// <returns>An awaitable task that yields a dictionary where the key is the template id, and the value is the count of fingerprints in the template.</returns>
        public async Task<Dictionary<long, int>> ListTemplatesAsync()
        {
            const int batchSize = 50;
            var currentBatch = 1;
            var templatesRetrieved = 0;

            var data = string.Format("L0{0:D2}{1:D5}{2:D5}", batchSize, 1, 0);
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vL0", "vF0");

            // v0L00200001000020000000001000000000118:1(EOT)
            //   01234567890123

            var totalTemplates = int.Parse(response.Data.Substring(9, 5));
            var result = new Dictionary<long, int>();

            while (true)
            {
                var count = int.Parse(response.Data.Substring(2, 2));
                templatesRetrieved += count;

                for (int i = 0; i < count; i++)
                {
                    var j = i * 10 + 14;
                    var templateId = long.Parse(response.Data.Substring(j, 10));

                    if (result.ContainsKey(templateId))
                        result[templateId]++;
                    else
                        result.Add(templateId, 1);
                }

                if (templatesRetrieved >= totalTemplates)
                    return result;

                currentBatch++;
                data = string.Format("L0{0:D2}{1:D5}{2:D5}", batchSize, currentBatch, totalTemplates);
                response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vL0", "vF0");
            }
        }
#endif
        #endregion

        #region PutTemplate
        /// <summary>
        /// Puts a fingerprint template onto the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="template">The fingerprint template.</param>
        public void PutTemplate(long templateId, byte[] template)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (template == null || template.Length == 0)
                throw new ArgumentException("Template data is required.", "template");

            if (template.Length != 348 && template.Length != 384)
                throw new ArgumentException("Template data should be either 348 or 384 bytes in length.", "template");


            var data = string.Format("B1{0:D10}0101{1:D3}{2}", templateId, template.Length, SynelByteFormat.Convert(template));
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, data, "vb0");

            if (response.Command != PrimaryResponseCommand.LastCommand_Or_Fingerprint || !response.Data.StartsWith("b0"))
                throw new InvalidOperationException(string.Format("Could not upload fingerprint to terminal with template id {0}", templateId));
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that puts a fingerprint template onto the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        /// <param name="template">The fingerprint template.</param>
        public async Task PutTemplateAsync(long templateId, byte[] template)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            if (template == null || template.Length == 0)
                throw new ArgumentException("Template data is required.", "template");

            if (template.Length != 348 && template.Length != 384)
                throw new ArgumentException("Template data should be either 348 or 384 bytes in length.", "template");


            var data = string.Format("B1{0:D10}0101{1:D3}{2}", templateId, template.Length, SynelByteFormat.Convert(template));
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, "vb0");

            if (response.Command != PrimaryResponseCommand.LastCommand_Or_Fingerprint || !response.Data.StartsWith("b0"))
                throw new InvalidOperationException(string.Format("Could not upload fingerprint to terminal with template id {0}", templateId));
        }
#endif
        #endregion

        #region DeleteTemplate
        /// <summary>
        /// Deletes a specific fingerprint template (all indexes) from the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        public void DeleteTemplate(long templateId)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            var data = string.Format("G0{0:D10}", templateId);
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, data, ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that deletes a specific fingerprint template (all indexes) from the terminal.
        /// </summary>
        /// <param name="templateId">The id associated with the template.</param>
        public async Task DeleteTemplateAsync(long templateId)
        {
            if (templateId < 1 || templateId > 9999999999)
                throw new ArgumentOutOfRangeException("templateId", templateId, "The template id must be between 1 and 9999999999");

            var data = string.Format("G0{0:D10}", templateId);
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }
#endif
        #endregion

        #region DeleteAllTemplates
        /// <summary>
        /// Deletes all fingerprint templates from the terminal.
        /// </summary>
        public void DeleteAllTemplates()
        {
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, "G0@@@@@@@@@@", ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that deletes all fingerprint templates from the terminal.
        /// </summary>
        public async Task DeleteAllTemplatesAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, "G0@@@@@@@@@@", ACK);
            TerminalOperations.ValidateAcknowledgment(response);
        }
#endif
        #endregion

        #region SetUnitMode
        /// <summary>
        /// Sets the fingerprint unit mode on the terminal.
        /// </summary>
        /// <param name="mode">The fingerprint unit mode.</param>
        public void SetUnitMode(FingerprintUnitModes mode)
        {
            string val;
            switch (mode)
            {
                case FingerprintUnitModes.Master:
                    val = "MASTERMODE";
                    break;
                case FingerprintUnitModes.Slave:
                    val = "SLAVE-MODE";
                    break;
                default:
                    throw new ArgumentException("Invalid FPU mode.", "mode");
            }

            var data = string.Format("H0F{0}", val);
            _client.SendAndReceive(RequestCommand.Fingerprint, data, ACK);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that sets the fingerprint unit mode on the terminal.
        /// </summary>
        /// <param name="mode">The fingerprint unit mode.</param>
        public async Task SetUnitModeAsync(FingerprintUnitModes mode)
        {
            string val;
            switch (mode)
            {
                case FingerprintUnitModes.Master:
                    val = "MASTERMODE";
                    break;
                case FingerprintUnitModes.Slave:
                    val = "SLAVE-MODE";
                    break;
                default:
                    throw new ArgumentException("Invalid FPU mode.", "mode");
            }

            var data = string.Format("H0F{0}", val);
            await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, ACK);
        }
#endif
        #endregion

        #region SetThreshold
        /// <summary>
        /// Sets the fingerprint threshold on the terminal.
        /// </summary>
        /// <param name="threshold">The fingerprint threshold.</param>
        public void SetThreshold(FingerprintThreshold threshold)
        {
            string val;
            switch (threshold)
            {
                case FingerprintThreshold.VeryHigh:
                    val = "1-VERYHIGH";
                    break;
                case FingerprintThreshold.High:
                    val = "2-HIGH----";
                    break;
                case FingerprintThreshold.Medium:
                    val = "3-MEDIUM--";
                    break;
                case FingerprintThreshold.Low:
                    val = "4-LOW-----";
                    break;
                case FingerprintThreshold.VeryLow:
                    val = "5-VERYLOW-";
                    break;
                default:
                    throw new ArgumentException("Invalid fingerprint threshold.", "threshold");
            }

            var data = string.Format("H0T{0}", val);
            _client.SendAndReceive(RequestCommand.Fingerprint, data, ACK);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that sets the fingerprint threshold on the terminal.
        /// </summary>
        /// <param name="threshold">The fingerprint threshold.</param>
        public async Task SetThresholdAsync(FingerprintThreshold threshold)
        {
            string val;
            switch (threshold)
            {
                case FingerprintThreshold.VeryHigh:
                    val = "1-VERYHIGH";
                    break;
                case FingerprintThreshold.High:
                    val = "2-HIGH----";
                    break;
                case FingerprintThreshold.Medium:
                    val = "3-MEDIUM--";
                    break;
                case FingerprintThreshold.Low:
                    val = "4-LOW-----";
                    break;
                case FingerprintThreshold.VeryLow:
                    val = "5-VERYLOW-";
                    break;
                default:
                    throw new ArgumentException("Invalid fingerprint threshold.", "threshold");
            }

            var data = string.Format("H0T{0}", val);
            await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, ACK);
        }
#endif
        #endregion

        #region SetEnrollMode
        /// <summary>
        /// Sets the fingerprint enroll mode on the terminal.
        /// </summary>
        /// <param name="mode">The fingerprint enroll mode.</param>
        public void SetEnrollMode(FingerprintEnrollModes mode)
        {
            string val;
            switch (mode)
            {
                case FingerprintEnrollModes.Once:
                    val = "0-TEMPLATE";
                    break;
                case FingerprintEnrollModes.Twice:
                    val = "1-TEMPLATE";
                    break;
                case FingerprintEnrollModes.Dual:
                    val = "A-TEMPLATE";
                    break;
                default:
                    throw new ArgumentException("Invalid fingerprint enroll mode.", "mode");
            }

            var data = string.Format("H0E{0}", val);
            _client.SendAndReceive(RequestCommand.Fingerprint, data, ACK);
        }


#if NET_45
        /// <summary>
        /// Returns an awaitable task that sets the fingerprint enroll mode on the terminal.
        /// </summary>
        /// <param name="mode">The fingerprint enroll mode.</param>
        public async Task SetEnrollModeAsync(FingerprintEnrollModes mode)
        {
            string val;
            switch (mode)
            {
                case FingerprintEnrollModes.Once:
                    val = "0-TEMPLATE";
                    break;
                case FingerprintEnrollModes.Twice:
                    val = "1-TEMPLATE";
                    break;
                case FingerprintEnrollModes.Dual:
                    val = "A-TEMPLATE";
                    break;
                default:
                    throw new ArgumentException("Invalid fingerprint enroll mode.", "mode");
            }

            var data = string.Format("H0E{0}", val);
            await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, data, ACK);
        }
#endif
        #endregion

        // ReSharper disable InconsistentNaming
        private readonly string ACK = ControlChars.ACK.ToString(CultureInfo.InvariantCulture);
        // ReSharper restore InconsistentNaming
    }
}

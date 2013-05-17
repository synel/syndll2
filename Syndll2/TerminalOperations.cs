using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Syndll2.Data;

namespace Syndll2
{
    /// <summary>
    /// Provides simple methods that perform specific actions on the terminal.
    /// </summary>
    public class TerminalOperations
    {
        private readonly SynelClient _client;

        internal TerminalOperations(SynelClient client)
        {
            _client = client;
        }

        #region GetTerminalStatus
        /// <summary>
        /// Gets the status information from the terminal.
        /// </summary>
        public TerminalStatus GetTerminalStatus()
        {
            var response = _client.SendAndReceive(RequestCommand.GetStatus);
            return GetTerminalStatusResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets the status information from the terminal.
        /// </summary>
        public async Task<TerminalStatus> GetTerminalStatusAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetStatus);
            return GetTerminalStatusResult(response);
        }

        private static TerminalStatus GetTerminalStatusResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.TerminalStatus)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.TerminalStatus, response.Command));

            return TerminalStatus.Parse(response.Data);
        }
        #endregion

        #region SetTerminalStatus
        /// <summary>
        /// Sets the terminal's clock and active function.
        /// </summary>
        public void SetTerminalStatus(DateTime clockDateTime, char activeFunction)
        {
            var request = new TimeAndFunction(clockDateTime, activeFunction);
            var response = _client.SendAndReceive(RequestCommand.SetStatus, request.ToString());
            ValidateAcknowledgment(response);

            // pause to allow time for the terminal to update internally
            Thread.Sleep(20);
        }

        /// <summary>
        /// Returns an awaitable task that sets the terminal's clock and active function.
        /// </summary>
        public async Task SetTerminalStatusAsync(DateTime clockDateTime, char activeFunction)
        {
            var request = new TimeAndFunction(clockDateTime, activeFunction);
            var response = await _client.SendAndReceiveAsync(RequestCommand.SetStatus, request.ToString());
            ValidateAcknowledgment(response);

            // pause to allow time for the terminal to update internally
            await Task.Delay(20);
        }
        #endregion

        #region SetTerminalClock
        /// <summary>
        /// Sets the terminal's clock.
        /// Does not change the currently active function.
        /// </summary>
        public void SetTerminalClock(DateTime clockDateTime)
        {
            var status = GetTerminalStatus();
            SetTerminalStatus(clockDateTime, status.ActiveFunction);
        }

        /// <summary>
        /// Returns an awaitable task that sets the terminal's clock.
        /// Does not change the currently active function.
        /// </summary>
        public async Task SetTerminalClockAsync(DateTime clockDateTime)
        {
            var status = await GetTerminalStatusAsync();
            await SetTerminalStatusAsync(clockDateTime, status.ActiveFunction);
        }
        #endregion

        #region GetTechnicianModeSettings
        /// <summary>
        /// Gets the current "technician mode" settings from the terminal.
        /// </summary>
        public TechnicianModeSettings GetTechnicianModeSettings()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HPG");
            return GetTechnicianModeSettingsResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets the current "technician mode" settings from the terminal.
        /// </summary>
        public async Task<TechnicianModeSettings> GetTechnicianModeSettingsAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HPG");
            return GetTechnicianModeSettingsResult(response);
        }

        private static TechnicianModeSettings GetTechnicianModeSettingsResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.SystemCommands)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.SystemCommands, response.Command));

            return TechnicianModeSettings.Parse(response.Data);
        }
        #endregion

        #region SetTechnicianModeSettings
        /// <summary>
        /// Sends new "technician mode" settings to the terminal.
        /// </summary>
        public void SetTechnicianModeSettings(TechnicianModeSettings settings)
        {
            var firmwareRevision = GetHardwareConfiguration().FirmwareVersion;
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, settings.ToString());
            response = SetTechnicianModeSettings_AdjustResponse(response, firmwareRevision);
            ValidateAcknowledgment(response);
        }

        /// <summary>
        /// Returns an awaitable task that sends new "technician mode" settings to the terminal.
        /// </summary>
        public async Task SetTechnicianModeSettingsAsync(TechnicianModeSettings settings)
        {
            var firmwareRevision = (await GetHardwareConfigurationAsync()).FirmwareVersion;
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, settings.ToString());
            response = SetTechnicianModeSettings_AdjustResponse(response, firmwareRevision);
            ValidateAcknowledgment(response);
        }

        private Response SetTechnicianModeSettings_AdjustResponse(Response response, int firmwareRevision)
        {
            // TODO: Update with real fixed revision number
            if (firmwareRevision >= 90000)
                return response;

            // Bug in firmware.  We always receive a NACK instead of an ACK for this command.
            if (response == Response.NotAcknowledged)
                response = Response.Acknowledged;

            return response;
        }
        #endregion

        #region GetHardwareConfiguration
        /// <summary>
        /// Gets the hardware configuration information from the terminal.
        /// </summary>
        public HardwareConfiguration GetHardwareConfiguration()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HG");
            return GetHardwareConfigurationResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets the hardware configuration information from the terminal.
        /// </summary>
        public async Task<HardwareConfiguration> GetHardwareConfigurationAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HG");
            return GetHardwareConfigurationResult(response);
        }

        private static HardwareConfiguration GetHardwareConfigurationResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.SystemCommands)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.SystemCommands, response.Command));

            return HardwareConfiguration.Parse(response.Data);
        }
        #endregion

        #region GetNetworkConfiguration
        /// <summary>
        /// Gets the network configuration information from the terminal.
        /// </summary>
        public NetworkConfiguration GetNetworkConfiguration()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HN");
            return GetNetworkConfigurationResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets the network configuration information from the terminal.
        /// </summary>
        public async Task<NetworkConfiguration> GetNetworkConfigurationAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HN");
            return GetNetworkConfigurationResult(response);
        }

        private static NetworkConfiguration GetNetworkConfigurationResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.SystemCommands)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.SystemCommands, response.Command));

            return NetworkConfiguration.Parse(response.Data);
        }
        #endregion

        #region GetFingerprintUnitStatus
        /// <summary>
        /// Gets the fingerprint unit status from the terminal.
        /// </summary>
        public FingerprintUnitStatus GetFingerprintUnitStatus()
        {
            var response = _client.SendAndReceive(RequestCommand.Fingerprint, "M0");
            return GetFingerprintUnitStatusResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets the fingerprint unit status from the terminal.
        /// </summary>
        public async Task<FingerprintUnitStatus> GetFingerprintUnitStatusAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.Fingerprint, "M0");
            return GetFingerprintUnitStatusResult(response);
        }

        private static FingerprintUnitStatus GetFingerprintUnitStatusResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.LastCommand_Or_Fingerprint)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.LastCommand_Or_Fingerprint, response.Command));

            return FingerprintUnitStatus.Parse(response.Data);
        }
        #endregion

        #region GetFullDataBlock
        /// <summary>
        /// Gets a full block of data from the terminal.
        /// Caution - it only returns *full* blocks.  If a data block is not filled out, it does not return anything.
        /// You probably want to use <see cref="GetData"/> instead.
        /// </summary>
        public string GetFullDataBlock()
        {
            var response = _client.SendAndReceive(RequestCommand.GetFullDataBlock);
            return GetFullDataBlockResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets a full block of data from the terminal.
        /// Caution - it only returns *full* blocks.  If a data block is not filled out, it does not return anything.
        /// You probably want to use <see cref="GetDataAsync"/> instead.
        /// </summary>
        public async Task<string> GetFullDataBlockAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetFullDataBlock);
            return GetFullDataBlockResult(response);
        }

        /// <summary>
        /// Gets a full block of data from the terminal, and acknowledges receipt before returning.
        /// Caution - it only returns *full* blocks.  If a data block is not filled out, it does not return anything.
        /// You probably want to use <see cref="GetDataAndAcknowledge"/> instead.
        /// </summary>
        public string GetFullDataBlockAndAcknowledge()
        {
            var data = GetFullDataBlock();
            if (data != null)
                AcknowledgeLastRecord();

            return data;
        }

        /// <summary>
        /// Returns an awaitable task that gets a full block of data from the terminal, and acknowledges receipt before returning.
        /// Caution - it only returns *full* blocks.  If a data block is not filled out, it does not return anything.
        /// You probably want to use <see cref="GetDataAndAcknowledgeAsync"/> instead.
        /// </summary>
        public async Task<string> GetFullDataBlockAndAcknowledgeAsync()
        {
            var data = await GetFullDataBlockAsync();
            if (data != null)
                await AcknowledgeLastRecordAsync();

            return data;
        }

        private static string GetFullDataBlockResult(Response response)
        {
            var validResponses = new[]
                {
                    PrimaryResponseCommand.NoData,
                    PrimaryResponseCommand.DataRecord,
                    PrimaryResponseCommand.QueryForHost
                };

            if (!validResponses.Contains(response.Command))
                throw new InvalidDataException(string.Format("Received invalid response: {0}.", response.Command));

            if (response.Command == PrimaryResponseCommand.DataRecord && response.Data == null)
                throw new InvalidDataException("Received a response indicating a data record, but no data was returned!");

            return response.Data;
        }
        #endregion

        #region GetData
        /// <summary>
        /// Gets data from the terminal.
        /// After recording the data, you should call <see cref="AcknowledgeLastRecord"/>.
        /// </summary>
        public string GetData()
        {
            var response = _client.SendAndReceive(RequestCommand.GetData);
            return GetDataResult(response);
        }

        /// <summary>
        /// Returns an awaitable task that gets data from the terminal.
        /// After recording the data, you should call <see cref="AcknowledgeLastRecordAsync"/>.
        /// </summary>
        public async Task<string> GetDataAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetData);
            return GetDataResult(response);
        }

        /// <summary>
        /// Gets data from the terminal, and acknowledges receipt before returning.
        /// </summary>
        public string GetDataAndAcknowledge()
        {
            var data = GetData();
            if (data != null)
                AcknowledgeLastRecord();

            return data;
        }

        /// <summary>
        /// Returns an awaitable task that gets data from the terminal, and acknowledges receipt before returning.
        /// </summary>
        public async Task<string> GetDataAndAcknowledgeAsync()
        {
            var data = await GetDataAsync();
            if (data != null)
                await AcknowledgeLastRecordAsync();

            return data;
        }

        private static string GetDataResult(Response response)
        {
            var validResponses = new[]
                {
                    PrimaryResponseCommand.NoData,
                    PrimaryResponseCommand.DataRecord,
                    PrimaryResponseCommand.QueryForHost
                };

            if (!validResponses.Contains(response.Command))
                throw new InvalidDataException(string.Format("Received invalid response: {0}.", response.Command));

            if (response.Command == PrimaryResponseCommand.DataRecord && response.Data == null)
                throw new InvalidDataException("Received a response indicating a data record, but no data was returned!");

            return response.Data; // todo: parse this
        }
        #endregion

        #region AcknowledgeLastRecord
        /// <summary>
        /// Acknowledges the last record sent from the terminal, so the terminal can delete it from its memory.
        /// Use after a call to <see cref="GetData"/> or <see cref="GetFullDataBlock"/>
        /// </summary>
        public void AcknowledgeLastRecord()
        {
            var response = _client.SendAndReceive(RequestCommand.AcknowledgeLastRecord);
            ValidateAcknowledgment(response);
        }

        /// <summary>
        /// Returns an awaitable task that acknowledges the last record sent from the terminal, so the terminal can delete it from its memory.
        /// Use after a call to <see cref="GetDataAsync"/> or <see cref="GetFullDataBlockAsync"/>
        /// </summary>
        public async Task AcknowledgeLastRecordAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.AcknowledgeLastRecord);
            ValidateAcknowledgment(response);
        }
        #endregion

        #region ClearBuffer
        /// <summary>
        /// Directs the terminal to clear all transmitted and acknowledged records stored in its memory buffer.
        /// </summary>
        public void ClearBuffer()
        {
            var response = _client.SendAndReceive(RequestCommand.ClearBuffer);
            ValidateAcknowledgment(response);
        }

        /// <summary>
        /// Returns an awaitable task that directs the terminal to clear all transmitted and acknowledged records stored in its memory buffer.
        /// </summary>
        public async Task ClearBufferAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.ClearBuffer);
            ValidateAcknowledgment(response);
        }
        #endregion

        #region DisplayMessage

        /// <summary>
        /// Displays a message on the terminal's screen.
        /// </summary>
        /// <param name="message">The message to display.  Must be 16 characters or shorter.</param>
        /// <param name="displaySeconds">The number of whole seconds to display the message for, or a 0 to display until the next message.</param>
        /// <param name="alignment">Indicates how to align the message in the display.  Text is centered by default.</param>
        public void DisplayMessage(string message, int displaySeconds, TextAlignment alignment = TextAlignment.Center)
        {
            var request = DisplayMessage_ValidateInput(message, displaySeconds, alignment);
            var response = _client.SendAndReceive(RequestCommand.DisplayMessage, request);
            ValidateAcknowledgment(response);
        }

        /// <summary>
        /// Returns an awaitable task that displays a message on the terminal's screen.
        /// </summary>
        /// <param name="message">The message to display.  Must be 16 characters or shorter.</param>
        /// <param name="displaySeconds">The number of whole seconds to display the message for, or a 0 to display until the next message.</param>
        /// <param name="alignment">Indicates how to align the message in the display.  Text is centered by default.</param>
        public async Task DisplayMessageAsync(string message, int displaySeconds, TextAlignment alignment = TextAlignment.Center)
        {
            var request = DisplayMessage_ValidateInput(message, displaySeconds, alignment);
            var response = await _client.SendAndReceiveAsync(RequestCommand.DisplayMessage, request);
            ValidateAcknowledgment(response);
        }

        private string DisplayMessage_ValidateInput(string message, int displaySeconds, TextAlignment alignment)
        {
            if (message == null)
                message = string.Empty;

            const int screenWidth = 16;

            if (message.Length > screenWidth)
                throw new ArgumentException(string.Format("The message cannot be larger than {0} characters.", screenWidth));

            if (displaySeconds < 0 || displaySeconds > 9)
                throw new ArgumentOutOfRangeException("displaySeconds",
                                                      displaySeconds,
                                                      "The message can not be displayed for more than 9 seconds, unless you pass a 0 to display it until the next message.");

            switch (alignment)
            {
                case TextAlignment.Left:
                    message = message.PadRight(screenWidth, ' ');
                    break;

                case TextAlignment.Center:
                    message = message.PadLeft((message.Length + screenWidth) / 2).PadRight(screenWidth);
                    break;

                case TextAlignment.Right:
                    message = message.PadLeft(screenWidth, ' ');
                    break;
            }

            return displaySeconds + message + "  ";
        }
        #endregion

        #region ValidateAcknowledgment
        private static void ValidateAcknowledgment(Response response)
        {
            if (response != Response.Acknowledged)
                throw new InvalidDataException(string.Format("Expected simple acknowledgment.  Got: {0}", response.Command));
        }
        #endregion
    }
}

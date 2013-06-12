using System;
using System.Globalization;
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

        /// <summary>
        /// Puts the terminal in programming mode, and exposes programming functions.
        /// Call this from a <code>using</code> block, or otherwise dispose of it to exit programming mode.
        /// </summary>
        public ProgrammingOperations Programming()
        {
            return new ProgrammingOperations(_client);
        }

        #region GetTerminalStatus
        /// <summary>
        /// Gets the status information from the terminal.
        /// </summary>
        public TerminalStatus GetTerminalStatus()
        {
            var response = _client.SendAndReceive(RequestCommand.GetStatus, null, "s");
            return GetTerminalStatusResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets the status information from the terminal.
        /// </summary>
        public async Task<TerminalStatus> GetTerminalStatusAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetStatus, "s");
            return GetTerminalStatusResult(response);
        }
#endif

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
            var response = _client.SendAndReceive(RequestCommand.SetStatus, request.ToString(), ACK);
            ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that sets the terminal's clock and active function.
        /// </summary>
        public async Task SetTerminalStatusAsync(DateTime clockDateTime, char activeFunction)
        {
            var request = new TimeAndFunction(clockDateTime, activeFunction);
            var response = await _client.SendAndReceiveAsync(RequestCommand.SetStatus, request.ToString(), ACK);
            ValidateAcknowledgment(response);
        }
#endif
        #endregion

        #region SetTerminalClock
        /// <summary>
        /// Sets the terminal's clock.
        /// Does not change the currently active function.
        /// </summary>
        public void SetTerminalClock(DateTime clockDateTime)
        {
            SetTerminalStatus(clockDateTime, '#');
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that sets the terminal's clock.
        /// Does not change the currently active function.
        /// </summary>
        public async Task SetTerminalClockAsync(DateTime clockDateTime)
        {
            await SetTerminalStatusAsync(clockDateTime, '#');
        }
#endif
        #endregion

        #region GetTechnicianModeSettings
        /// <summary>
        /// Gets the current "technician mode" settings from the terminal.
        /// </summary>
        public TechnicianModeSettings GetTechnicianModeSettings()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HPG", "SHPG");
            return GetTechnicianModeSettingsResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets the current "technician mode" settings from the terminal.
        /// </summary>
        public async Task<TechnicianModeSettings> GetTechnicianModeSettingsAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HPG", "SHPG");
            return GetTechnicianModeSettingsResult(response);
        }
#endif

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
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, settings.ToString(), ACK, NACK);
            response = SetTechnicianModeSettings_AdjustResponse(response, firmwareRevision);
            ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that sends new "technician mode" settings to the terminal.
        /// </summary>
        public async Task SetTechnicianModeSettingsAsync(TechnicianModeSettings settings)
        {
            var firmwareRevision = (await GetHardwareConfigurationAsync()).FirmwareVersion;
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, settings.ToString(), ACK, NACK);
            response = SetTechnicianModeSettings_AdjustResponse(response, firmwareRevision);
            ValidateAcknowledgment(response);
        }
#endif

        private Response SetTechnicianModeSettings_AdjustResponse(Response response, int firmwareRevision)
        {
            // TODO: Update with real fixed revision number
            if (firmwareRevision >= 90000)
                return response;

            // Bug in firmware.  We always receive a NACK instead of an ACK for this command.
            if (response.Command == PrimaryResponseCommand.NotAcknowledged)
                response = new Response(response.RawResponse, PrimaryResponseCommand.Acknowledged, response.Data);

            return response;
        }
        #endregion

        #region GetHardwareConfiguration
        /// <summary>
        /// Gets the hardware configuration information from the terminal.
        /// </summary>
        public HardwareConfiguration GetHardwareConfiguration()
        {
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HG", "SHG");
            return GetHardwareConfigurationResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets the hardware configuration information from the terminal.
        /// </summary>
        public async Task<HardwareConfiguration> GetHardwareConfigurationAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HG", "SHG");
            return GetHardwareConfigurationResult(response);
        }
#endif

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
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, "HN", "SHN");
            return GetNetworkConfigurationResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets the network configuration information from the terminal.
        /// </summary>
        public async Task<NetworkConfiguration> GetNetworkConfigurationAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, "HN", "SHN");
            return GetNetworkConfigurationResult(response);
        }
#endif

        private static NetworkConfiguration GetNetworkConfigurationResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.SystemCommands)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.SystemCommands, response.Command));

            return NetworkConfiguration.Parse(response.Data);
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
            var response = _client.SendAndReceive(RequestCommand.GetFullDataBlock, null, "n", "d");
            return GetFullDataBlockResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets a full block of data from the terminal.
        /// Caution - it only returns *full* blocks.  If a data block is not filled out, it does not return anything.
        /// You probably want to use <see cref="GetDataAsync"/> instead.
        /// </summary>
        public async Task<string> GetFullDataBlockAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetFullDataBlock, null, "n", "d");
            return GetFullDataBlockResult(response);
        }
#endif

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

#if NET_45
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
#endif

        private static string GetFullDataBlockResult(Response response)
        {
            var validResponses = new[]
                {
                    PrimaryResponseCommand.NoData,
                    PrimaryResponseCommand.DataRecord
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
            var response = _client.SendAndReceive(RequestCommand.GetData, null, "n", "d");
            return GetDataResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets data from the terminal.
        /// After recording the data, you should call <see cref="AcknowledgeLastRecordAsync"/>.
        /// </summary>
        public async Task<string> GetDataAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.GetData, null, "n", "d");
            return GetDataResult(response);
        }
#endif

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

#if NET_45
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
#endif

        private static string GetDataResult(Response response)
        {
            var validResponses = new[]
                {
                    PrimaryResponseCommand.NoData,
                    PrimaryResponseCommand.DataRecord
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
            var response = _client.SendAndReceive(RequestCommand.AcknowledgeLastRecord, null, ACK);
            ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that acknowledges the last record sent from the terminal, so the terminal can delete it from its memory.
        /// Use after a call to <see cref="GetDataAsync"/> or <see cref="GetFullDataBlockAsync"/>
        /// </summary>
        public async Task AcknowledgeLastRecordAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.AcknowledgeLastRecord, null, ACK);
            ValidateAcknowledgment(response);
        }
#endif
        #endregion

        #region ClearBuffer
        /// <summary>
        /// Directs the terminal to clear all transmitted and acknowledged records stored in its memory buffer.
        /// </summary>
        public void ClearBuffer()
        {
            var response = _client.SendAndReceive(RequestCommand.ClearBuffer, null, ACK);
            ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that directs the terminal to clear all transmitted and acknowledged records stored in its memory buffer.
        /// </summary>
        public async Task ClearBufferAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.ClearBuffer, null, ACK);
            ValidateAcknowledgment(response);
        }
#endif
        #endregion

        #region ResetLine
        /// <summary>
        /// Directs the terminal to terminate all transmission to the host and reinitialize the communication settings.
        /// </summary>
        public void ResetLine()
        {
            _client.SendAndReceive(RequestCommand.ResetLine);
            // no ack
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that directs the terminal to terminate all transmission to the host and reinitialize the communication settings.
        /// </summary>
        public async Task ResetLineAsync()
        {
            await _client.SendAndReceiveAsync(RequestCommand.ResetLine);
            // no ack
        }
#endif
        #endregion

        #region Halt
        /// <summary>
        /// Directs the terminal to terminate the normal operation mode and proceed to programming mode.
        /// </summary>
        public ProgrammingStatus Halt()
        {
            var response = _client.SendAndReceive(RequestCommand.Halt, null, ACK);
            ValidateAcknowledgment(response);
            var status = ProgrammingStatus.Parse(response.Data);

            // A delay here is required, or some operations will fail.
            Thread.Sleep(200);

            return status;
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that directs the terminal to terminate the normal operation mode and proceed to programming mode.
        /// </summary>
        public async Task<ProgrammingStatus> HaltAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.Halt, null, ACK);
            ValidateAcknowledgment(response);

            // A delay here is required, or some operations will fail.
            await Task.Delay(200);

            var status = ProgrammingStatus.Parse(response.Data);
            return status;
        }
#endif
        #endregion

        #region Run
        /// <summary>
        /// Directs the terminal to terminate programming mode and proceed to the normal operation mode.
        /// </summary>
        public ProgrammingStatus Run()
        {
            var response = _client.SendAndReceive(RequestCommand.Run, null, ACK);
            ValidateAcknowledgment(response);
            return ProgrammingStatus.Parse(response.Data);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that directs the terminal to terminate programming mode and proceed to the normal operation mode.
        /// </summary>
        public async Task<ProgrammingStatus> RunAsync()
        {
            var response = await _client.SendAndReceiveAsync(RequestCommand.Run, null, ACK);
            ValidateAcknowledgment(response);
            return ProgrammingStatus.Parse(response.Data);
        }
#endif
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
            var response = _client.SendAndReceive(RequestCommand.DisplayMessage, request, ACK);
            ValidateAcknowledgment(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that displays a message on the terminal's screen.
        /// </summary>
        /// <param name="message">The message to display.  Must be 16 characters or shorter.</param>
        /// <param name="displaySeconds">The number of whole seconds to display the message for, or a 0 to display until the next message.</param>
        /// <param name="alignment">Indicates how to align the message in the display.  Text is centered by default.</param>
        public async Task DisplayMessageAsync(string message, int displaySeconds, TextAlignment alignment = TextAlignment.Center)
        {
            var request = DisplayMessage_ValidateInput(message, displaySeconds, alignment);
            var response = await _client.SendAndReceiveAsync(RequestCommand.DisplayMessage, request, ACK);
            ValidateAcknowledgment(response);
        }
#endif

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

        internal static void ValidateAcknowledgment(Response response)
        {
            if (response.Command != PrimaryResponseCommand.Acknowledged)
                throw new InvalidDataException(string.Format("Expected acknowledgment.  Got: {0}  Data: {1}", response.Command, response.Data));
        }
        #endregion

        #region GetSingleRecord
        /// <summary>
        /// Gets a single record from a table, by a key.
        /// </summary>
        /// <param name="tableType">The table type.</param>
        /// <param name="tableId">The table ID.</param>
        /// <param name="key">The key</param>
        /// <returns>An object containing status information, and the record if it was found.</returns>
        public SingleRecord GetSingleRecord(char tableType, int tableId, string key)
        {
            var data = string.Format("MFS{0}{1:D3}0{2}", tableType, tableId, key);
            var response = _client.SendAndReceive(RequestCommand.SystemCommands, data, "SMFS");
            return GetSingleRecordResult(response);
        }

#if NET_45
        /// <summary>
        /// Returns an awaitable task that gets a single record from a table, by a key.
        /// </summary>
        /// <param name="tableType">The table type.</param>
        /// <param name="tableId">The table ID.</param>
        /// <param name="key">The key</param>
        /// <returns>A task that returns an object containing status information, and the record if it was found.</returns>
        public async Task<SingleRecord> GetSingleRecordAsync(char tableType, int tableId, string key)
        {
            var data = string.Format("MFS{0}{1:D3}0{2}", tableType, tableId, key);
            var response = await _client.SendAndReceiveAsync(RequestCommand.SystemCommands, data, "SMFS");
            return GetSingleRecordResult(response);
        }
#endif

        private static SingleRecord GetSingleRecordResult(Response response)
        {
            if (response.Command != PrimaryResponseCommand.SystemCommands)
                throw new InvalidDataException(string.Format("Expected response of {0} but received {1}.",
                                                             PrimaryResponseCommand.SystemCommands, response.Command));

            return SingleRecord.Parse(response.Data);
        }
        #endregion

        // ReSharper disable InconsistentNaming
        private readonly string ACK = ControlChars.ACK.ToString(CultureInfo.InvariantCulture);
        private readonly string NACK = ControlChars.NACK.ToString(CultureInfo.InvariantCulture);
        // ReSharper restore InconsistentNaming
    }
}

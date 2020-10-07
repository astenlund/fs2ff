using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using fs2ff.Models;
using fs2ff.SimConnect;

#pragma warning disable 67

namespace fs2ff
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local", Justification = "DispatcherTimer field is kept to prevent premature GC")]
    public class MainViewModel : INotifyPropertyChanged, ISimConnectMessageHandler
    {
        private readonly DataSender _dataSender;
        private readonly SimConnectAdapter _simConnect;
        private readonly IpDetectionService _ipDetectionService;
        private readonly DispatcherTimer _ipHintTimer;

        private uint _attitudeFrequency = Preferences.Default.att_freq.AdjustToBounds(AttitudeFrequencyMin, AttitudeFrequencyMax);
        private bool _autoDetectIpEnabled = Preferences.Default.ip_detection_enabled;
        private bool _dataAttitudeEnabled = Preferences.Default.att_enabled;
        private bool _dataPositionEnabled = Preferences.Default.pos_enabled;
        private bool _dataTrafficEnabled = Preferences.Default.tfk_enabled;
        private bool _errorOccurred;
        private IntPtr _hwnd = IntPtr.Zero;
        private IPAddress? _ipAddress;
        private uint _ipHintMinutesLeft = Preferences.Default.ip_hint_time;

        public MainViewModel(DataSender dataSender, SimConnectAdapter simConnect, IpDetectionService ipDetectionService)
        {
            _dataSender = dataSender;

            _simConnect = simConnect;
            _simConnect.StateChanged += SimConnectStateChanged;
            _simConnect.PositionReceived += SimConnectPositionReceived;
            _simConnect.AttitudeReceived += SimConnectAttitudeReceived;
            _simConnect.TrafficReceived += SimConnectTrafficReceived;

            _ipDetectionService = ipDetectionService;
            _ipDetectionService.NewIpDetected += IpDetectionService_NewIpDetected;

            OpenSettingsCommand = new ActionCommand(OpenSettings);
            DismissSettingsPaneCommand = new ActionCommand(DismissSettingsPane);
            GotoNewReleaseCommand = new ActionCommand(GotoReleaseNotesPage);
            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);
            ToggleSettingsPaneCommand = new ActionCommand(ToggleSettingsPane);

            _ipAddress = IPAddress.TryParse(Preferences.Default.ip_address, out var ip) ? ip : null;

            _ipHintTimer = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.Normal, IpHintCallback, Dispatcher.CurrentDispatcher);

            CheckForUpdates();
            UpdateVisualState();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Unknown is not supposed to be used")]
        private enum FlightSimState
        {
            Unknown,
            Connected,
            Disconnected,
            ErrorOccurred
        }

        public static string WindowTitle => $"fs2ff - {App.InformationalVersion}";

        public uint AttitudeFrequency
        {
            get => _attitudeFrequency;
            set
            {
                _attitudeFrequency = value.AdjustToBounds(AttitudeFrequencyMin, AttitudeFrequencyMax);
                _simConnect.SetAttitudeFrequency(_attitudeFrequency);
                Preferences.Default.att_freq = value;
                Preferences.Default.Save();
            }
        }

        public static uint AttitudeFrequencyMax => 10;

        public static uint AttitudeFrequencyMin => 4;

        public bool AutoDetectIpEnabled
        {
            get => _autoDetectIpEnabled;
            set
            {
                if (value != _autoDetectIpEnabled)
                {
                    _autoDetectIpEnabled = value;
                    Preferences.Default.ip_detection_enabled = value;
                    Preferences.Default.Save();
                }
            }
        }

        public string? ConnectButtonText { get; private set; }

        public bool ConnectedLabelVisible { get; private set; }

        public bool DataAttitudeEnabled
        {
            get => _dataAttitudeEnabled;
            set
            {
                if (value != _dataAttitudeEnabled)
                {
                    _dataAttitudeEnabled = value;
                    Preferences.Default.att_enabled = value;
                    Preferences.Default.Save();
                }
            }
        }

        public bool DataPositionEnabled
        {
            get => _dataPositionEnabled;
            set
            {
                if (value != _dataPositionEnabled)
                {
                    _dataPositionEnabled = value;
                    Preferences.Default.pos_enabled = value;
                    Preferences.Default.Save();
                }
            }
        }

        public bool DataTrafficEnabled
        {
            get => _dataTrafficEnabled;
            set
            {
                if (value != _dataTrafficEnabled)
                {
                    _dataTrafficEnabled = value;
                    Preferences.Default.tfk_enabled = value;
                    Preferences.Default.Save();
                }
            }
        }

        public ICommand DismissSettingsPaneCommand { get; }

        public bool ErrorLabelVisible { get; private set; }

        public ICommand GotoNewReleaseCommand { get; }

        public bool IndicatorVisible { get; private set; }

        public IPAddress? IpAddress
        {
            get => _ipAddress;
            set
            {
                if (!Equals(value, _ipAddress))
                {
                    _ipAddress = value;
                    Preferences.Default.ip_address = value?.ToString() ?? "";
                    Preferences.Default.Save();

                    if (value != null)
                    {
                        ResetIpHintMinutesLeft();
                    }

                    ResetDataSenderConnection();
                }
            }
        }

        public bool IpHintVisible => IpAddress == null && IpHintMinutesLeft == 0;

        public bool NotLabelVisible { get; private set; }

        public ICommand OpenSettingsCommand { get; }

        public bool SettingsPaneVisible { get; set; }

        public ActionCommand ToggleConnectCommand { get; }

        public ICommand ToggleSettingsPaneCommand { get; }

        public bool UpdateMsgVisible => UpdateInfo != null && !SettingsPaneVisible;

        public IntPtr WindowHandle
        {
            get => _hwnd;
            set
            {
                _hwnd = value;
                ToggleConnectCommand.TriggerCanExecuteChanged();
            }
        }

        private FlightSimState CurrentFlightSimState =>
            _errorOccurred
                ? FlightSimState.ErrorOccurred
                : _simConnect.Connected
                    ? FlightSimState.Connected
                    : FlightSimState.Disconnected;

        private uint IpHintMinutesLeft
        {
            get => _ipHintMinutesLeft;
            set
            {
                if (value != _ipHintMinutesLeft)
                {
                    _ipHintMinutesLeft = value;
                    Preferences.Default.ip_hint_time = value;
                    Preferences.Default.Save();
                }
            }
        }

        private UpdateInformation? UpdateInfo { get; set; }

        public void Dispose()
        {
            _ipDetectionService.NewIpDetected -= IpDetectionService_NewIpDetected;

            _simConnect.TrafficReceived -= SimConnectTrafficReceived;
            _simConnect.AttitudeReceived -= SimConnectAttitudeReceived;
            _simConnect.PositionReceived -= SimConnectPositionReceived;
            _simConnect.StateChanged -= SimConnectStateChanged;
            _simConnect.Dispose();

            _dataSender.Dispose();
        }

        public void ReceiveFlightSimMessage() => _simConnect.ReceiveMessage();

        private bool CanConnect() => WindowHandle != IntPtr.Zero;

        private void CheckForUpdates()
        {
            UpdateChecker.Check().ContinueWith(task => UpdateInfo = task.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Connect() => _simConnect.Connect(WindowHandle, AttitudeFrequency);

        private void Disconnect() => _simConnect.Disconnect();

        private void DismissSettingsPane() => SettingsPaneVisible = false;

        private async Task SimConnectAttitudeReceived(Attitude att)
        {
            if (DataAttitudeEnabled)
            {
                await _dataSender.Send(att).ConfigureAwait(false);
            }
        }

        private async Task SimConnectPositionReceived(Position pos)
        {
            if (DataPositionEnabled)
            {
                await _dataSender.Send(pos).ConfigureAwait(false);
            }
        }

        private void SimConnectStateChanged(bool failure)
        {
            _errorOccurred = failure;

            ResetDataSenderConnection();
            UpdateVisualState();
        }

        private async Task SimConnectTrafficReceived(Traffic tfk, uint id)
        {
            if (DataTrafficEnabled)
            {
                await _dataSender.Send(tfk, id).ConfigureAwait(false);
            }
        }

        private void GotoReleaseNotesPage()
        {
            if (UpdateInfo != null)
            {
                Process.Start("explorer.exe", UpdateInfo.DownloadLink.ToString());
            }
        }

        private void IpDetectionService_NewIpDetected(IPAddress ip)
        {
            if (AutoDetectIpEnabled)
            {
                IpAddress = ip;
            }
        }

        private void IpHintCallback(object? sender, EventArgs e)
        {
            if (IpHintMinutesLeft > 0 && IpAddress == null && _simConnect.Connected)
            {
                IpHintMinutesLeft--;
            }
        }

        private void OpenSettings() => SettingsPaneVisible = true;

        private void ResetDataSenderConnection()
        {
            if (CurrentFlightSimState == FlightSimState.Connected)
            {
                _dataSender.Connect(IpAddress);
            }
            else
            {
                _dataSender.Disconnect();
            }
        }

        private void ResetIpHintMinutesLeft()
        {
            Preferences.Default.PropertyValues["ip_hint_time"].SerializedValue = Preferences.Default.Properties["ip_hint_time"].DefaultValue;
            Preferences.Default.PropertyValues["ip_hint_time"].Deserialized = false;
            IpHintMinutesLeft = Preferences.Default.ip_hint_time;
        }

        private void ToggleConnect()
        {
            if (_simConnect.Connected) Disconnect();
            else                         Connect();
        }

        private void ToggleSettingsPane() => SettingsPaneVisible = !SettingsPaneVisible;

        private void UpdateVisualState()
        {
            (IndicatorVisible, NotLabelVisible, ErrorLabelVisible, ConnectedLabelVisible, ConnectButtonText) = CurrentFlightSimState switch
            {
                FlightSimState.Connected     => (true, false, false, true, "Disconnect"),
                FlightSimState.Disconnected  => (false, true, false, true, "Connect"),
                FlightSimState.ErrorOccurred => (false, false, true, false, "Connect"),
                _                            => (false, true, false, true, "Connect")
            };
        }
    }
}

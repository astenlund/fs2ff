using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using fs2ff.FlightSim;
using fs2ff.ForeFlight;
using fs2ff.Models;

#pragma warning disable 67

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IFlightSimMessageHandler
    {
        private readonly FlightSimService _flightSim;
        private readonly ForeFlightService _foreFlight;

        private bool _broadcastEnabled = Preferences.Default.broadcast_enabled;
        private bool _dataAttitudeEnabled = Preferences.Default.att_enabled;
        private bool _dataPositionEnabled = Preferences.Default.pos_enabled;
        private bool _dataTrafficEnabled = Preferences.Default.tfk_enabled;
        private bool _errorOccurred;
        private IntPtr _hwnd = IntPtr.Zero;
        private IPAddress? _ipAddress;

        public MainViewModel(ForeFlightService foreFlight, FlightSimService flightSim)
        {
            _foreFlight = foreFlight;
            _flightSim = flightSim;
            _flightSim.StateChanged += FlightSim_StateChanged;
            _flightSim.PositionReceived += FlightSim_PositionReceived;
            _flightSim.AttitudeReceived += FlightSim_AttitudeReceived;
            _flightSim.TrafficReceived += FlightSim_TrafficReceived;

            AcknowledgeBroadcastHintCommand = new ActionCommand(AcknowledgeBroadcastHint);
            DismissSettingsPaneCommand = new ActionCommand(DismissSettingsPane);
            GotoNewReleaseCommand = new ActionCommand(GotoReleaseNotesPage);
            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);

            _ipAddress = IPAddress.TryParse(Preferences.Default.ip_address, out var ip) ? ip : null;

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

        public static string WindowTitle => $"fs2ff (Flight Simulator -> ForeFlight) {App.InformationalVersion}";

        public ICommand AcknowledgeBroadcastHintCommand { get; }

        public bool BroadcastEnabled
        {
            get => _broadcastEnabled;
            set
            {
                if (value != _broadcastEnabled)
                {
                    _broadcastEnabled = value;
                    Preferences.Default.broadcast_enabled = value;
                    Preferences.Default.Save();
                    UpdateForeFlightConnection();
                }
            }
        }

        public bool BroadcastHintVisible => PrefBroadcastHint && (BroadcastEnabled || IpAddress == null);

        public string? ConnectButtonLabel { get; set; }

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

        public ICommand GotoNewReleaseCommand { get; }

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
                    UpdateForeFlightConnection();
                }
            }
        }

        public bool SettingsPaneVisible { get; set; }

        public Brush? StateLabelColor { get; set; }

        public string? StateLabelText { get; set; }

        public ActionCommand ToggleConnectCommand { get; }

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

        private bool Connected => _flightSim.Connected;

        private FlightSimState CurrentFlightSimState =>
            _errorOccurred
                ? FlightSimState.ErrorOccurred
                : Connected
                    ? FlightSimState.Connected
                    : FlightSimState.Disconnected;

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local", Justification = "PropertyChanged.Fody needs this to be non-static")]
        private bool PrefBroadcastHint
        {
            get => Preferences.Default.broadcast_hint;
            set
            {
                Preferences.Default.broadcast_hint = value;
                Preferences.Default.Save();
            }
        }

        private UpdateInformation? UpdateInfo { get; set; }

        public void Dispose()
        {
            _flightSim.TrafficReceived -= FlightSim_TrafficReceived;
            _flightSim.AttitudeReceived -= FlightSim_AttitudeReceived;
            _flightSim.PositionReceived -= FlightSim_PositionReceived;
            _flightSim.StateChanged -= FlightSim_StateChanged;
            _flightSim.Dispose();
            _foreFlight.Dispose();
        }

        public void ReceiveFlightSimMessage() => _flightSim.ReceiveMessage();

        private void AcknowledgeBroadcastHint() => PrefBroadcastHint = false;

        private bool CanConnect() => WindowHandle != IntPtr.Zero;

        private void CheckForUpdates()
        {
            UpdateChecker.Check().ContinueWith(task => UpdateInfo = task.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Connect() => _flightSim.Connect(WindowHandle);

        private void Disconnect() => _flightSim.Disconnect();

        private void DismissSettingsPane() => SettingsPaneVisible = false;

        private async Task FlightSim_AttitudeReceived(Attitude att)
        {
            if (DataAttitudeEnabled)
                await _foreFlight.Send(att).ConfigureAwait(false);
        }

        private async Task FlightSim_PositionReceived(Position pos)
        {
            if (DataPositionEnabled)
                await _foreFlight.Send(pos).ConfigureAwait(false);
        }

        private void FlightSim_StateChanged(bool failure)
        {
            _errorOccurred = failure;

            UpdateForeFlightConnection();
            UpdateVisualState();
        }

        private async Task FlightSim_TrafficReceived(Traffic tfk, uint id)
        {
            if (DataTrafficEnabled)
                await _foreFlight.Send(tfk, id).ConfigureAwait(false);
        }

        private void GotoReleaseNotesPage()
        {
            if (UpdateInfo != null)
            {
                Process.Start("explorer.exe", UpdateInfo.DownloadLink.ToString());
            }
        }

        private void ToggleConnect()
        {
            if (Connected) Disconnect();
            else              Connect();
        }

        private void UpdateForeFlightConnection()
        {
            if (CurrentFlightSimState == FlightSimState.Connected)
                _foreFlight.Connect(BroadcastEnabled ? IPAddress.Broadcast : IpAddress);
            else
                _foreFlight.Disconnect();
        }

        private void UpdateVisualState()
        {
            (ConnectButtonLabel, StateLabelColor, StateLabelText) = CurrentFlightSimState switch
            {
                FlightSimState.Connected      => ("Disconnect", Brushes.Goldenrod, "CONNECTED"),
                FlightSimState.Disconnected   => ("Connect", Brushes.DarkGray, "NOT CONNECTED"),
                FlightSimState.ErrorOccurred  => ("Connect", Brushes.OrangeRed, "UNABLE TO CONNECT"),
                _                             => ("Connect", Brushes.DarkGray, "")
            };
        }
    }
}

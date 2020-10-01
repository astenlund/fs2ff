using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using fs2ff.FlightSim;
using fs2ff.Models;

#pragma warning disable 67

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IFlightSimMessageHandler
    {
        private readonly FlightSimAdapter _flightSim;
        private readonly NetworkAdapter _network;

        private bool _broadcastEnabled = Preferences.Default.broadcast_enabled;
        private bool _dataAttitudeEnabled = Preferences.Default.att_enabled;
        private bool _dataPositionEnabled = Preferences.Default.pos_enabled;
        private bool _dataTrafficEnabled = Preferences.Default.tfk_enabled;
        private bool _errorOccurred;
        private IntPtr _hwnd = IntPtr.Zero;
        private IPAddress? _ipAddress;

        public MainViewModel(NetworkAdapter network, FlightSimAdapter flightSim)
        {
            _network = network;
            _flightSim = flightSim;
            _flightSim.StateChanged += FlightSim_StateChanged;
            _flightSim.PositionReceived += FlightSim_PositionReceived;
            _flightSim.AttitudeReceived += FlightSim_AttitudeReceived;
            _flightSim.TrafficReceived += FlightSim_TrafficReceived;

            AcknowledgeBroadcastHintCommand = new ActionCommand(AcknowledgeBroadcastHint);
            DismissSettingsPaneCommand = new ActionCommand(DismissSettingsPane);
            GotoNewReleaseCommand = new ActionCommand(GotoReleaseNotesPage);
            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);
            ToggleSettingsPaneCommand = new ActionCommand(ToggleSettingsPane);

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

        public static string WindowTitle => $"fs2ff - {App.InformationalVersion}";

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
                    ResetNetwork();
                }
            }
        }

        public bool BroadcastHintVisible => !PrefSuppressBroadcastHint && (BroadcastEnabled || IpAddress == null);

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
                    ResetNetwork();
                }
            }
        }

        public bool NotLabelVisible { get; private set; }

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
                : _flightSim.Connected
                    ? FlightSimState.Connected
                    : FlightSimState.Disconnected;

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local", Justification = "PropertyChanged.Fody needs this to be non-static")]
        private bool PrefSuppressBroadcastHint
        {
            get => Preferences.Default.suppress_broadcast_hint;
            set
            {
                Preferences.Default.suppress_broadcast_hint = value;
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
            _network.Dispose();
        }

        public void ReceiveFlightSimMessage() => _flightSim.ReceiveMessage();

        private void AcknowledgeBroadcastHint() => PrefSuppressBroadcastHint = true;

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
            {
                await _network.Send(att).ConfigureAwait(false);
            }
        }

        private async Task FlightSim_PositionReceived(Position pos)
        {
            if (DataPositionEnabled)
            {
                await _network.Send(pos).ConfigureAwait(false);
            }
        }

        private void FlightSim_StateChanged(bool failure)
        {
            _errorOccurred = failure;

            ResetNetwork();
            UpdateVisualState();
        }

        private async Task FlightSim_TrafficReceived(Traffic tfk, uint id)
        {
            if (DataTrafficEnabled)
            {
                await _network.Send(tfk, id).ConfigureAwait(false);
            }
        }

        private void GotoReleaseNotesPage()
        {
            if (UpdateInfo != null)
            {
                Process.Start("explorer.exe", UpdateInfo.DownloadLink.ToString());
            }
        }

        private void ResetNetwork()
        {
            if (CurrentFlightSimState == FlightSimState.Connected)
            {
                _network.Connect(BroadcastEnabled ? IPAddress.Broadcast : IpAddress);
            }
            else
            {
                _network.Disconnect();
            }
        }

        private void ToggleConnect()
        {
            if (_flightSim.Connected) Disconnect();
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

using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using fs2ff.FlightSim;
using fs2ff.ForeFlight;
using fs2ff.Models;

#pragma warning disable 67

// ReSharper disable UnusedMember.Local

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IFlightSimMessageHandler
    {
        private readonly FlightSimService _flightSim;
        private readonly ForeFlightService _foreFlight;

        private bool _errorOccurred;
        private IntPtr _hwnd = IntPtr.Zero;
        private IPAddress? _ipAddress;
        private bool _broadcastEnabled = true;

        public MainViewModel(ForeFlightService foreFlight, FlightSimService flightSim)
        {
            _foreFlight = foreFlight;
            _flightSim = flightSim;
            _flightSim.StateChanged += FlightSim_StateChanged;
            _flightSim.PositionReceived += FlightSim_PositionReceived;
            _flightSim.AttitudeReceived += FlightSim_AttitudeReceived;
            _flightSim.TrafficReceived += FlightSim_TrafficReceived;

            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);
            DismissSettingsPaneCommand = new ActionCommand(DismissSettingsPane);

            UpdateVisualState();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private enum FlightSimState
        {
            Unknown,
            Connected,
            Disconnected,
            ErrorOccurred
        }

        public static string WindowTitle => $"fs2ff (Flight Simulator -> ForeFlight) {App.AssemblyVersion}";

        public bool BroadcastEnabled
        {
            get => _broadcastEnabled;
            set
            {
                if (value != _broadcastEnabled)
                {
                    _broadcastEnabled = value;
                    UpdateForeFlightConnection();
                }
            }
        }

        public string? ConnectButtonLabel { get; set; }

        public bool DataAttitudeEnabled { get; set; } = true;

        public bool DataPositionEnabled { get; set; } = true;

        public bool DataTrafficEnabled { get; set; } = true;

        public ICommand DismissSettingsPaneCommand { get; }

        public IPAddress? IpAddress
        {
            get => _ipAddress;
            set
            {
                if (value != null && !value.Equals(_ipAddress))
                {
                    _ipAddress = value;
                    UpdateForeFlightConnection();
                }
            }
        }

        public bool SettingsPaneVisible { get; set; }

        public Brush? StateLabelColor { get; set; }

        public string? StateLabelText { get; set; }

        public ActionCommand ToggleConnectCommand { get; }

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

        private bool CanConnect() => WindowHandle != IntPtr.Zero;

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
            {
                await _foreFlight.Send(tfk, id).ConfigureAwait(false);
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

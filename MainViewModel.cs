// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Local

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using fs2ff.Annotations;
using fs2ff.FlightSim;

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly FlightSimService _flightSim;

        private bool _errorOccurred = false;

        public MainViewModel(FlightSimService flightSim)
        {
            _flightSim = flightSim;

            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private enum FlightSimState
        {
            Unknown,
            Connected,
            Disconnected,
            ErrorOccurred
        }

        public string ConnectButtonLabel => Connected ? "Disconnect" : "Connect";

        public Brush StateLabelColor =>
            CurrentFlightSimState switch
            {
                FlightSimState.Connected      => Brushes.Gold,
                FlightSimState.Disconnected   => Brushes.DarkGray,
                FlightSimState.ErrorOccurred  => Brushes.OrangeRed,
                _                             => Brushes.DarkGray
            };

        public string StateLabelText =>
            CurrentFlightSimState switch
            {
                FlightSimState.Connected      => "CONNECTED",
                FlightSimState.Disconnected   => "NOT CONNECTED",
                FlightSimState.ErrorOccurred  => "Unable to connect to Flight Simulator",
                _                             => ""
            };

        public ICommand ToggleConnectCommand { get; }

        private bool Connected => false; // TODO: Return Flight Simulator connection status

        private FlightSimState CurrentFlightSimState =>
            _errorOccurred
                ? FlightSimState.ErrorOccurred
                : Connected
                    ? FlightSimState.Connected
                    : FlightSimState.Disconnected;

        public void Dispose()
        {
            _flightSim.Dispose();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanConnect() => false; // TODO: Implement connectability check

        private void Connect() { /* TODO: Connect to Flight Simulator */ }

        private void Disconnect() { /* TODO: Disconnect from Flight Simulator */ }

        private void ToggleConnect()
        {
            if (Connected) Disconnect();
            else              Connect();
        }
    }
}

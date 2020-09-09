// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace fs2ff.FlightSim
{
    public class FlightSimService : IDisposable
    {
        private const string AppName = "fs2ff";
        private const uint WM_USER_SIMCONNECT = 0x0402;

        private SimConnect? _simConnect;

        public event Action<bool>? StateChanged;

        public bool Connected => _simConnect != null;

        public void Connect(IntPtr hwnd)
        {
            try
            {
                _simConnect = new SimConnect(AppName, hwnd, WM_USER_SIMCONNECT, null, 0);
                StateChanged?.Invoke(false);
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
                StateChanged?.Invoke(true);
            }
        }

        public void Disconnect() => DisconnectInternal(false);

        public void Dispose() => DisconnectInternal(false);

        public void ReceiveMessage()
        {
            try
            {
                _simConnect?.ReceiveMessage();
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
                DisconnectInternal(true);
            }
        }

        private void DisconnectInternal(bool failure)
        {
            _simConnect?.Dispose();
            _simConnect = null;

            StateChanged?.Invoke(failure);
        }
    }
}

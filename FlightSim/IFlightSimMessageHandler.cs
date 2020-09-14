using System;

namespace fs2ff.FlightSim
{
    public interface IFlightSimMessageHandler : IDisposable
    {
        public void ReceiveFlightSimMessage();
        public IntPtr WindowHandle { set; }
    }
}

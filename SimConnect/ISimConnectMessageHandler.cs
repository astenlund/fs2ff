using System;

namespace fs2ff.SimConnect
{
    public interface ISimConnectMessageHandler : IDisposable
    {
        public void ReceiveFlightSimMessage();
        public IntPtr WindowHandle { set; }
    }
}

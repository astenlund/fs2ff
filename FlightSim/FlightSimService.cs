using System;
using Microsoft.FlightSimulator.SimConnect;

namespace fs2ff.FlightSim
{
    public class FlightSimService : IDisposable
    {
        private SimConnect? _simConnect;

        public void Dispose()
        {
            _simConnect?.Dispose();
        }
    }
}

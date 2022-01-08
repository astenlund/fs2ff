// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace fs2ff.SimConnect
{
    public enum REQUEST : uint
    {
        Undefined,
        Position,
        Attitude,
        TrafficAircraft,
        TrafficHelicopter,
        Owner,
        Airport,
        Weather = 10,
        TrafficObjectBase = 0x00100000
    }
}

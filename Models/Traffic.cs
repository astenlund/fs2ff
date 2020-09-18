// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Runtime.InteropServices;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Traffic
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double VerticalSpeed;
        public bool OnGround;
        public double TrueHeading;
        public double GroundVelocity;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string TailNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Airline;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string FlightNumber;
    }
}

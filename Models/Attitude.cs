// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Runtime.InteropServices;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Attitude
    {
        public double Pitch;
        public double Bank;
        public double TrueHeading;
        public double SkidSlip;
        public double TurnRate;
        public double AirspeedIndicated;
        public double AirspeedTrue;
        public double PressureAlt;
        public double VertSpeed;
        public double GForce;
    }
}

// ReSharper disable FieldCanBeMadeReadOnly.Global

using System;
using System.Runtime.InteropServices;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Position
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double GroundTrack;
        public double GroundSpeed;
    }
}

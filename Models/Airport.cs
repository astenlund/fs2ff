using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Airport
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string Icao;
        public double Latitude;
        public double Longitude;
        public double Altitude;
    }
}

// ReSharper disable FieldCanBeMadeReadOnly.Global

using System;
using System.Runtime.InteropServices;

namespace fs2ff.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Traffic
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double PressureAlt;
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

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Category;

        public double MaxGrossWeight;
        public double AirspeedIndicated;
        public double AirspeedTrue;
        public uint TransponderCode;
        public TranssponderState TransponderState;
    }

    public static class TrafficExtensions
    {
        public static bool IsValid(this Traffic t)
        {
            return t.Latitude != 0 && t.Longitude != 0 && t.MaxGrossWeight > 1;
        }

        public static double DistanceTo(this Traffic self, Traffic target, bool useAltitude = false)
        {
            var baseRad = Math.PI * self.Latitude / 180;
            var targetRad = Math.PI * target.Latitude / 180;
            var theta = self.Longitude - target.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist = Math.Sin(baseRad) * Math.Sin(targetRad) + (Math.Cos(baseRad) * Math.Cos(targetRad) * Math.Cos(thetaRad));
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            // Convert to meters
            dist = dist * (60 * 1853.159616);
            double altDist = useAltitude ? Math.Abs(self.Altitude.FeetToMeters() - target.Altitude.FeetToMeters()) : 0;

            return dist + altDist;
        }

        /// <summary>
        /// Is the traffic close enough to worry about. 
        /// </summary>
        /// <param name="self">Self location</param>
        /// <param name="target">Target location</param>
        /// <returns>true if close enough to alert</returns>
        public static bool IsAlertable(this Traffic self, Traffic target)
        {
            bool alert = self.IsValid() && target.IsValid();

            if (alert)
            {
                alert = self.DistanceTo(target).MetersToMiles() < 2 && Math.Abs(self.Altitude - target.Altitude) < 2000;
            }

            return alert;
        }
    }

    public enum TranssponderState :int
    {
        Off,
        Standby,
        Test,
        On,
        Alt,
        Ground
    }
}

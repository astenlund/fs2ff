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

    public static class PositionExtensions
    {
        /// <summary>
        /// Returns relative distance to a given position
        /// Currently not used could be used for Prioritizing traffic etc.
        /// </summary>
        /// <param name="baseCoordinates"></param>
        /// <param name="targetCoordinates"></param>
        /// <returns>Distance from other position</returns>
        public static double DistanceTo(this Position baseCoordinates, Position targetCoordinates)
        {
            var baseRad = Math.PI * baseCoordinates.Latitude / 180;
            var targetRad = Math.PI * targetCoordinates.Latitude / 180;
            var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            //meters
            dist = dist * (60 * 1853.159616);
            double altDist = Math.Abs(baseCoordinates.Altitude - targetCoordinates.Altitude);


            return dist + altDist;
        }
    

        public static double DistanceTo(this Position baseCoordinates, double lat, double lon)
        {
            var targetPos = new Position
            {
                Latitude = lat,
                Longitude = lon
            };
            return baseCoordinates.DistanceTo(targetPos);
        }
    }
}

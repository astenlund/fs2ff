using System;
using GeoidHeightsDotNet;

namespace fs2ff.Models
{
    public class Gdl90GeoAltitude :Gdl90Base
    {
        /// <summary>
        /// GDL-90 Ownership Geometric Altitude (1Hz) 5 bytes.
        /// Used by the AHRS Altitude bar
        /// </summary>
        /// <param name="pos"></param>
        public Gdl90GeoAltitude(Position pos) : base(5)
        {
            Msg[0] = 0x0B; // Geo Alt report

            // Bytes 1-2 Height above WGS84 Ellipsoid LSB = 5ft
            // Using GeoidHeightsDotNet to calculate the height correction for EGM2008
            var h = GeoidHeights.undulation(pos.Latitude, pos.Longitude).MetersToFeet();
            var encodedAlt = Convert.ToInt16((pos.Altitude + h).RoundBy(5).AdjustToBounds(-1000, short.MaxValue) / 5);
            Msg[1] = (byte)(encodedAlt >> 8);     // Altitude.
            Msg[2] = (byte)(encodedAlt & 0x00FF); // Altitude.

            // No Vertical Warning, VFOM = 10 meters
            Msg[3] = 0x00;
            Msg[4] = 0x0A;
        }
    }
}

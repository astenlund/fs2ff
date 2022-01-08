using System;

namespace fs2ff.Models
{
    public class Gdl90FfmAhrs : Gdl90Base
    {
        /// <summary>
        /// ForeFlights implementation of GDL90 AHRS 10hz 12 bytes
        /// Unfortunately Garmin Pilot is also using this limited data instead of GDL90Ahrs.
        /// GP is working on using a more generic GDL90 implementation but until then
        /// </summary>
        public Gdl90FfmAhrs(Attitude att) : base(12)
        {
            Msg[0] = 0x65; // Message type "ForeFlight".
            Msg[1] = 0x01; // AHRS message identifier.

            // pitch, roll, heading have an LSB = 0.1
            var pitch = Convert.ToInt16(att.Pitch * -10);
            var roll = Convert.ToInt16(att.Bank * -10);
            var hdg = Convert.ToInt16(att.TrueHeading * 10);

            var ias = Convert.ToInt16(att.AirspeedIndicated);
            var tas = Convert.ToInt16(att.AirspeedTrue);

            Msg[2] = (byte)((roll >> 8) & 0xFF);
            Msg[3] = (byte)(roll & 0xFF);

            // Pitch.
            Msg[4] = (byte)((pitch >> 8) & 0xFF);
            Msg[5] = (byte)(pitch & 0xFF);

            // Heading.
            Msg[6] = (byte)((hdg >> 8) & 0xFF);
            Msg[7] = (byte)(hdg & 0xFF);

            // Indicated Airspeed.
            Msg[8] = (byte)((ias >> 8) & 0xFF);
            Msg[9] = (byte)(ias & 0xFF);

            // True Airspeed.
            Msg[10] = (byte)((tas & 0xFF0) >> 4);
            Msg[11] = (byte)((tas & 0x00F) << 4);
        }
    }
}

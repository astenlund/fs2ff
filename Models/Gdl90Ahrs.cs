using System;

namespace fs2ff.Models
{
    public class Gdl90Ahrs : Gdl90Base
    {
        /// <summary>
        /// Standard GDL90 AHRS implementation 24 bytes long
        /// </summary>
        /// <param name="att"></param>
        public Gdl90Ahrs(Attitude att) : base(24)
        {
            Msg[0] = 0x4c;
            Msg[1] = 0x45;
            Msg[2] = 0x01;
            Msg[3] = 0x01;

            // All of the following have an LSB = 0.1
            var pitch = Convert.ToInt16(att.Pitch * -10); // MSFS reverses the values
            var roll = Convert.ToInt16(att.Bank * -10); // MSFS reverses the values
            var hdg = Convert.ToInt16(att.TrueHeading * 10);
            var slipSkid = Convert.ToInt16(att.SkidSlip * 10);
            var yaw = Convert.ToInt16(att.TurnRate * 10);
            var g = Convert.ToInt16((att.GForce * 10).AdjustToBounds(short.MinValue + 1, short.MaxValue - 1));

            var palt = Convert.ToInt32(att.PressureAlt.AdjustToBounds(short.MinValue + 1, short.MaxValue -1));
            var ias = Convert.ToInt16(att.AirspeedIndicated.AdjustToBounds(short.MinValue + 1, short.MaxValue - 1));
            var vs = Convert.ToInt16(att.VertSpeed.AdjustToBounds(short.MinValue + 1, short.MaxValue - 1));

            // Roll.
            Msg[4] = (byte)((roll >> 8) & 0xFF);
            Msg[5] = (byte)(roll & 0xFF);

            // Pitch.
            Msg[6] = (byte)((pitch >> 8) & 0xFF);
            Msg[7] = (byte)(pitch & 0xFF);

            // Heading.
            Msg[8] = (byte)((hdg >> 8) & 0xFF);
            Msg[9] = (byte)(hdg & 0xFF);

            // Slip/skid.
            Msg[10] = (byte)((slipSkid >> 8) & 0xFF);
            Msg[11] = (byte)(slipSkid & 0xFF);

            // Yaw rate.
            Msg[12] = (byte)((yaw >> 8) & 0xFF);
            Msg[13] = (byte)(yaw & 0xFF);

            // "G".
            Msg[14] = (byte)((g >> 8) & 0xFF);
            Msg[15] = (byte)(g & 0xFF);

            // Indicated Airspeed
            Msg[16] = (byte)((ias >> 8) & 0xFF);
            Msg[17] = (byte)(ias & 0xFF);

            // Pressure Altitude
            Msg[18] = (byte)((palt >> 8) & 0xFF);
            Msg[19] = (byte)(palt & 0xFF);

            // Vertical Speed
            Msg[20] = (byte)((vs >> 8) & 0xFF);
            Msg[21] = (byte)(vs & 0xFF);

            // Reserved
            Msg[22] = 0x7F;
            Msg[23] = 0xFF;
        }
    }
}

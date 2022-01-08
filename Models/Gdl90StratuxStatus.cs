using System;
using System.Collections.Generic;
using System.Text;

namespace fs2ff.Models
{
    public class Gdl90StratuxStatus : Gdl90Base
    {
        /// <summary>
        /// Stratux GDL90 message (1Hz) 29 bytes + number of towers * 6 bytes
        /// </summary>
        public Gdl90StratuxStatus() : base(29)
        {
            Msg[0] = (byte)'S';
            Msg[1] = (byte)'X';
            Msg[2] = 1;
            Msg[3] = 1; // "message version".

            for(int i = 4; i <= 11; i++)
            {
                Msg[i] = 0xFF;
            }

            // Capabilities 
            Msg[13] = 0x02;  // WASS GPS Enabled 1 == 3D GPS
            Msg[13] |= 1 << 2; // AHRS
            Msg[13] |= 1 << 3; // Pressure Alt
            Msg[13] |= 1 << 4; // CPU Temp Valid


            if (ViewModelLocator.Main.DataTrafficEnabled)
            {
                Msg[13] |= 1 << 5;
                Msg[13] |= 1 << 6;
            }

            if (ViewModelLocator.Main.DataPositionEnabled)
            {
                Msg[13] |= 1 << 7;
            }

            if (ViewModelLocator.Main.DataAttitudeEnabled)
            {
                Msg[12] = 1;
            }

            // IMU connected
            Msg[15] = 1 << 2;
            // Radio Count
            Msg[15] |= 0x02;
            // GPS Satellites locked
            Msg[16] = 0x0A;
            // GPS Satellites tracked
            Msg[17] = 0x0F;

            // TODO: 18-25 are UAT message stats can pull these from a global var


            var fakeTemp = Convert.ToUInt16(10 * 20.123);
            Msg[26] = (byte)(fakeTemp & (0xFF00 >> 8));
            Msg[27] = (byte)(fakeTemp & 0x00FF);

            // TODO: when we get Airport data etc. make each Airport a tower
            Msg[28] = 0;

            // TODO: Append each tower at the end 3 bytes Lat 3 bytes longitude, repeat for count

        }
    }
}

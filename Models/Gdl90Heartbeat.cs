using System;

namespace fs2ff.Models
{
    public class Gdl90Heartbeat : Gdl90Base
    {
        /// <summary>
        /// GDL-90 Heartbeat (1Hz) 7 bytes
        /// </summary>
        public Gdl90Heartbeat() : base(7)
        {
            // GLD90 heartbeat
            Msg[0] = 0x00;
            // UAT, Talk back
            Msg[1] = 0x11;

            if (ViewModelLocator.Main.DataPositionEnabled)
            {
                // GPS flag
                Msg[1] |= 0x80;
            }

            this.UpdateTime();
        }

        public void UpdateTime()
        {
            var secondsSinceMidnightUTC = Convert.ToInt32(DateTime.UtcNow.TimeOfDay.TotalSeconds);
            Msg[2] = (byte)(((secondsSinceMidnightUTC >> 16) << 7) | 0x1);
            Msg[3] = (byte)(secondsSinceMidnightUTC & 0xFF);
            Msg[4] = (byte)((secondsSinceMidnightUTC & 0xFFFF) >> 8);
        }
    }
}

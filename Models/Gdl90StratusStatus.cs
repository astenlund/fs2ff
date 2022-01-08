using System;
using System.Runtime.InteropServices;
using System.Text;

namespace fs2ff.Models
{
    public class Gdl90StratusStatus : Gdl90Base
    {

        /// <summary>
        /// GDL-90 (Appareo) Status Message (1Hz) 34 bytes
        /// </summary>
        public Gdl90StratusStatus() : base(34)
        {
            Msg[0] = 0x69;  // Message type "Stratus 3".
            Msg[1] = 0x0;   // ID message identifier.
            Msg[2] = 0x1;    // Message version.

            // These will show up in GP under the stratus device info
            var verInfo = Encoding.ASCII.GetBytes("Stratus Emu");
            Array.Copy(verInfo, 0, Msg, 3, verInfo.Length < 16 ? verInfo.Length : 16);
            Msg[18] = 0;

            verInfo = Encoding.ASCII.GetBytes("FS2GDL");
            Array.Copy(verInfo, 0, Msg, 19, verInfo.Length < 6 ? verInfo.Length : 6);
            // battery status 0 - 100
            Msg[25] = 98;

            // Charging ICON 0 not charging 0x10 charging
            Msg[26] = 0x10;
            
            // rest is reserved
        }
    }
}

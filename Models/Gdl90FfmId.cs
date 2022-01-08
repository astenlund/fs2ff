using System;
using System.Runtime.InteropServices;
using System.Text;

namespace fs2ff.Models
{
     public class Gdl90FfmId : Gdl90Base
    {
        /// <summary>
        /// ForeFlight "ID Message".
        /// GDL-90 (FFM) ID Message (1Hz) 39 bytes
        /// </summary>
        public Gdl90FfmId() : base(39)
        {
            Msg[0] = 0x65; // Message type "ForeFlight".
            Msg[1] = 0;    // ID message identifier.
            Msg[2] = 1;    // Message version.
            
            // Serial number. Set to "invalid" for now.
            for (var i = 3; i <= 10; i++)
            {
                Msg[i] = 0xFF;
            }

            var isStratux = ViewModelLocator.Main.DataStratuxEnabled;
            var devShortName = Encoding.UTF8.GetBytes($"FS2FF {(isStratux ? "X" : "S")}");
            Array.Copy(devShortName, 0, Msg, 11, devShortName.Length > 8 ? 8 : devShortName.Length);
            // TODO could make this something else but doesn't matter
            devShortName = Encoding.UTF8.GetBytes($"FS2FF {(isStratux ? "Stratux" : "Stratus")}");
            Array.Copy(devShortName, 0, Msg, 19, devShortName.Length > 16 ? 16 : devShortName.Length);

            // GDL90 default
            Msg[38] = 0x00; // Just for correctness 
        }
    }
}

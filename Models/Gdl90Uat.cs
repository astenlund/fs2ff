using System;
using System.Threading;

namespace fs2ff.Models
{
    // TODO: MSFS currently doesn't support getting in game weather and Airport querying is broken
    // Maybe I'll bother implementing this when I can actually get Weather data at a given airport
    public class Gdl90Uat : Gdl90Base
    {
        // TODO: Add data counters for incoming UAT MSGs
        //private static long uatMetarCount = 0;

        /// <summary>
        /// UAT message protocol (WIP)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        public Gdl90Uat(RelayMsgType id, byte[] msg) : base (msg.Length + 4)
        {
            Msg[0] = (byte)id;
            
            // TODO MSG 1-3 is some sort of time value
            
            Array.Copy(msg, 0, Msg, 4, msg.Length);
        }

        // TODO: constructor to take Text UAT messages and convert them

        // TODO: Add data counters for incoming UAT MSGs
        //public static long UatMetarCount
        //{
        //    get => Interlocked.Read(ref uatMetarCount);
        //    set => Interlocked.Add(ref uatMetarCount, value);
        //}

    }

    public enum RelayMsgType : byte
    {
        UPLINK = 0x07,
        BASIC_REPORT = 0x1E,
        LONG_REPORT = 0x1F
    }
}

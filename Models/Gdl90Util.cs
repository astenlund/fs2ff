using System;
using System.Collections.Generic;
using System.Linq;

namespace fs2ff.Models
{
    public static class Gdl90Util
    {
        public const double LON_LAT_RESOLUTION = 180.0 / 8388608.0;
        public const double TRACK_RESOLUTION = 360.0 / 256.0;
        public const double METERS_PER_FOOT = 0.3048;
        public const double FEET_PER_METER = 3.28084;
        public const double METERS_TO_KNOTS = 0.5144447;
        public const double METERS_TO_MILES = 0.000621371193;
        public static ushort[] Crc16Table = new ushort[256];

        static Gdl90Util()
        {
            CrcInit();
        }

        /// <summary>
        /// Rounds a number to the nearest X
        /// </summary>
        /// <param name="value">The value to round</param> 
        /// <param name="stepAmount">The amount to round the value by</param>
        /// <param name="type">The type of rounding to perform</param>
        /// <returns>The value rounded by the step amount and type</returns>
        public static double RoundBy(this double value, double stepAmount)
        {
            var inverse = 1 / stepAmount;
            var dividend = value * inverse;
            dividend = Math.Round(dividend);
            var result = dividend / inverse;
            return result;
        }
        
        /// <summary>
        /// Converts the double from meters per second to knots
        /// </summary>
        /// <param name="mps">Meters Per Second</param>
        /// <returns>Knots</returns>
        public static double MetersToKnots(this double mps)
        {
            return mps * METERS_TO_KNOTS;
        }

        /// <summary>
        /// Converts from meters to feet
        /// </summary>
        /// <param name="meters"></param>
        /// <returns></returns>
        public static double MetersToFeet(this double meters)
        {
            return meters * FEET_PER_METER;
        }

        /// <summary>
        /// Convert feet to meters
        /// </summary>
        /// <param name="feet">feet</param>
        /// <returns>meters</returns>
        public static double FeetToMeters(this double feet)
        {
            return feet * METERS_PER_FOOT;
        }

        /// <summary>
        /// Meters to Miles
        /// </summary>
        /// <param name="meters"></param>
        /// <returns></returns>
        public static double MetersToMiles(this double meters)
        {
            return meters * METERS_TO_MILES;
        }

        /// <summary>
        /// Computes the CRC for the given byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort ComputeCrc(IEnumerable<byte> data)
        {
            ushort ret = 0;
            foreach(var d in data)
            {
                ret = (ushort)(Crc16Table[ret >> 8] ^ (ret << 8) ^ (d));
            }

            return ret;
        }

        /// <summary>
        /// Converts the given byte array to a properly formatted GDL90 data structure
        /// Adds the GDL90 start and end codes, escapes bytes, computes and appends the CRC
        /// </summary>
        /// <param name="data">Basic GDL90 message structure</param>
        /// <returns>Correctly formatted GDL90 message for transmission</returns>
        public static byte[] MakeGdl90Message(this IEnumerable<byte> data)
        {
            var crc = ComputeCrc(data);
            data = data.Append((byte)(crc & 0xFF));
            data = data.Append((byte)(crc >> 8));
            IEnumerable<byte> tmp = new byte[] { 0x7E };

            foreach (var b in data)
            {
                byte mv = b;
                if (mv == 0x7E || mv == 0x7D)
                {
                    mv = (byte)(mv ^ 0x20);
                    tmp = tmp.Append((byte)0x7D);
                }

                tmp = tmp.Append(mv);
            }

            tmp = tmp.Append((byte)0x7E); // Flag end.
            return tmp.ToArray();
        }

        /// <summary>
        /// Converts the current double (lat/long value) into a 3 byte value
        /// </summary>
        /// <param name="v">double to convert</param>
        /// <returns>3 bytes containing the lat or long value</returns>
        public static byte[] MakeLatLng(this double v)
        {
            var ret = new byte[3];
            v = v / LON_LAT_RESOLUTION;
            var wk = Convert.ToInt32(v);
            ret[0] = (byte)((wk & 0xFF0000) >> 16);
            ret[1] = (byte)((wk & 0x00FF00) >> 8);
            ret[2] = (byte)(wk & 0x0000FF);

            return ret;
        }

        /// <summary>
        /// String of Hex to a Hex value
        /// </summary>
        /// <param name="hex">string of Hex</param>
        /// <returns>byte array of hex values</returns>
        public static byte[] FromHex(this string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Builds the GDL90 CRC hash table
        /// </summary>
        private static void CrcInit()
        {
            for (ushort i = 0; i < 256; i++)
            {
                ushort crc = (ushort)(i << 8);
                for (ushort bitctr = 0; bitctr < 8; bitctr++)
                {
                    ushort z = 0;
                    if ((crc & 0x8000) != 0)
                    {
                        z = 0x1021;
                    }

                    crc = (ushort)((crc << 1) ^ z);
                }

                Crc16Table[i] = crc;
            }
        }
    }
}

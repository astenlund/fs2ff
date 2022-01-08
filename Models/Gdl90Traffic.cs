using System;
using System.Text;

namespace fs2ff.Models
{
    public class Gdl90Traffic : Gdl90Base
    {
        /// <summary>
        /// Used for both ownership report and traffic report 28 bytes
        /// As Ownership message 5hz
        /// As Traffic report 1hz
        /// </summary>
        /// <param name="traffic">Traffic object to convert</param>
        /// <param name="isOwner">true if owner otherwise false</param>
        public Gdl90Traffic(Traffic traffic, uint iaco) : base(28)
        {
            var isOwner = iaco == 1;
            var owner = isOwner ? traffic : ViewModelLocator.Main.OwnerInfo;
            // 0x0A (10) Ownership message
            // 0x14 (20) Standard traffic
            if (isOwner)
            {
                Msg[0] = 0xA;
                iaco = 0x00A00001;
            }
            else
            {
                Msg[0] = 0x14;
            }

            var code = BitConverter.GetBytes(iaco);
            if (code[2] != 0xF0 && code[2] != 0x00)
            {
                Msg[1] = 0x00; // ADS-B Out with ICAO
                Msg[2] = code[2]; // Mode S address.
                Msg[3] = code[1]; // Mode S address.
                Msg[4] = code[0]; // Mode S address.
            }
            else
            {
                Msg[1] = 0x01; // ADS-B Out with self-assigned code
                // Reserved dummy code.
                Msg[2] = 0xF0;
                Msg[3] = 0x00;
                Msg[4] = 0x00;
            }

            // Convert double lat to 3 bytes
            var tmp = traffic.Latitude.MakeLatLng();
            Array.Copy(tmp, 0, Msg, 5, tmp.Length);

            // Convert double longitude to 3 bytes
            tmp = traffic.Longitude.MakeLatLng();
            Array.Copy(tmp, 0, Msg, 8, tmp.Length);

            // Altitude. LSB = 25ft starting at -1000 (0x00)
            var altf = (traffic.Altitude + 1000) / 25;

            // Range -1000 -> 101,350 feet so doesn't work with the space shuttle
            ushort alt = (ushort)((altf < -1000 || altf > 101350) ? 0x0FFF : Convert.ToUInt16(altf));
            Msg[11] = (byte)((alt & 0xFF0) >> 4); 
            Msg[12] = (byte)((alt & 0x00F) << 4);

            Msg[12] |= 0x01;

            // MSFS has a desire to show stationary planes on not on ground
            if (!traffic.OnGround && (traffic.GroundVelocity > 0 || traffic.VerticalSpeed > 0))
            {
                Msg[12] = (byte)(Msg[12] | 1 << 3);

                if (!isOwner && owner.IsAlertable(traffic))
                {
                    // Set the alert bit. I haven't found an EFB that uses this
                    Msg[1] |= 0x10;
                }
            }

            // GPS NIC and NACp
            // Both values range from 0 (unknown) to 0XB (11) lower than 4 means degraded target.
            Msg[13] = 0xB0 | (0x0B & 0x0F);

            // Ground speed 12 bits LSB = 1kts
            var knots = Convert.ToUInt16(traffic.GroundVelocity);
            Msg[14] = (byte)((knots & 0x0FF0) >> 4);
            Msg[15] = (byte)((knots & 0x000F) << 4);

            // Vertical Speed 12 bits LSB = 64ft/m
            var verticalVelocity = Convert.ToInt16(traffic.VerticalSpeed.RoundTo(64) / 64);
            Msg[15] |= (byte)((verticalVelocity & 0x0F00) >> 8);
            Msg[16] = (byte)(verticalVelocity & 0x00FF);

            // Truncate Heading to 359 to not overflow the convert below
            var trk = Math.Min(traffic.TrueHeading, 359);
            // Heading is 360/256 to fit into 1 byte
            trk /= Gdl90Util.TRACK_RESOLUTION;
            Msg[17] = Convert.ToByte(trk);

            if (traffic.Category == "Helicopter")
            {
                Msg[18] = 0x7;
            }
            else if (traffic.Category == "Airplane")
            {
                if (traffic.MaxGrossWeight < 15500)
                {
                    // Light
                    Msg[18] = 0x1;
                }
                else if (traffic.MaxGrossWeight < 75000)
                {
                    // Small
                    Msg[18] = 0x2;
                }
                else if (traffic.MaxGrossWeight < 300000)
                {
                    // Large
                    Msg[18] = 0x2;
                }
                else
                {
                    // Heavy (B747)
                    Msg[18] = 0x5;
                }
                // TODO: Add more aircraft types (glider, high-speed, High Vortex, etc.)
            }
            else
            {
                Msg[18] = 0;
            }

            var tail = "None";
            if (!string.IsNullOrEmpty(traffic.TailNumber))
            {
                tail = traffic.TailNumber.Trim();
            }

            // Max length 8 bytes
            var tailBytes = Encoding.ASCII.GetBytes(tail);
            for (int i = 0; i < tailBytes.Length && i < 8; i++)
            {
                var c = tailBytes[i];
                // Remove special characters See p.24, FAA ref.
                if (c != 0x20 && !((c >= 48) && (c <= 57)) && !((c >= 65) && (c <= 90)) && c != 'e' && c != 'u' && c != 'a' && c != 'r' && c != 't') 
                {
                    c = 0x20;
                }

                Msg[19 + i] = c;
            }

            //// if (!isOwner) traffic.TransponderCode = 7700;

            // Priority status 0-6, 0 is normal
            // I haven't found an EFB that uses this
            switch (traffic.TransponderCode)
            {
                // Emergency
                case 7700:
                    Msg[27] = 1 << 4;
                    // TODO: Decide if I want to set alert on traffic in these cases
                    Msg[0] |= 0x10;
                    break;
                // Lost communication
                case 7600:
                    Msg[27] = 4 << 4;
                    Msg[0] |= 0x10;
                    break;
                // Hijack
                case 7500:
                    Msg[27] = 5 << 4;
                    Msg[0] |= 0x10;
                    break;
                default:
                    Msg[27] = 0;
                    break;
            };
        }
    }
}

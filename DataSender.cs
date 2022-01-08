using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using fs2ff.Models;

namespace fs2ff
{
    public class DataSender : IDisposable
    {
        private const int FlightSimPort = 49002;
        private const int Gdl90Port = 4000;
        private const string SimId = "MSFS";

        private IPEndPoint? _endPoint;
        private Socket? _socket;

        public void Connect(IPAddress? ip)
        {
            Disconnect();
            int port = ViewModelLocator.Main.DataGdl90Enabled ? port = Gdl90Port : FlightSimPort;

            ip ??= IPAddress.Broadcast;

            _endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(_endPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = ip.Equals(IPAddress.Broadcast),
            };

            // TODO: Make this a separate option for Stratus 3 emulation
            // To get Garmin Pilot to work with GDL90 Stratus, The Host PC must have an IP of 10.29.39.1 and
            // The Client needs to be in the same subnet otherwise GP will ICMP DU it. If your PC has a WiFi adapter
            // Setup like this: 1. Enable WiFi Hot Spot 2. Add Static IP of 10.29.39.1 to the WiFi adapter
            // 3. Connect device (iPad) to the Hot spot then switch from dynamic IP to Static with IP 10.29.39.x
            // 4. Make 10.29.39.1 the default gateway and use standard DNS like 1.1.1.1
            if (ViewModelLocator.Main.DataGdl90Enabled && ViewModelLocator.Main.DataStratusEnabled)
            {
                try
                {
                    this._socket.Bind(new IPEndPoint(IPAddress.Parse("10.29.39.1"), 4001));
                }catch(Exception ex)
                {
                    Console.Error.WriteLine($"Unable to bind to 10.29.39.1. To emulate a Stratus you must have this IP bound to your sending NIC\r\n{ex.Message}");
                }
            }
        }

        public void Disconnect() => _socket?.Dispose();

        public void Dispose() => _socket?.Dispose();

        public async Task Send(Attitude a)
        {
            if (ViewModelLocator.Main.DataGdl90Enabled)
            {
                var ffAhrs = new Gdl90FfmAhrs(a);
                var data = ffAhrs.ToGdl90Message();
                await Send(data).ConfigureAwait(false);

                // Right now this isn't supported by GP  or FF
                if (ViewModelLocator.Main.DataStratuxEnabled)
                {
                    var ahrs = new Gdl90Ahrs(a);
                    data = ahrs.ToGdl90Message();
                    await Send(data).ConfigureAwait(false);
                }
            }
            else
            {
                // Using a slip value between -127 and +127. .005 converts GP to be similar to the in game G1000 slip indicator
                var slipDeg = a.SkidSlip * -0.005;
                var data = string.Format(CultureInfo.InvariantCulture,
                    $"XATT{SimId},{a.TrueHeading:0.##},{-a.Pitch:0.##},{-a.Bank:0.##},,,{a.TurnRate:0.##},,,,{slipDeg:0.###},,");

                await Send(data).ConfigureAwait(false);
            }
        }

        public async Task Send(Position p)
        {
            if (!ViewModelLocator.Main.DataGdl90Enabled)
            {

                var data = string.Format(CultureInfo.InvariantCulture,
                "XGPS{0},{1:0.#####},{2:0.#####},{3:0.##},{4:0.###},{5:0.##}",
                SimId, p.Longitude, p.Latitude, p.Altitude, p.GroundTrack, p.GroundSpeed);

                await Send(data).ConfigureAwait(false);
            }
            else
            {
                Gdl90GeoAltitude geoAlt = new Gdl90GeoAltitude(p);
                var data = geoAlt.ToGdl90Message();
                await Send(data).ConfigureAwait(false);
            }
        }

        public async Task Send(Traffic t, uint id)
        {
            if (!t.IsValid())
            {
                return;
            }

            if (ViewModelLocator.Main.DataGdl90Enabled)
            {
                var traffic = new Gdl90Traffic(t, id);
                var data = traffic.ToGdl90Message();
                await Send(data).ConfigureAwait(false);
            }
            else
            {
                var data = string.Format(CultureInfo.InvariantCulture,
                    "XTRAFFIC{0},{1},{2:0.#####},{3:0.#####},{4:0.#},{5:0.#},{6},{7:0.###},{8:0.#},{9}",
                    SimId, id, t.Latitude, t.Longitude, t.Altitude, t.VerticalSpeed, t.OnGround ? 0 : 1,
                    t.TrueHeading, t.GroundVelocity, TryGetFlightNumber(t) ?? t.TailNumber);

                await Send(data).ConfigureAwait(false);
            }
        }

        private async Task Send(string data)
        {
            if (_endPoint != null && _socket != null)
            {
                await _socket
                    .SendToAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(data)), SocketFlags.None, _endPoint)
                    .ConfigureAwait(false);
            }
        }
        
        public async Task Send(byte[] data)
        {
            if (_endPoint != null && _socket != null)
            {
                await _socket
                    .SendToAsync(data, SocketFlags.None, _endPoint)
                    .ConfigureAwait(false);
            }
        }

        private static string? TryGetFlightNumber(Traffic t) =>
            !string.IsNullOrEmpty(t.Airline) && !string.IsNullOrEmpty(t.FlightNumber)
                ? $"{t.Airline} {t.FlightNumber}"
                : null;
    }
}

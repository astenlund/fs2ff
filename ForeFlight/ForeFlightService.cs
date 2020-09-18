using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using fs2ff.Models;

namespace fs2ff.ForeFlight
{
    public class ForeFlightService : IDisposable
    {
        private const int Port = 49002;
        private const string SimId = "MSFS";

        private readonly IPEndPoint _endPoint;
        private readonly Socket _socket;

        public ForeFlightService()
        {
            _endPoint = new IPEndPoint(IPAddress.Broadcast, Port);
            _socket = new Socket(_endPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true
            };
        }

        public void Dispose() => _socket.Dispose();

        public async Task Send(Attitude a)
        {
            var data = string.Format(CultureInfo.InvariantCulture,
                "XATT{0},{1:0.#},{2:0.#},{3:0.#}",
                SimId, a.TrueHeading, -a.Pitch, -a.Bank);

            await Send(data).ConfigureAwait(false);
        }

        public async Task Send(Position p)
        {
            var data = string.Format(CultureInfo.InvariantCulture,
                "XGPS{0},{1:0.#####},{2:0.#####},{3:0.#},{4:0.###},{5:0.#}",
                SimId, p.Longitude, p.Latitude, p.Altitude, p.GroundTrack, p.GroundSpeed);

            await Send(data).ConfigureAwait(false);
        }

        public async Task Send(Traffic t, uint id)
        {
            var data = string.Format(CultureInfo.InvariantCulture,
                "XTRAFFIC{0},{1},{2:0.#####},{3:0.#####},{4:0.#},{5:0.#},{6},{7:0.###},{8:0.#},{9}",
                SimId, id, t.Latitude, t.Longitude, t.Altitude, t.VerticalSpeed, t.OnGround ? 0 : 1,
                t.TrueHeading, t.GroundVelocity, TryGetFlightNumber(t) ?? t.TailNumber);

            await Send(data).ConfigureAwait(false);
        }

        private async Task Send(string data) =>
            await _socket
                .SendToAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(data)), SocketFlags.None, _endPoint)
                .ConfigureAwait(false);

        private static string? TryGetFlightNumber(Traffic t) =>
            !string.IsNullOrEmpty(t.Airline) && !string.IsNullOrEmpty(t.FlightNumber)
                ? $"{t.Airline} {t.FlightNumber}"
                : null;
    }
}

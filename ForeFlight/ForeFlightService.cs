using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public void Send(Attitude a)
        {
            var data = string.Format(CultureInfo.InvariantCulture,
                "XATT{0},{1:0.#},{2:0.#},{3:0.#}",
                SimId, a.TrueHeading, -a.Pitch, -a.Bank);

            _socket.SendTo(Encoding.ASCII.GetBytes(data), _endPoint);
        }

        public void Send(Position p)
        {
            var data = string.Format(CultureInfo.InvariantCulture,
                "XGPS{0},{1:0.#####},{2:0.#####},{3:0.#},{4:0.###},{5:0.#}",
                SimId, p.Longitude, p.Latitude, p.Altitude, p.GroundTrack, p.GroundSpeed);

            _socket.SendTo(Encoding.ASCII.GetBytes(data), _endPoint);
        }
    }
}

// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Runtime.InteropServices;
using fs2ff.Models;
using Microsoft.FlightSimulator.SimConnect;

namespace fs2ff.FlightSim
{
    public class FlightSimService : IDisposable
    {
        private const string AppName = "fs2ff";
        private const uint WM_USER_SIMCONNECT = 0x0402;

        private SimConnect? _simConnect;

        public event Action<Attitude>? AttitudeReceived;
        public event Action<Position>? PositionReceived;
        public event Action<bool>? StateChanged;

        public bool Connected => _simConnect != null;

        public void Connect(IntPtr hwnd)
        {
            try
            {
                _simConnect = new SimConnect(AppName, hwnd, WM_USER_SIMCONNECT, null, 0);
                SubscribeEvents();
                StateChanged?.Invoke(false);
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
                StateChanged?.Invoke(true);
            }
        }

        public void Disconnect() => DisconnectInternal(false);

        public void Dispose() => DisconnectInternal(false);

        public void ReceiveMessage()
        {
            try
            {
                _simConnect?.ReceiveMessage();
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
                DisconnectInternal(true);
            }
        }

        private void AddToDataDefinition(DEFINITION defineId, string datumName, string unitsName, SIMCONNECT_DATATYPE datumType = SIMCONNECT_DATATYPE.FLOAT64)
        {
            _simConnect?.AddToDataDefinition(defineId, datumName, unitsName, datumType, 0, SimConnect.SIMCONNECT_UNUSED);
        }

        private void DisconnectInternal(bool failure)
        {
            UnsubscribeEvents();

            _simConnect?.Dispose();
            _simConnect = null;

            StateChanged?.Invoke(failure);
        }

        private void RegisterAttitudeStruct()
        {
            AddToDataDefinition(DEFINITION.Attitude, "PLANE PITCH DEGREES", "Degrees");
            AddToDataDefinition(DEFINITION.Attitude, "PLANE BANK DEGREES", "Degrees");
            AddToDataDefinition(DEFINITION.Attitude, "PLANE HEADING DEGREES TRUE", "Degrees");

            _simConnect?.RegisterDataDefineStruct<Attitude>(DEFINITION.Attitude);
        }

        private void RegisterPositionStruct()
        {
            AddToDataDefinition(DEFINITION.Position, "PLANE LATITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "PLANE LONGITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "PLANE ALTITUDE", "Meters");
            AddToDataDefinition(DEFINITION.Position, "GPS GROUND TRUE TRACK", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "GPS GROUND SPEED", "Meters per second");

            _simConnect?.RegisterDataDefineStruct<Position>(DEFINITION.Position);
        }

        private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT data)
        {
            if (data.uEventID == (ulong) EVENT.SixHz)
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Attitude, DEFINITION.Attitude,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.ONCE,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.Error.WriteLine("Exception caught: " + data.dwException);
            DisconnectInternal(true);
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV data)
        {
            RegisterPositionStruct();
            RegisterAttitudeStruct();

            _simConnect?.RequestDataOnSimObject(
                REQUEST.Position, DEFINITION.Position,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SECOND,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                0, 0, 0);

            _simConnect?.SubscribeToSystemEvent(EVENT.SixHz, "6Hz");
        }

        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            DisconnectInternal(false);
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (ulong) REQUEST.Position &&
                data.dwDefineID == (ulong) DEFINITION.Position &&
                data.dwData?.FirstOrDefault() is Position pos)
            {
                PositionReceived?.Invoke(pos);
            }

            if (data.dwRequestID == (ulong) REQUEST.Attitude &&
                data.dwDefineID == (ulong) DEFINITION.Attitude &&
                data.dwData?.FirstOrDefault() is Attitude att)
            {
                AttitudeReceived?.Invoke(att);
            }
        }

        private void SubscribeEvents()
        {
            if (_simConnect != null)
            {
                _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
                _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
                _simConnect.OnRecvEvent += SimConnect_OnRecvEvent;
                _simConnect.OnRecvException += SimConnect_OnRecvException;
                _simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_simConnect != null)
            {
                _simConnect.OnRecvSimobjectData -= SimConnect_OnRecvSimobjectData;
                _simConnect.OnRecvException -= SimConnect_OnRecvException;
                _simConnect.OnRecvEvent -= SimConnect_OnRecvEvent;
                _simConnect.OnRecvQuit -= SimConnect_OnRecvQuit;
                _simConnect.OnRecvOpen -= SimConnect_OnRecvOpen;
            }
        }
    }
}

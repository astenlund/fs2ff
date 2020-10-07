// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using fs2ff.Models;
using Microsoft.FlightSimulator.SimConnect;

namespace fs2ff.FlightSim
{
    public class FlightSimAdapter : IDisposable
    {
        private const string AppName = "fs2ff";
        private const uint WM_USER_SIMCONNECT = 0x0402;

        private Timer? _attitudeTimer;
        private SimConnect? _simConnect;

        public event Func<Attitude, Task>? AttitudeReceived;
        public event Func<Position, Task>? PositionReceived;
        public event Action<bool>? StateChanged;
        public event Func<Traffic, uint, Task>? TrafficReceived;

        public bool Connected => _simConnect != null;

        public void Connect(IntPtr hwnd, uint attitudeFrequency)
        {
            try
            {
                UnsubscribeEvents();

                _simConnect?.Dispose();
                _attitudeTimer?.Dispose();

                _simConnect = new SimConnect(AppName, hwnd, WM_USER_SIMCONNECT, null, 0);
                _attitudeTimer = new Timer(RequestAttitudeData, null, 100, 1000 / attitudeFrequency);

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

        public void SetAttitudeFrequency(uint frequency)
        {
            _attitudeTimer?.Change(0, 1000 / frequency);
        }

        private void AddToDataDefinition(DEFINITION defineId, string datumName, string? unitsName, SIMCONNECT_DATATYPE datumType = SIMCONNECT_DATATYPE.FLOAT64)
        {
            _simConnect?.AddToDataDefinition(defineId, datumName, unitsName, datumType, 0, SimConnect.SIMCONNECT_UNUSED);
        }

        private void DisconnectInternal(bool failure)
        {
            UnsubscribeEvents();

            _attitudeTimer?.Dispose();
            _attitudeTimer = null;

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

        private void RegisterTrafficStruct()
        {
            AddToDataDefinition(DEFINITION.Traffic, "PLANE LATITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "PLANE LONGITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "PLANE ALTITUDE", "Feet");
            AddToDataDefinition(DEFINITION.Traffic, "VELOCITY WORLD Y", "Feet per minute");
            AddToDataDefinition(DEFINITION.Traffic, "SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32);
            AddToDataDefinition(DEFINITION.Traffic, "PLANE HEADING DEGREES TRUE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "GROUND VELOCITY", "Knots");
            AddToDataDefinition(DEFINITION.Traffic, "ATC ID", null, SIMCONNECT_DATATYPE.STRING64);
            AddToDataDefinition(DEFINITION.Traffic, "ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING64);
            AddToDataDefinition(DEFINITION.Traffic, "ATC FLIGHT NUMBER", null, SIMCONNECT_DATATYPE.STRING8);

            _simConnect?.RegisterDataDefineStruct<Traffic>(DEFINITION.Traffic);
        }

        private void RequestAttitudeData(object? _)
        {
            try
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Attitude, DEFINITION.Attitude,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.ONCE,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
            }
        }

        private void SimConnect_OnRecvEventObjectAddremove(SimConnect sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data)
        {
            if (data.uEventID == (uint) EVENT.ObjectAdded &&
                (data.eObjType == SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT ||
                 data.eObjType == SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER) &&
                data.dwData != SimConnect.SIMCONNECT_OBJECT_ID_USER)
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.TrafficObjectBase + data.dwData,
                    DEFINITION.Traffic, data.dwData,
                    SIMCONNECT_PERIOD.SECOND,
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
            RegisterTrafficStruct();

            _simConnect?.RequestDataOnSimObject(
                REQUEST.Position, DEFINITION.Position,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.SECOND,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                0, 0, 0);

            _simConnect?.RequestDataOnSimObjectType(REQUEST.TrafficAircraft, DEFINITION.Traffic, 200000, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
            _simConnect?.RequestDataOnSimObjectType(REQUEST.TrafficHelicopter, DEFINITION.Traffic, 200000, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);

            _simConnect?.SubscribeToSystemEvent(EVENT.ObjectAdded, "ObjectAdded");
            _simConnect?.SubscribeToSystemEvent(EVENT.SixHz, "6Hz");
        }

        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            DisconnectInternal(false);
        }

        private async void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint) REQUEST.Position &&
                data.dwDefineID == (uint) DEFINITION.Position &&
                data.dwData?.FirstOrDefault() is Position pos)
            {
                await PositionReceived.RaiseAsync(pos).ConfigureAwait(false);
            }

            if (data.dwRequestID == (uint) REQUEST.Attitude &&
                data.dwDefineID == (uint) DEFINITION.Attitude &&
                data.dwData?.FirstOrDefault() is Attitude att)
            {
                await AttitudeReceived.RaiseAsync(att).ConfigureAwait(false);
            }

            if (data.dwRequestID == (uint) REQUEST.TrafficObjectBase + data.dwObjectID &&
                data.dwDefineID == (uint) DEFINITION.Traffic &&
                data.dwObjectID != SimConnect.SIMCONNECT_OBJECT_ID_USER &&
                data.dwData?.FirstOrDefault() is Traffic tfk)
            {
                await TrafficReceived.RaiseAsync(tfk, data.dwObjectID).ConfigureAwait(false);
            }
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if ((data.dwRequestID == (uint) REQUEST.TrafficAircraft ||
                 data.dwRequestID == (uint) REQUEST.TrafficHelicopter) &&
                data.dwDefineID == (uint) DEFINITION.Traffic &&
                data.dwObjectID != SimConnect.SIMCONNECT_OBJECT_ID_USER)
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.TrafficObjectBase + data.dwObjectID,
                    DEFINITION.Traffic, data.dwObjectID,
                    SIMCONNECT_PERIOD.SECOND,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }
        }

        private void SubscribeEvents()
        {
            if (_simConnect != null)
            {
                _simConnect.OnRecvOpen += SimConnect_OnRecvOpen;
                _simConnect.OnRecvQuit += SimConnect_OnRecvQuit;
                _simConnect.OnRecvException += SimConnect_OnRecvException;
                _simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;
                _simConnect.OnRecvSimobjectDataBytype += SimConnect_OnRecvSimobjectDataBytype;
                _simConnect.OnRecvEventObjectAddremove += SimConnect_OnRecvEventObjectAddremove;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_simConnect != null)
            {
                _simConnect.OnRecvEventObjectAddremove -= SimConnect_OnRecvEventObjectAddremove;
                _simConnect.OnRecvSimobjectDataBytype -= SimConnect_OnRecvSimobjectDataBytype;
                _simConnect.OnRecvSimobjectData -= SimConnect_OnRecvSimobjectData;
                _simConnect.OnRecvException -= SimConnect_OnRecvException;
                _simConnect.OnRecvQuit -= SimConnect_OnRecvQuit;
                _simConnect.OnRecvOpen -= SimConnect_OnRecvOpen;
            }
        }
    }
}

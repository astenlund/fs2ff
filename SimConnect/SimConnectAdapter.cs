// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using fs2ff.Models;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectImpl = Microsoft.FlightSimulator.SimConnect.SimConnect;

namespace fs2ff.SimConnect
{
    public class SimConnectAdapter : IDisposable
    {
        private const string AppName = "fs2ff";
        private const uint WM_USER_SIMCONNECT = 0x0402;
        private const uint OBJECT_ID_USER_RESULT = 1;

        private Timer? _attitudeTimer;
        private SimConnectImpl? _simConnect;

        public event Func<Attitude, Task>? AttitudeReceived;
        public event Func<Position, Task>? PositionReceived;
        public event Action<bool>? StateChanged;
        public event Func<Traffic, uint, Task>? TrafficReceived;
        public event Func<Traffic, uint, Task>? OwnerReceived;

        public bool Connected => _simConnect != null;

        public void Connect(IntPtr hwnd, uint attitudeFrequency)
        {
            try
            {
                UnsubscribeEvents();

                _simConnect?.Dispose();
                _attitudeTimer?.Dispose();

                _simConnect = new SimConnectImpl(AppName, hwnd, WM_USER_SIMCONNECT, null, 0);
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
            _simConnect?.AddToDataDefinition(defineId, datumName, unitsName, datumType, 0, SimConnectImpl.SIMCONNECT_UNUSED);
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
            AddToDataDefinition(DEFINITION.Attitude, "TURN COORDINATOR BALL", "Degrees");
            AddToDataDefinition(DEFINITION.Attitude, "DELTA HEADING RATE", "Degrees");
            AddToDataDefinition(DEFINITION.Attitude, "AIRSPEED INDICATED", "Knots");
            AddToDataDefinition(DEFINITION.Attitude, "AIRSPEED TRUE", "Knots");
            AddToDataDefinition(DEFINITION.Attitude, "PRESSURE ALTITUDE", "Feet");
            AddToDataDefinition(DEFINITION.Attitude, "VELOCITY WORLD Y", "Feet per minute");
            AddToDataDefinition(DEFINITION.Attitude, "G FORCE", null);

            _simConnect?.RegisterDataDefineStruct<Attitude>(DEFINITION.Attitude);
        }

        private void RegisterPositionStruct()
        {
            AddToDataDefinition(DEFINITION.Position, "PLANE LATITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "PLANE LONGITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "PLANE ALTITUDE", "Feet");
            AddToDataDefinition(DEFINITION.Position, "GPS GROUND TRUE TRACK", "Degrees");
            AddToDataDefinition(DEFINITION.Position, "GPS GROUND SPEED", "Meters per second");

            _simConnect?.RegisterDataDefineStruct<Position>(DEFINITION.Position);
        }

        private void RegisterTrafficStruct()
        {
            AddToDataDefinition(DEFINITION.Traffic, "PLANE LATITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "PLANE LONGITUDE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "PLANE ALTITUDE", "Feet");
            // BUGBUG: All other traffic is reporting the Owners Pressure altitude (SU7) Use Plane Altitude instead
            AddToDataDefinition(DEFINITION.Traffic, "PRESSURE ALTITUDE", "Feet");
            AddToDataDefinition(DEFINITION.Traffic, "VELOCITY WORLD Y", "Feet per minute");
            AddToDataDefinition(DEFINITION.Traffic, "SIM ON GROUND", "Bool", SIMCONNECT_DATATYPE.INT32);
            AddToDataDefinition(DEFINITION.Traffic, "PLANE HEADING DEGREES TRUE", "Degrees");
            AddToDataDefinition(DEFINITION.Traffic, "GROUND VELOCITY", "Knots");
            AddToDataDefinition(DEFINITION.Traffic, "ATC ID", null, SIMCONNECT_DATATYPE.STRING64);
            AddToDataDefinition(DEFINITION.Traffic, "ATC AIRLINE", null, SIMCONNECT_DATATYPE.STRING64);
            AddToDataDefinition(DEFINITION.Traffic, "ATC FLIGHT NUMBER", null, SIMCONNECT_DATATYPE.STRING8);
            AddToDataDefinition(DEFINITION.Traffic, "Category", null, SIMCONNECT_DATATYPE.STRING32);
            AddToDataDefinition(DEFINITION.Traffic, "MAX GROSS WEIGHT", "Pounds");
            AddToDataDefinition(DEFINITION.Traffic, "AIRSPEED INDICATED", "Knots");
            AddToDataDefinition(DEFINITION.Traffic, "AIRSPEED TRUE", "Knots");
            AddToDataDefinition(DEFINITION.Traffic, "TRANSPONDER CODE:1", null, SIMCONNECT_DATATYPE.INT32);
            AddToDataDefinition(DEFINITION.Traffic, "TRANSPONDER STATE:1", null, SIMCONNECT_DATATYPE.INT32);

            _simConnect?.RegisterDataDefineStruct<Traffic>(DEFINITION.Traffic);
        }

        private void RequestAttitudeData(object? _)
        {
            try
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Attitude, DEFINITION.Attitude,
                    SimConnectImpl.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.ONCE,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }
            catch (COMException e)
            {
                Console.Error.WriteLine("Exception caught: " + e);
            }
        }

        private void SimConnect_OnRecvEventObjectAddremove(SimConnectImpl sender, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE data)
        {
            if (data.uEventID == (uint) EVENT.ObjectAdded &&
                (data.eObjType == SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT ||
                 data.eObjType == SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER) &&
                data.dwData != OBJECT_ID_USER_RESULT)
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.TrafficObjectBase + data.dwData,
                    DEFINITION.Traffic, data.dwData,
                    SIMCONNECT_PERIOD.SECOND,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }
        }

        private void SimConnect_OnRecvException(SimConnectImpl sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.Error.WriteLine("Exception caught: " + data.dwException);
            DisconnectInternal(true);
        }

        private void SimConnect_OnRecvOpen(SimConnectImpl sender, SIMCONNECT_RECV data)
        {
            RegisterPositionStruct();
            RegisterAttitudeStruct();
            RegisterTrafficStruct();

            if (ViewModelLocator.Main.DataGdl90Enabled)
            {
                // Sets the ownership report to run at ~10Hz.
                // This is less taxing than pulling with ONCE
                // SIM_FRAME = ~50hz or ~20ms (20ms * interval) so 5 * 20 == 200ms or 10Hz
                // GDL90 spec is 5hz but I find this Synthetic vision smoother at 10hz
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Owner, DEFINITION.Traffic,
                    SimConnectImpl.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.SIM_FRAME,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 5, 0);
            }

            if (ViewModelLocator.Main.DataGdl90Enabled)
            {
                // Send GeoAlt at 5hz (10 * 20ms == 200ms)
                // GDL90 specs calls for just 1hz but the EFBs I use work better at this rate
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Position, DEFINITION.Position,
                    SimConnectImpl.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.SIM_FRAME,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 10, 0);
            }
            else
            {
                _simConnect?.RequestDataOnSimObject(
                    REQUEST.Position, DEFINITION.Position,
                    SimConnectImpl.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.SECOND,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0, 0, 0);
            }

            _simConnect?.RequestDataOnSimObjectType(REQUEST.TrafficAircraft, DEFINITION.Traffic, 92600, SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
            _simConnect?.RequestDataOnSimObjectType(REQUEST.TrafficHelicopter, DEFINITION.Traffic, 92600, SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER);

            _simConnect?.SubscribeToSystemEvent(EVENT.ObjectAdded, "ObjectAdded");
            _simConnect?.SubscribeToSystemEvent(EVENT.SixHz, "6Hz");
            
            // TODO: will use the Airport data for setting up FIS-B weather reports These are broken right now,
            // they return the same 1234 airports 31 times.
            // _simConnect?.RequestFacilitiesList(SIMCONNECT_FACILITY_LIST_TYPE.AIRPORT, REQUEST.Airport);
            //_simConnect?.SubscribeToFacilities(SIMCONNECT_FACILITY_LIST_TYPE.AIRPORT, REQUEST.Airport);
        }

        private void SimConnect_OnRecvQuit(SimConnectImpl sender, SIMCONNECT_RECV data)
        {
            DisconnectInternal(false);
        }

        private async void SimConnect_OnRecvSimobjectData(SimConnectImpl sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (data.dwRequestID == (uint)REQUEST.TrafficObjectBase)
            {
                return;
            }

            if (data.dwRequestID == (uint) REQUEST.Position &&
                data.dwDefineID == (uint) DEFINITION.Position &&
                data.dwData?.FirstOrDefault() is Position pos)
            {
                await PositionReceived.RaiseAsync(pos).ConfigureAwait(false);
                return;
            }

            if (data.dwRequestID == (uint) REQUEST.Attitude &&
                data.dwDefineID == (uint) DEFINITION.Attitude &&
                data.dwData?.FirstOrDefault() is Attitude att)
            {

                await AttitudeReceived.RaiseAsync(att).ConfigureAwait(false);
                return;
            }

            if (data.dwRequestID == (uint)REQUEST.Owner &&
                data.dwDefineID == (uint)DEFINITION.Traffic &&
                (data.dwObjectID == OBJECT_ID_USER_RESULT 
                || data.dwObjectID == SimConnectImpl.SIMCONNECT_OBJECT_ID_USER) &&
                data.dwData?.FirstOrDefault() is Traffic owner)
            {
                await OwnerReceived.RaiseAsync(owner, data.dwObjectID).ConfigureAwait(false);
                return;
            }

            if (data.dwRequestID == (uint) REQUEST.TrafficObjectBase + data.dwObjectID &&
                data.dwDefineID == (uint) DEFINITION.Traffic &&
                data.dwObjectID != OBJECT_ID_USER_RESULT &&
                data.dwObjectID != SimConnectImpl.SIMCONNECT_OBJECT_ID_USER &&
                data.dwData?.FirstOrDefault() is Traffic tfk)
            {
                // Prevents all the parked aircraft from showing up on ADS-B
                // Could also use Master power/Avionics state but a plane with a transponder will more likely have ADS-B
                if (tfk.TransponderState != TranssponderState.Off)
                {
                    await TrafficReceived.RaiseAsync(tfk, data.dwRequestID).ConfigureAwait(false);
                }

                return;
            }

            Debug.WriteLine($"Unhandled event: {data.dwID}");
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnectImpl sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if ((data.dwRequestID == (uint) REQUEST.TrafficAircraft ||
                 data.dwRequestID == (uint) REQUEST.TrafficHelicopter) &&
                data.dwDefineID == (uint) DEFINITION.Traffic &&
                data.dwObjectID != OBJECT_ID_USER_RESULT)
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
//                _simConnect.OnRecvAirportList += _simConnect_OnRecvAirportList;
//                _simConnect.OnRecvCloudState += _simConnect_OnRecvCloudState;
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
//                _simConnect.OnRecvAirportList -= _simConnect_OnRecvAirportList;
//                _simConnect.OnRecvCloudState += _simConnect_OnRecvCloudState;
            }
        }

        //private void _simConnect_OnRecvCloudState(SimConnectImpl sender, SIMCONNECT_RECV_CLOUD_STATE data)
        //{
        //    Debug.WriteLine($"Data: {data.dwArraySize}");
        //}

        //private Dictionary<string, SIMCONNECT_DATA_FACILITY_AIRPORT> airports = new Dictionary<string, SIMCONNECT_DATA_FACILITY_AIRPORT>();
        //private void _simConnect_OnRecvAirportList(SimConnectImpl sender, SIMCONNECT_RECV_AIRPORT_LIST data)
        //{
        //    Debug.WriteLine($"Data: {data.dwArraySize}");
        //    var owner = ViewModelLocator.Main.OwnerInfo;
        //    foreach (SIMCONNECT_DATA_FACILITY_AIRPORT airport in data.rgData)
        //    {
        //        airports.TryAdd(airport.Icao, airport);
        //        if (Math.Round(airport.Latitude) == Math.Round(owner.Latitude) && Math.Round(airport.Longitude) == Math.Round(owner.Longitude))
        //        {
        //            _simConnect?.WeatherRequestCloudState(REQUEST.Weather, (float)airport.Latitude - 1, (float)airport.Longitude + 1, 100, (float)airport.Latitude + 1, (float)airport.Longitude + 1, 10000, 0);
        //        }
        //    }
        //}

    }
}

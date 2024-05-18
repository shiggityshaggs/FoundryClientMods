using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CargoDestinations
{
    internal enum StationType
    {
        None = -1,
        DropOff = 0,
        PickUp = 1,
    }

    class Helpers
    {
        private static IEnumerable<DroneTransportGO> _stations;
        internal static IEnumerable<DroneTransportGO> Stations => _stations ?? Refresh();
        internal static IEnumerable<DroneTransportGO> Refresh()
        {
            _stations = UnityEngine.Object.FindObjectsOfType<DroneTransportGO>(true).Where(s => IsValidDroneTransport(s) && !s.template.droneTransport_isStartStation);
            return _stations;
        }

        internal static bool IsValidDroneTransport(DroneTransportGO transport)
        {
            return transport?.template != null
                   && transport.template.type == BuildableObjectTemplate.BuildableObjectType.DroneTransport
                   && transport.transform.position != Vector3.zero;
        }

        internal static void ConstrainToScreen(ref Rect rect, bool shrinkToContent = false)
        {
            rect.x = Math.Max(0, Math.Min(rect.x, Screen.width - rect.width));
            rect.y = Math.Max(0, Math.Min(rect.y, Screen.height - rect.height));
            if (shrinkToContent) rect.width = 0; rect.height = 0;
        }

        internal static string GetStationName(ulong relatedEntityId, StationType stationType)
        {
            byte[] arr = new byte[128];
            uint count = 0U;
            DroneTransportGO.droneTransportEntity_getStationName(entityId: relatedEntityId, type: (byte)stationType, out_stationName: arr, stationNameLength: (uint)arr.Length, out_stationNameLength: ref count);
            return Encoding.UTF8.GetString(arr, 0, (int)count);
        }
    }
}

using System;
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

    internal class Station
    {
        public ulong EntityId { get; } = 0UL;
        public string StationName { get; set; } = string.Empty;
        public StationType StationType { get; } = StationType.None;
        public Vector3 Position { get; set; }
        public Station(ulong EntityId, bool IsStartStation, Vector3 Position)
        {
            this.EntityId = EntityId;
            this.StationType = IsStartStation ? StationType.PickUp : StationType.DropOff;
            this.StationName = Helpers.GetStationName(EntityId, StationType);
            this.Position = Position;
        }
    }

    class Helpers
    {
        internal static bool IsValidTemplate(BuildableObjectTemplate template)
        {
            return template != null && template.type == BuildableObjectTemplate.BuildableObjectType.DroneTransport;
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
            DroneTransportGO.droneTransportEntity_getStationName(entityId: relatedEntityId,
                                                                 type: (byte)stationType,
                                                                 out_stationName: arr,
                                                                 stationNameLength: (uint)arr.Length,
                                                                 out_stationNameLength: ref count);
            return Encoding.UTF8.GetString(arr, 0, (int)count);
        }
    }
}

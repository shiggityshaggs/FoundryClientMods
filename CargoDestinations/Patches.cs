using HarmonyLib;
using System.Text;
using UnityEngine;

namespace CargoDestinations
{
    [HarmonyPatch]
    class Patches
    {
        [HarmonyPatch(typeof(GameRoot), "Awake"), HarmonyPostfix]
        static void GameRoot_Awake()
        {
            var go = new GameObject("Experimental");
            go.transform.parent = GameRoot.getSingleton().transform;
            go.AddComponent<GUIComponent>();
        }

        //[HarmonyPatch(typeof(DroneTransportSetDestinationFrame), nameof(DroneTransportSetDestinationFrame.showFrame)), HarmonyPostfix]
        //static void DroneTransportSetDestinationFrame_showFrame()
        //{
        //}

        [HarmonyPatch(typeof(BuildableObjectGO), nameof(BuildableObjectGO.init)), HarmonyPrefix]
        static void DroneTransportGO_init(BuildableObjectTemplate buildableObjectTemplate, ulong relatedEntityId, BuildableObjectGO __instance)
        {
            if (!Helpers.IsValidTemplate(buildableObjectTemplate)) return;

            if (!GUIComponent.Stations.ContainsKey(relatedEntityId))
            {
                Vector3 position = __instance?.transform.position ?? Vector3.zero;
                Station station = new Station(relatedEntityId, buildableObjectTemplate.droneTransport_isStartStation, position);
                GUIComponent.Stations.Add(station.EntityId, station);
            }
        }

        [HarmonyPatch(typeof(DroneTransportGO), nameof(DroneTransportGO.droneTransportEntity_setStationName)), HarmonyPrefix]
        static void DroneTransportGO_droneTransportEntity_setStationName(ulong entityId, byte[] stationName, uint stationNameLength)
        {
            if (GUIComponent.Stations.TryGetValue(entityId, out Station station))
            {
                station.StationName = Encoding.UTF8.GetString(stationName, 0, (int)stationNameLength);
            }
        }
    }
}

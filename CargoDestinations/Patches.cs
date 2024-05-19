using HarmonyLib;
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

        [HarmonyPatch(typeof(DroneTransportSetDestinationFrame), nameof(DroneTransportSetDestinationFrame.showFrame)), HarmonyPostfix]
        static void DroneTransportSetDestinationFrame_showFrame()
        {
            Helpers.Refresh();
        }
    }
}

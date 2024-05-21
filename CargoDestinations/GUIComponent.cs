using System.Collections.Generic;
using UnityEngine;

namespace CargoDestinations
{
    [DisallowMultipleComponent]
    class GUIComponent : MonoBehaviour
    {
        internal static Dictionary<ulong, Station> Stations = new Dictionary<ulong, Station>();

        void Awake()
        {
            Stations.Clear();
        }

        void OnGUI()
        {
            if (DroneTransportSetDestinationFrame.singleton != null)
            {
                windowRect = GUILayout.Window(id: 321, screenRect: windowRect, func: WindowFunction, text: "Destinations", options: windowOptions);
                windowRect.x = 0;
                windowRect.y = (Screen.height / 2) - (windowRect.height / 2);
                Helpers.ConstrainToScreen(ref windowRect, true);
            }
        }

        Rect windowRect = new Rect();
        private readonly GUILayoutOption[] windowOptions = new GUILayoutOption[]
        {
            GUILayout.MinWidth(300),
            GUILayout.MinHeight(200),
            GUILayout.ExpandHeight(true),
            GUILayout.ExpandWidth(true)
        };

        void WindowFunction(int id)
        {
            foreach (var kvp in Stations)
            {
                Station station = kvp.Value;
                if (station.StationType != StationType.DropOff) continue;

                string displayString = string.Empty;
                bool unNamed = (station.StationName == "Unnamed Station");
                displayString += unNamed ? "Unnamed Station (Click to name)" : station.StationName;

                if (station.Position != Vector3.zero)
                {
                    var dist = Vector3.Distance(GameRoot.getRenderCharacterPosition(), station.Position);
                    displayString += $" ({dist:F0}m)";
                }

                GUI.color = unNamed ? Color.red : Color.green;
                if (GUILayout.Button(displayString))
                {
                    if (unNamed)
                    {
                        DroneTransportSetNameFrame.showFrame(station.EntityId);
                    }
                    else
                    {
                        DroneTransportSetDestinationFrame.singleton.inputField_stationName.tmp_inputField.text = station.StationName;
                    }
                }
            }
        }
    }
}

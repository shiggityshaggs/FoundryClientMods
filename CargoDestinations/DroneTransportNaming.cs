using UnityEngine;

namespace CargoDestinations
{
    [DisallowMultipleComponent]
    class GUIComponent : MonoBehaviour
    {
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
            foreach (var station in Helpers.Stations)
            {
                var name = station.StationName();
                var dist = Vector3.Distance(GameRoot.getRenderCharacterPosition(), station.transform.position);
                string distStr = $"({dist:F0}m)";

                bool unNamed = (name == "Unnamed Station");
                GUI.color = unNamed ? Color.red : Color.green;
                if (GUILayout.Button($"{name} {distStr}") && !unNamed)
                {
                    DroneTransportSetDestinationFrame.singleton.inputField_stationName.tmp_inputField.text = name;
                }
            }
        }
    }
}

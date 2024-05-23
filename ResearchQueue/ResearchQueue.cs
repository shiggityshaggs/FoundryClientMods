using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ResearchQueueMod
{
    [HarmonyPatch]
    internal class ResearchQueue
    {
        private static bool autoResearch;
        internal static ulong nextResearchTemplateId;
        internal static ResearchDetailsFrame researchDetailsFrame;
        internal static ResearchFrameManager researchFrameManager;
        internal static readonly HashSet<ulong> researchTemplateQueue = new HashSet<ulong>();

        [DisallowMultipleComponent]
        internal class ResearchQueueGUI : MonoBehaviour
        {
            Rect windowRect = new Rect();

            readonly GUILayoutOption[] windowOptions = new GUILayoutOption[]
            {
                GUILayout.MinWidth(200),
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
            };

            private void ConstrainToScreen(ref Rect rect, bool shrinkToContent = false)
            {
                rect.x = Screen.width;
                rect.y = Screen.height;
                rect.x = Math.Max(0, Math.Min(rect.x, Screen.width - rect.width));
                rect.y = Math.Max(0, Math.Min(rect.y, Screen.height - rect.height));
                if (shrinkToContent) rect.width = 0; rect.height = 0;
            }

            private void OnGUI()
            {
                if (GlobalStateManager.isDedicatedServer || !GameRoot.IsGameInitDone || researchFrameManager == null || !researchFrameManager.gameObject.activeSelf) return;
                windowRect = GUILayout.Window(id: 0, screenRect: windowRect, windowFunction, "Research Queue", options: windowOptions);
                ConstrainToScreen(ref windowRect, true);
            }

            private void windowFunction(int id)
            {
                autoResearch = GUILayout.Toggle(autoResearch, "Auto");

                if (!autoResearch)
                {
                    foreach (ulong rtId in researchTemplateQueue)
                    {
                        var rt = ItemTemplateManager.getResearchTemplateById(rtId);
                        if (rt == null) continue;
                        if (GUILayout.Button(rt.name))
                        {
                            researchTemplateQueue.Remove(rtId);
                            break;
                        }
                    }
                }
            }

            private void Update()
            {
                if (!GameRoot.IsGameInitDone || ResearchSystem.isAnyResearchActive()) return;

                if (autoResearch || GlobalStateManager.isDedicatedServer)
                {
                    var ordered = ResearchSystem.getAvailableResearchTemplateDictionary()
                                                .OrderBy(kvp => kvp.Value.highestSciencePackSortingOrder)
                                                .Where(kvp => !kvp.Value._isEndlessResearch());

                    foreach (var kvp in ordered)
                    {
                        if ((BuildInfo.isDemo && !kvp.Value.includeInDemo) || kvp.Value.isLockedByMissingEntitlement()) continue;
                        GameRoot.addLockstepEvent(new StartResearchEvent(kvp.Key, false));
                        break;
                    }
                }
                else
                {
                    if (nextResearchTemplateId != 0UL)
                    {
                        ulong researchTemplateId = nextResearchTemplateId;
                        nextResearchTemplateId = 0UL;
                        GameRoot.addLockstepEvent(new StartResearchEvent(researchTemplateId, false));
                    }
                }
            }
        }
    }
}

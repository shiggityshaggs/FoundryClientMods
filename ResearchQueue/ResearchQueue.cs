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
        internal static bool playResearchNotifications = true;
        internal static bool autoResearch;
        internal static float timer;
        internal static float delay = 1f;
        internal static bool hasSingleItemsLeft = true;
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
                rect.x = Screen.width - rect.width - 40;
                rect.y = Screen.height - rect.height - 40;
                rect.x = Math.Max(0, Math.Min(rect.x, Screen.width - rect.width));
                rect.y = Math.Max(0, Math.Min(rect.y, Screen.height - rect.height));
                if (shrinkToContent) rect.width = 0; rect.height = 0;
            }

            private void OnGUI()
            {
                if (GlobalStateManager.isDedicatedServer || !GameRoot.IsGameInitDone || researchFrameManager == null || !researchFrameManager.gameObject.activeSelf) return;
                GUI.backgroundColor = Color.black;
                windowRect = GUILayout.Window(id: 0, screenRect: windowRect, windowFunction, "Research Queue", options: windowOptions);
                ConstrainToScreen(ref windowRect, true);
            }

            private void windowFunction(int id)
            {
                if (!autoResearch)
                {
                    foreach (ulong rtId in researchTemplateQueue)
                    {
                        ResearchTemplate rt = ItemTemplateManager.getResearchTemplateById(rtId);
                        if (rt == null) continue;
                        GUI.color = Color.white;
                        if (GUILayout.Button(rt.name))
                        {
                            researchTemplateQueue.Remove(rtId);
                            break;
                        }
                    }
                }

                GUI.color = autoResearch ? Color.green : Color.red;
                autoResearch = GUILayout.Toggle(autoResearch, "Auto");

                GUI.color = playResearchNotifications ? Color.green : Color.red;
                playResearchNotifications = GUILayout.Toggle(playResearchNotifications, "Notifications");
            }

            private void Update()
            {
                if (timer > delay) { timer -= timer; }
                else { timer += Time.deltaTime; return; }

                if (!GameRoot.IsGameInitDone || ResearchSystem.isAnyResearchActive()) return;

                if (hasSingleItemsLeft && (autoResearch || GlobalStateManager.isDedicatedServer))
                {
                    var ordered = ResearchSystem.getAvailableResearchTemplateDictionary()
                                                .OrderBy(kvp => kvp.Value.highestSciencePackSortingOrder) // Do the least complex science packs first
                                                .ThenBy(kvp => kvp.Value.input.Sum(input => input.Value) * kvp.Value.secondsPerScienceItem) // Do the least time consuming first
                                                .Where(kvp => !kvp.Value.isLockedByMissingEntitlement()) // Respect entitlement
                                                .Where(kvp => !kvp.Value._isEndlessResearch()); // Don't get stuck on endless

                    if (BuildInfo.isDemo) ordered = ordered.Where(kvp => kvp.Value.includeInDemo);
                    if (!BuildInfo.isDemo) ordered = ordered.Where(kvp => !kvp.Value.flags.HasFlagNonAlloc(ResearchTemplate.ResearchTemplateFlags.HIDE_IN_NON_DEMO_BUILD));

                    hasSingleItemsLeft = ordered.Any(kvp => !kvp.Value._isEndlessResearch());

                    foreach (var (key,RT) in ordered)
                    {
                        GameRoot.addLockstepEvent(new StartResearchEvent(key, false));
                        break;
                    }
                    return;
                }

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

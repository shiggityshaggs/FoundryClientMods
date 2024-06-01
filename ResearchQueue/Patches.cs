using HarmonyLib;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace ResearchQueueMod
{
    [HarmonyPatch]
    class Patches
    {
        [HarmonyPatch(typeof(GameRoot), "Awake"), HarmonyPostfix]
        static void GameRoot_Awake()
        {
            var go = new GameObject("ResearchQueueGO");
            go.transform.parent = GameRoot.getSingleton().transform;
            go.AddComponent<ResearchQueue.ResearchQueueGUI>();
        }

        [HarmonyPatch(typeof(ResearchDetailsFrame), nameof(ResearchDetailsFrame.Awake)), HarmonyPostfix]
        static void ResearchDetailsFrame_Awake(ResearchDetailsFrame __instance)
        {
            ResearchQueue.researchDetailsFrame = __instance;
        }

        [HarmonyPatch(typeof(ResearchDetailsFrame), nameof(ResearchDetailsFrame.onClick_startResearch)), HarmonyPrefix]
        static bool ResearchDetailsFrame_onClick_startResearch(ResearchTemplate ___currentResearchTemplate)
        {
            var id = ___currentResearchTemplate.id;
            var RT = ItemTemplateManager.getResearchTemplateById(id);
            if (id == 0UL || RT == null) return true;

            if (!ResearchQueue.autoResearch && ResearchSystem.isAnyResearchActive())
            {
                ResearchQueue.researchTemplateQueue.Add(id);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ResearchFrameManager), nameof(ResearchFrameManager.Init)), HarmonyPostfix]
        static void ResearchFrameManager_Init(ResearchFrameManager ___singleton)
        {
            ResearchQueue.researchFrameManager = ___singleton;
        }

        [HarmonyPatch(typeof(ResearchSystem), "onResearchFinished"), HarmonyPostfix]
        static void ResearchSystem_onResearchFinished(ResearchTemplate rt)
        {
            ResearchQueue.timer -= ResearchQueue.timer;

            ResearchQueue.researchTemplateQueue.Remove(rt.id);

            if (ResearchQueue.autoResearch && !ResearchQueue.hasSingleItemsLeft && rt._isEndlessResearch())
            {
                ResearchQueue.nextResearchTemplateId = rt.id;
                return;
            }

            if (ResearchQueue.researchTemplateQueue.Any())
            {
                ResearchQueue.nextResearchTemplateId = ResearchQueue.researchTemplateQueue.First();
            }
        }

        [HarmonyPatch(typeof(NotificationManager_ResearchComplete), nameof(NotificationManager_ResearchComplete.queueResearchCompleteNotification)), HarmonyPrefix]
        static bool NotificationManager_ResearchComplete_queueResearchCompleteNotification()
        {
            return ResearchQueue.playResearchNotifications;
        }

        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.playUISoundEffect)), HarmonyPrefix]
        static bool AudioManager_playUISoundEffect(AudioClip audioClip)
        {
            if (!ResearchQueue.playResearchNotifications &&
                audioClip == ResourceDB.resourceLinker.audioClip_researchComplete) return false;

            return true;
        }
    }
}

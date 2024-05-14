using HarmonyLib;
using System.Linq;
using UnityEngine;

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

            if (id != 0UL && ResearchSystem.isAnyResearchActive() && !ResearchQueue.researchTemplateQueue.Contains(id))
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
            ResearchQueue.researchTemplateQueue.Remove(rt.id);

            if (ResearchQueue.researchTemplateQueue.Count > 0)
            {
                ResearchQueue.nextResearchTemplateId = ResearchQueue.researchTemplateQueue.First();
            }
        }
    }
}

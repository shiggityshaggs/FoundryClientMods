using HarmonyLib;

namespace SkipIntroAndTutorial
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(NarrativeManager), nameof(NarrativeManager.playGameStartAction)), HarmonyPrefix]
        static bool NarrativeManager_playGameStartAction()
        {
            return false; // Intro
        }

        [HarmonyPatch(typeof(TaskSystem), nameof(TaskSystem.Init)), HarmonyPrefix]
        static bool TaskSystem_Init()
        {
            return false; // Tutorial
        }

        [HarmonyPatch(typeof(GameRoot), "InitializeGameRoot", MethodType.Enumerator), HarmonyPostfix]
        static void GameRoot_InitializeGameRoot()
        {
            GlobalStateManager.GameSceneLoadData loadData = GlobalStateManager.getDataForNextGameSceneLoad();
            if (GameRoot.IsGameInitDone && loadData.savegame.isFirstGameStart)
            {
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_ALLOW_CRAFTING", false);
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_CR_DRONE_MINER", false);
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_CR_BIOMASS_BURNER", false);
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_CR_CONVEYORS", false);
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_CR_SMELTER", false);
                NarrativeSystem.activateNarrativeTrigger("NARRATIVE_ALLOW_RESEARCH", false);
                ResearchSystem.enableResearchSystem();
            }
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(JournalEntry))]
    [HarmonyPatch("Activate")]
    class JournalEntry_Activate
    {
        static bool Prefix(JournalEntry __instance, ref GameObject __result)
        {
            if (__instance.GetComponent<BuildObject>() != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.uiPrefab);
                gameObject.GetComponent<JournalUI>().SetJournalKey("custom=" + __instance.entryKey);
                foreach (ProgressDirector.ProgressType type in __instance.ensureProgress)
                {
                    SRSingleton<SceneContext>.Instance.ProgressDirector.SetProgress(type, 1);
                }
                __result = gameObject;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(JournalUI))]
    [HarmonyPatch("SetJournalKey")]
    class JournalUI_SetJournalKey
    {
        static bool Prefix(JournalUI __instance, string journalKey)
        {
            if (journalKey.StartsWith("custom="))
            {
                __instance.journalText.text = journalKey.Substring(7);
                return false;
            }
            return true;
        }
    }
}

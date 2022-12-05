using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(IntroUI))]
    [HarmonyPatch("Awake")]
    class IntroUI_Awake
    {
        static void Postfix(IntroUI __instance)
        {
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                GameObject.Destroy(__instance.gameObject);
            }
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(AchievementsDirector))]
    [HarmonyPatch("Update")]
    class AchievementsDirector_Update
    {
        static bool Prefix()
        {
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AchievementsDirector))]
    [HarmonyPatch("LateUpdate")]
    class AchievementsDirector_LateUpdate
    {
        static bool Prefix()
        {
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                return false;
            }
            return true;
        }
    }
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(PauseMenu))]
    [HarmonyPatch("PauseGame")]
    class PauseMenu_PauseGame
    {
        static bool Prefix()
        {
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                BetterBuildCamera.Instance.SetActive(true);
                return false;
            }
            return true;
        }
    }
}

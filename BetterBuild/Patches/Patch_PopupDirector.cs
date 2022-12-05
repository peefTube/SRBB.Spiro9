using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(PopupDirector))]
    [HarmonyPatch("MaybePopupNext")]
    class PopupDirector_MaybePopupNext
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

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(Identifiable))]
    [HarmonyPatch("Awake")]
    class Identifiable_Awake
    {
        static void Postfix(Identifiable __instance)
        {
            if (BetterBuildCamera.Instance != null && BetterBuildCamera.Instance.gameObject.activeSelf)
            {
                if(__instance.id != Identifiable.Id.PLAYER)
                {
                    try
                    {
                        Destroyer.DestroyActor(__instance.gameObject, "CameraActivated");
                    }
                    catch { }
                }
            }
        }
    }
}

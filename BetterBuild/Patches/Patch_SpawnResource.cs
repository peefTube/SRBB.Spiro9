using BetterBuild.Persistance;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(SpawnResource))]
    [HarmonyPatch("Update")]
    class SpawnResource_Update
    {
        static bool Prefix(SpawnResource __instance)
        {
            if(__instance.GetComponent<BuildObject>() != null)
            {
                __instance.UpdateToTime(__instance.timeDir.WorldTime(), __instance.timeDir.DeltaWorldTime());
                if (__instance.spawnQueue.Count > 0)
                {
                    __instance.Spawn(__instance.spawnQueue.Dequeue());
                }
                return false;
            }
            return true;
        }
    }
}

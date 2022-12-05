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
    [HarmonyPatch(typeof(PlayerModel))]
    [HarmonyPatch("SetCurrRegionSet")]
    class PlayerModel_SetCurrRegionSet
    {
        static void Postfix(RegionRegistry.RegionSetId regionSetId)
        {
            foreach(var buildObject in BuildObject.AllObjects.Values.ToList())
            {
                buildObject.gameObject.SetActive(buildObject.Region == regionSetId);
            }
        }
    }
}

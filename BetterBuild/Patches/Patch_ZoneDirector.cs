using HarmonyLib;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(ZoneDirector))]
    [HarmonyPatch("GetRegionSetId")]
    class ZoneDirector_GetRegionSetId
    {
        static bool Prefix(ZoneDirector.Zone zone, ref RegionRegistry.RegionSetId __result)
        {
            int zoneID = (int)zone;

            if (zoneID >= 100)
            {
                __result = (RegionRegistry.RegionSetId)zoneID;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ZoneDirector))]
    [HarmonyPatch("Update")]
    class ZoneDirector_Update
    {
        static bool Prefix()
        {
            if (BetterBuildCamera.Instance != null && BetterBuildCamera.Instance.gameObject.activeSelf)
            {
                return false;
            }
            return true;
        }
    }
}

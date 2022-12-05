using BetterBuild.Persistance;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(IdHandler))]
    [HarmonyPatch("id", MethodType.Getter)]
    class IdHandler_id
    {
        static bool Prefix(IdHandler __instance, ref string __result)
        {
            var buildObject = __instance.GetComponent<BuildObject>();
            if (buildObject != null)
            {
                if (buildObject.HandlerID == 0) buildObject.HandlerID = Globals.LastHandlerID++;
                __result = "bb" + buildObject.HandlerID;
                Debug.Log("Overwrote ID with " + __result);
                return false;
            }
            return true;
        }
    }
}

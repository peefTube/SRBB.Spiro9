using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(KillOnTrigger))]
    [HarmonyPatch("OnTriggerEnter")]
    class KillOnTrigger_OnTriggerEnter
    {
        static bool Prefix(Collider collider)
        {
            if(BetterBuildCamera.Instance != null && BetterBuildCamera.Instance.gameObject.activeSelf)
            {
                if(PhysicsUtil.IsPlayerMainCollider(collider))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

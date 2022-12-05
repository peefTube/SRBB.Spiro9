using BetterBuild.Persistance;
using BetterBuild.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(AutoSaveDirector))]
    [HarmonyPatch("SaveGame")]
    class AutoSaveDirector_SaveGame
    {
        static bool Prefix(AutoSaveDirector __instance)
        {
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                World.Save();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AutoSaveDirector))]
    [HarmonyPatch("OnGameLoaded")]
    class AutoSaveDirector_OnGameLoaded
    {
        static void Prefix(AutoSaveDirector __instance)
        {
            ObjectManager.Clear();

            if (BetterBuildMod.Instance.IsBuildMode)
            {
                World.Load();

                GameObject worldObj = new GameObject("World");
                worldObj.AddComponent<ObjectSelection>();
                worldObj.AddComponent<ObjectEditor>();
                worldObj.AddComponent<ZoneEditor>();
                worldObj.AddComponent<WireEditor>();
                worldObj.AddComponent<MissingEditor>();
                worldObj.AddComponent<UndoManager>();

                GameObject toolbarObj = GameObject.Instantiate(Globals.ToolbarPrefab);
                toolbarObj.AddComponent<ToolbarUI>();

                GameObject hierarchyObj = GameObject.Instantiate(Globals.HierarchyPrefab);
                hierarchyObj.AddComponent<HierarchyUI>();
                hierarchyObj.AddComponent<TeleportUI>();
                hierarchyObj.AddComponent<SettingsUI>();
                hierarchyObj.AddComponent<InspectorUI>();
                hierarchyObj.AddComponent<InfoUI>();

                BetterBuildCamera.CreateCamera();

                SRSingleton<SceneContext>.Instance.PlayerState.AddCurrency(10000000);
                foreach(var upgrade in (PlayerState.Upgrade[])Enum.GetValues(typeof(PlayerState.Upgrade)))
                {
                    SRSingleton<SceneContext>.Instance.PlayerState.AddUpgrade(upgrade);
                }
                //World.LoadObjectsFromFile();
            }
            else
            {
                GameObject worldObj = new GameObject("World");
                worldObj.AddComponent<ZoneEditor>();

                BetterBuildMod.Instance.LoadWorld();
            }
        }
    }
}

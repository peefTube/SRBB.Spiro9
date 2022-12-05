#if CUSTOM
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace BetterBuild.Loaders
{
    public class Custom
    {
        private GameObject m_GameObject;

        /// <summary>
        /// Call this function from any modloader. This will load the mod
        /// </summary>
        public void Load()
        {
            if (m_GameObject != null) return;

            var harmony = new Harmony("com.saty.betterbuild");
            harmony.PatchAll();

            if (!Directory.Exists(BetterBuildMod.ModDataPath))
                Directory.CreateDirectory(BetterBuildMod.ModDataPath);

            m_GameObject = new GameObject("BetterBuildMod");
            m_GameObject.AddComponent<BetterBuildMod>();

            GameObject.DontDestroyOnLoad(m_GameObject);
        }

        /// <summary>
        /// Call this function to remove the mod object and therefor unload everything
        /// </summary>
        public void Unload()
        {
            GameObject.Destroy(m_GameObject);
        }
    }
}
#endif

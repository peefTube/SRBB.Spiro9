using BetterBuild.Persistance;
using BetterBuild.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(FileStorageProvider))]
    [HarmonyPatch("StoreGameData")]
    class FileStorageProvider_StoreGameData
    {
        static void Prefix(string gameId, string gameName, string name, MemoryStream stream)
        {
            if (!BetterBuildMod.Instance.IsBuildMode && !string.IsNullOrEmpty(BetterBuildMod.Instance.CurrentLevel))
            {
                string[] data = name.Split('_');
                string fullFilePath = Path.Combine(BetterBuildMod.ModDataPath, string.Format("{0}_{1}{2}", BetterBuildMod.Instance.CurrentLevel, data[1], ".sav"));
                string text = string.Format("{0}{1}", fullFilePath, ".tmp");
                using (FileStream fileStream = File.Create(text))
                {
                    CopyStream(stream, fileStream);
                }
                File.Copy(text, fullFilePath, true);
                File.Delete(text);
            }
        }

        private static void CopyStream(Stream from, Stream to)
        {
            byte[] array = new byte[1024];
            int num;
            do
            {
                num = from.Read(array, 0, array.Length);
                to.Write(array, 0, num);
            }
            while (num >= array.Length);
        }
    }

    [HarmonyPatch(typeof(FileStorageProvider))]
    [HarmonyPatch("GetGameData")]
    class FileStorageProvider_GetGameData
    {
        static bool Prefix(FileStorageProvider __instance, string name, MemoryStream dataStream)
        {
            if (!BetterBuildMod.Instance.IsBuildMode && !string.IsNullOrEmpty(BetterBuildMod.Instance.CurrentLevel))
            {
                string[] data = name.Split('_');
                string fullFilePath = Path.Combine(BetterBuildMod.ModDataPath, string.Format("{0}_{1}{2}", BetterBuildMod.Instance.CurrentLevel, data[1], ".sav"));
                if (File.Exists(fullFilePath))
                {
                    __instance.Load(fullFilePath, name, dataStream);
                    return false;
                }
            }
            return true;
        }
    }
}

using HarmonyLib;
using MonomiPark.SlimeRancher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild.Patches
{
    [HarmonyPatch(typeof(SavedGame))]
    [HarmonyPatch("Load")]
    class SavedGame_Load
    {
        static void Postfix(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);

            Debug.Log(reader.BaseStream.Position + " - " + reader.BaseStream.Length);
            try
            {
                string header = reader.ReadString();
                if (header.Equals("BBDATA"))
                {
                    Debug.Log("Contains BB data!");
                }
            }
            catch
            {
                Debug.Log("No BB data!");
            }
        }
    }

    [HarmonyPatch(typeof(SavedGame))]
    [HarmonyPatch("Save")]
    class SavedGame_Save
    {
        static void Postfix(Stream stream)
        {
            //BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);

            //writer.Write("BBDATA");
        }
    }
}

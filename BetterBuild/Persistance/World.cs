using BetterBuild.UI;
using MonomiPark.SlimeRancher.Regions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BetterBuild.Persistance
{
    public static class World
    {
        public static Dictionary<uint, List<GameObject>> BuildObjects = new Dictionary<uint, List<GameObject>>();
        public static GameObject BuildObjectsHolder;

        public static WorldV01 SavedWorld;
        public static uint LastBuildID = 0;
        private static List<uint> ObjectsToLoad;
        private static int ObjectLoadCount;
        public static bool IsLoading { get { return ObjectsToLoad != null ? ObjectsToLoad.Any() : false; } }
        public static int LoadPercentage { get { return ObjectsToLoad != null ? (int)(((ObjectLoadCount - ObjectsToLoad.Count) / (float)ObjectLoadCount) * 100) : 0; } }

        public static int TotalObjects
        {
            get
            {
                int total = 0;
                foreach(var list in BuildObjects.Values.ToList())
                {
                    total += list.Count;
                }
                return total;
            }
        }

        public static void Load(bool async = true)
        {
            LastBuildID = 0;
            Globals.LastHandlerID = 0;
            BuildObjects = new Dictionary<uint, List<GameObject>>();
            BuildObjectsHolder = new GameObject("World");
            SavedWorld = new WorldV01();
            ObjectsToLoad = new List<uint>();
            ObjectLoadCount = 0;
            BuildObject.AllObjects.Clear();

            if (string.IsNullOrWhiteSpace(BetterBuildMod.Instance.CurrentLevel)) return;
            if(!File.Exists(Path.Combine(BetterBuildMod.ModDataPath, BetterBuildMod.Instance.CurrentLevel + ".world")))
            {
                SavedWorld.name = BetterBuildMod.Instance.CurrentLevel;
                SavedWorld.buildObjects = new Dictionary<uint, List<BuildObjectV04>>();
                return;
            }

            try
            {
                using (FileStream stream = new FileStream(Path.Combine(BetterBuildMod.ModDataPath, BetterBuildMod.Instance.CurrentLevel + ".world"), FileMode.Open))
                {
                    SavedWorld.Load(stream);
                    
                    foreach(var id in SavedWorld.buildObjects.Keys.ToList())
                    {
                        foreach(var data in SavedWorld.buildObjects[id])
                        {
                            if (async)
                            {
                                ObjectsToLoad.Add(data.BuildID);
                                ObjectLoadCount++;
                                ObjectManager.RequestObject(id, (buildObject) =>
                                {
                                    if (buildObject != null)
                                    {
                                        GameObject obj = GameObject.Instantiate(buildObject, data.pos.value, Quaternion.Euler(data.euler.value)) as GameObject;
                                        obj.GetComponent<BuildObject>().BuildID = data.BuildID;
                                        obj.GetComponent<BuildObject>().Region = data.region;
                                        obj.GetComponent<BuildObject>().HandlerID = data.HandlerID;
                                        obj.GetComponent<BuildObject>().SetData(data.Data);
                                        obj.transform.localScale = data.scale.value;
                                        AddObject(id, obj);
                                        obj.SetActive(true);
                                    }
                                    ObjectsToLoad.Remove(data.BuildID);
                                    ToolbarUI.Instance.UpdateStatus();
                                });
                            }
                            else
                            {
                                var bObj = ObjectManager.GetObject(id);
                                if (bObj != null)
                                {
                                    GameObject obj = GameObject.Instantiate(bObj, data.pos.value, Quaternion.Euler(data.euler.value)) as GameObject;
                                    obj.GetComponent<BuildObject>().BuildID = data.BuildID;
                                    obj.GetComponent<BuildObject>().Region = data.region;
                                    obj.GetComponent<BuildObject>().HandlerID = data.HandlerID;
                                    obj.GetComponent<BuildObject>().SetData(data.Data);
                                    obj.transform.localScale = data.scale.value;
                                    AddObject(id, obj);
                                    obj.SetActive(true);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        public static void Save()
        {
            if (!BetterBuildMod.Instance.IsBuildMode || string.IsNullOrWhiteSpace(BetterBuildMod.Instance.CurrentLevel) || IsLoading) return;

            try
            {
                SavedWorld.buildObjects.Clear();
                foreach(var data in BuildObjects)
                {
                    SavedWorld.buildObjects.Add(data.Key, new List<BuildObjectV04>());
                    foreach(var obj in data.Value)
                    {
                        SavedWorld.buildObjects[data.Key].Add(new BuildObjectV04()
                        {
                            pos = new MonomiPark.SlimeRancher.Persist.Vector3V02() { value = obj.transform.position },
                            euler = new MonomiPark.SlimeRancher.Persist.Vector3V02() { value = obj.transform.eulerAngles },
                            scale = new MonomiPark.SlimeRancher.Persist.Vector3V02() { value = obj.transform.localScale },
                            region = obj.GetComponent<BuildObject>().Region,
                            BuildID = obj.GetComponent<BuildObject>().BuildID,
                            HandlerID = obj.GetComponent<BuildObject>().HandlerID,
                            Data = obj.GetComponent<BuildObject>().GetData()
                        });
                    }
                }

                if(File.Exists(Path.Combine(BetterBuildMod.ModDataPath, BetterBuildMod.Instance.CurrentLevel + ".world")))
                {
                    if(!Directory.Exists(Path.Combine(BetterBuildMod.ModDataPath, "Backups", BetterBuildMod.Instance.CurrentLevel)))
                    {
                        Directory.CreateDirectory(Path.Combine(BetterBuildMod.ModDataPath, "Backups", BetterBuildMod.Instance.CurrentLevel));
                    }
                    int id = 0;
                    string filename = BetterBuildMod.Instance.CurrentLevel + "_" + id + ".world";
                    while(File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "Backups", BetterBuildMod.Instance.CurrentLevel, filename)))
                    {
                        id++;
                        filename = BetterBuildMod.Instance.CurrentLevel + "_" + id + ".world";
                    }
                    File.Copy(Path.Combine(BetterBuildMod.ModDataPath, BetterBuildMod.Instance.CurrentLevel + ".world"), Path.Combine(BetterBuildMod.ModDataPath, "Backups", BetterBuildMod.Instance.CurrentLevel, filename));
                    foreach (var fi in new DirectoryInfo(Path.Combine(BetterBuildMod.ModDataPath, "Backups", BetterBuildMod.Instance.CurrentLevel)).GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(5))
                        fi.Delete();
                }

                using (FileStream stream = new FileStream(Path.Combine(BetterBuildMod.ModDataPath, BetterBuildMod.Instance.CurrentLevel + ".world"), FileMode.Create, FileAccess.Write))
                {
                    SavedWorld.Write(stream);
                }
            }
            catch { }
        }

        public static void AddObject(uint id, GameObject obj)
        {
            List<Region> regions = new List<Region>();
            SRSingleton<SceneContext>.Instance.RegionRegistry.GetContaining(ref regions, obj.transform.position);
            if (!regions.Any())
            {
                ZoneDirector.Zone zone = ZoneDirector.Zone.RANCH;
                if (SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId() == RegionRegistry.RegionSetId.DESERT)
                    zone = ZoneDirector.Zone.DESERT;
                else if (SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId() == RegionRegistry.RegionSetId.SLIMULATIONS)
                    zone = ZoneDirector.Zone.SLIMULATIONS;
                else if (SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId() == RegionRegistry.RegionSetId.VALLEY)
                    zone = ZoneDirector.Zone.VALLEY;
                else if (SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId() == RegionRegistry.RegionSetId.VIKTOR_LAB)
                    zone = ZoneDirector.Zone.VIKTOR_LAB;
                var newRoot = SRSingleton<ZoneEditor>.Instance.CreateNewCell(zone, "BetterBuild", new Bounds(obj.transform.position, new Vector3(2000, 5000, 2000)));
            }

            if (BuildObjects.ContainsKey(id))
            {
                BuildObjects[id].Add(obj);
            }
            else
            {
                BuildObjects.Add(id, new List<GameObject>() { obj });
            }
            if (BetterBuildMod.Instance.IsBuildMode)
            {
                ToolbarUI.Instance.UpdateStatus();
            }
        }

        public static void RemoveObject(uint id, GameObject obj)
        {
            if (BuildObjects.ContainsKey(id))
            {
                BuildObjects[id].Remove(obj);
            }
            ToolbarUI.Instance.UpdateStatus();
        }

        public static void RemoveObject(GameObject obj)
        {
            BuildObject buildObj = obj.GetComponent<BuildObject>();
            if (buildObj != null)
            {
                if (BuildObjects.ContainsKey(buildObj.ID))
                {
                    BuildObjects[buildObj.ID].Remove(obj);
                }
            }
            ToolbarUI.Instance.UpdateStatus();
        }
    }
}

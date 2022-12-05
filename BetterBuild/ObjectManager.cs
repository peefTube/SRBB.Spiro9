using BetterBuild.Persistance;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterBuild
{
    public static class ObjectManager
    {
        public static Dictionary<uint, GameObject> BuildObjects { get; private set; }
        public static Dictionary<string, List<uint>> BuildCategories { get; private set; }
        public static Dictionary<uint, BuildObjectsData> BuildObjectsData { get; private set; }
        public static List<uint> WireObjects { get; private set; }

        private static Dictionary<uint, List<Action<GameObject>>> m_ObjectRequests;
        private static GameObject m_BuildObjectsHolder;

        private static string[] m_DefaultRemovedScripts = new string[]
        {
            "ActivateOnProgressRange",
            "DeactivateOnGameMode",
            "DeactivateOnDLCDisabled",
            "PuzzleTeleportLock"
        };

        static ObjectManager()
        {
            m_ObjectRequests = new Dictionary<uint, List<Action<GameObject>>>();
            BuildObjects = new Dictionary<uint, GameObject>();
            BuildObjectsData = new Dictionary<uint, BuildObjectsData>();
            BuildCategories = new Dictionary<string, List<uint>>();
            WireObjects = new List<uint>();

            LoadObjectsFromFile();
        }

        public static void Clear()
        {
            m_ObjectRequests = new Dictionary<uint, List<Action<GameObject>>>();
            BuildObjects = new Dictionary<uint, GameObject>();
            WireObjects.Clear();
            World.BuildObjects.Clear();
            GameObject.Destroy(World.BuildObjectsHolder);
            GameObject.Destroy(m_BuildObjectsHolder);
        }

        private static void LoadObjectsFromFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BetterBuild.buildObjects.txt";
            string json = null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
            if (json == null)
            {
                UnityEngine.Debug.Log("[BetterBuild] Failed to load objects");
                return;
            }

            BuildObjectsData = new Dictionary<uint, BuildObjectsData>();
            BuildCategories = new Dictionary<string, List<uint>>();

            var buildCategories = JsonConvert.DeserializeObject<List<BuildCategoryData>>(json);
            foreach (var category in buildCategories)
            {
                List<uint> objectIDs = new List<uint>();
                foreach (var buildobject in category.Objects)
                {
                    objectIDs.Add(buildobject.ID);
                    BuildObjectsData.Add(buildobject.ID, buildobject);
                }
                BuildCategories.Add(category.Category, objectIDs);
            }
            BuildCategories["Favorites"] = new List<uint>();
            if (File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "favorites.txt")))
            {
                BuildCategories["Favorites"] = JsonConvert.DeserializeObject<List<uint>>(File.ReadAllText(Path.Combine(BetterBuildMod.ModDataPath, "favorites.txt")));
            }
        }

        private static void HandleObject(uint id, GameObject buildObject)
        {
            TeleportDestination destination = buildObject.GetComponentInChildren<TeleportDestination>(true);
            TeleportSource source = buildObject.GetComponentInChildren<TeleportSource>(true);
            if (destination != null)
            {
                destination.teleportDestinationName = "NotSet";
            }
            if (source != null)
            {
                source.activated = false;
                source.activationBlocker = null;
                source.waitForExternalActivation = false;
                source.activationProgress = ProgressDirector.ProgressType.NONE;
                source.blockingGenerator = null;
                source.destinationSetName = "NotSet";
            }

            JournalEntry journal = buildObject.GetComponentInChildren<JournalEntry>();
            if (journal != null)
            {
                journal.ensureProgress = new ProgressDirector.ProgressType[0];
            }
        }

        public static GameObject GetObject(uint id)
        {
            if (BuildObjects.TryGetValue(id, out GameObject newObj))
            {
                return newObj;
            }
            else
            {
                if (BuildObjectsData.TryGetValue(id, out BuildObjectsData data))
                {
                    GameObject worldObject = GameObject.Find(data.Path);
                    if (worldObject != null)
                    {
                        return LoadWorldObject(data, worldObject);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[BetterBuild] Could not load {data.ID} because path {data.Path} does not exist");
                    }
                }
                else
                {
                    Debug.Log("[BetterBuild] Failed to load object ID " + id);
                }
                return null;
            }
        }

        private static GameObject LoadWorldObject(BuildObjectsData data, GameObject worldObject)
        {
            if (m_BuildObjectsHolder == null)
            {
                m_BuildObjectsHolder = new GameObject("BuildObjects");
            }

            if (BuildObjects.TryGetValue(data.ID, out GameObject obj))
            {
                return obj;
            }

            worldObject.SetActive(false);
            GameObject buildObject = GameObject.Instantiate(worldObject, new Vector3(0, -1000, 0), Quaternion.identity, m_BuildObjectsHolder.transform);
            buildObject.AddComponent<BuildObject>().ID = data.ID;
            buildObject.name = data.ID + " " + data.Name;
            worldObject.SetActive(true);

            foreach (var renderer in buildObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.allowOcclusionWhenDynamic = false;
                //var mf = renderer.GetComponent<MeshFilter>();
                //if(mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
                //{
                //    float quality = 0.1f;
                //    var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
                //    meshSimplifier.Initialize(mf.sharedMesh);
                //    meshSimplifier.SimplifyMesh(quality);
                //    mf.sharedMesh = meshSimplifier.ToMesh();
                //}
            }

            foreach (MonoBehaviour script in buildObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                foreach (var defaultRemove in m_DefaultRemovedScripts)
                {
                    if (script.GetType().ToString().Equals(defaultRemove, StringComparison.CurrentCultureIgnoreCase))
                    {
                        UnityEngine.Object.Destroy(script);
                    }
                }
                foreach (var scriptName in data.RemoveScripts)
                {
                    if (script.GetType().ToString().Equals(scriptName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        UnityEngine.Object.Destroy(script);
                    }
                }
            }

            HandleObject(data.ID, buildObject);

            BuildObjects.Add(data.ID, buildObject);

            return buildObject;
        }

        public static void RequestObject(uint id, Action<GameObject> callback)
        {
            if (BuildObjects.TryGetValue(id, out GameObject obj))
            {
                callback(obj);
            }
            else
            {
                if (m_ObjectRequests.ContainsKey(id))
                {
                    m_ObjectRequests[id].Add(callback);
                }
                else
                {
                    m_ObjectRequests.Add(id, new List<Action<GameObject>>() { callback });
                }
            }
        }

        public static void UpdateRequests()
        {
            if (m_ObjectRequests.Count <= 0) return;

            var request = m_ObjectRequests.First();

            if (BuildObjectsData.TryGetValue(request.Key, out BuildObjectsData data))
            {
                GameObject worldObject = GameObject.Find(data.Path);
                if (worldObject != null)
                {
                    var buildObject = LoadWorldObject(data, worldObject);

                    foreach (var callback in request.Value)
                    {
                        callback(buildObject);
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"[BetterBuild] Could not load {data.ID} because path {data.Path} does not exist");
                    foreach (var callback in request.Value)
                    {
                        callback(null);
                    }
                }
            }
            else
            {
                Debug.Log("[BetterBuild] Failed to load object ID " + request.Key);
                foreach (var callback in request.Value)
                {
                    callback(null);
                }
            }

            m_ObjectRequests.Remove(request.Key);
        }

        private static void SetStatic(GameObject obj)
        {
            obj.isStatic = false;
            foreach (Transform child in obj.transform)
            {
                SetStatic(child.gameObject);
            }
        }
    }
}

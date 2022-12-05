using MonomiPark.SlimeRancher.Regions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class MapConverter
    {
        public static List<OldBuildObject> AllBuildObjects = new List<OldBuildObject>();
        public static List<object> Gordos = new List<object>();
        public static Dictionary<int, GameObject> m_Objs = new Dictionary<int, GameObject>();
        public static Dictionary<uint, GameObject> m_ObjsID = new Dictionary<uint, GameObject>();
        private static List<string> ExcludeObjects = new List<string>() { "Audio", "Colliders", "Collider", "Lights", "Loot", "Slime Nodes" };

        public static int OldObjectCount = 0;

        public static void Load()
        {
            var m_BuildCategories = JsonConvert.DeserializeObject<List<BuildCategoryData>>(File.ReadAllText("buildObjects.txt"));

            foreach (BuildCategoryData categoryData in m_BuildCategories)
            {
                foreach(BuildObjectsData objectsData in categoryData.Objects)
                {
                    GameObject previewObj = GameObject.Find(objectsData.Path);
                    if (previewObj != null)
                    {
                        MeshFilter[] meshFilters = previewObj.GetComponentsInChildren<MeshFilter>();
                        if(meshFilters != null && meshFilters.Length > 0)
                        {
                            string text = "";
                            for (int m = 0; m < meshFilters.Length; m++)
                            {
                                if (meshFilters[m].sharedMesh == null) continue;
                                text += meshFilters[m].sharedMesh.name;
                                if (meshFilters[m].GetComponent<Renderer>() != null)
                                {
                                    if (meshFilters[m].GetComponent<Renderer>().sharedMaterial == null) continue;
                                    text = text + "-" + meshFilters[m].GetComponent<Renderer>().sharedMaterial.name;
                                }
                            }

                            if(text != "")
                            {
                                if(!m_Objs.ContainsKey(text.GetHashCode()))
                                    m_Objs.Add(text.GetHashCode(), previewObj);
                                if (!m_ObjsID.ContainsKey(objectsData.ID))
                                    m_ObjsID.Add(objectsData.ID, previewObj);
                            }
                        }
                    }
                }
            }
        }

        public static void LoadWorld(string worldname)
        {
            Load();
            OldObjectCount = 0;

            try
            {
                List<NewObjectData> result = new List<NewObjectData>();
                using (FileStream fileStream = new FileStream(BetterBuildMod.ModDataPath + "/BB Worlds/" + worldname + ".world", FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fileStream);
                    result = LoadData(reader);
                    fileStream.Close();
                }

                Debug.Log("Found " + result.Count + " Objects");

                foreach (NewObjectData dat in result)
                {
                    bool flag2 = dat == null;
                    if (!flag2)
                    {
                        if (m_ObjsID.TryGetValue(dat.Identifier, out GameObject prefab))
                        {
                            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab, dat.Position, Quaternion.Euler(dat.Rotation));
                            gameObject.transform.localScale = dat.Scale;
                        }
                        else
                        {
                            Debug.Log("Cant find object " + dat.Identifier + ": " + dat.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Could not Load World: " + ex.ToString());
            }
        }

        public static void LoadOldWorld(string worldname)
        {
            Load();
            OldObjectCount = 0;

            try
            {
                List<ObjectData> result = new List<ObjectData>();
                using (FileStream fileStream = new FileStream(BetterBuildMod.ModDataPath + "/BB Worlds/" + worldname + ".world", FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fileStream);
                    result = LoadOldData(reader);
                    fileStream.Close();
                }

                Debug.Log("Found " + result.Count + " Objects");

                foreach (ObjectData dat in result)
                {
                    bool flag2 = dat == null;
                    if (!flag2)
                    {
                        if (m_Objs.TryGetValue(dat.Identifier, out GameObject prefab))
                        {
                            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab, dat.Position, Quaternion.Euler(dat.Rotation));
                        }
                        else
                        {
                            Debug.Log("Cant find object " + dat.Identifier + ": " + dat.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Could not Load World: " + ex.ToString());
            }
        }

        private static List<ObjectData> LoadOldData(BinaryReader reader)
        {
            List<ObjectData> list = new List<ObjectData>();
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                uint id = reader.ReadUInt32();
                int identifier = reader.ReadInt32();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector3 rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector3 scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                list.Add(new ObjectData
                {
                    Identifier = identifier,
                    Position = position,
                    Scale = scale,
                    Rotation = rotation,
                    ID = id
                });
            }
            return list;
        }

        private static List<NewObjectData> LoadData(BinaryReader reader)
        {
            List<NewObjectData> list = new List<NewObjectData>();
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                uint id = reader.ReadUInt32();
                uint identifier = reader.ReadUInt32();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector3 rotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector3 scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                list.Add(new NewObjectData
                {
                    Identifier = identifier,
                    Position = position,
                    Scale = scale,
                    Rotation = rotation,
                    ID = id
                });
            }
            return list;
        }

        public static string[] m_ExcludedCategories = new string[]
        {
            "Resources",
            "Slimes",
            "FX",
            "Lights",
            "Water",
            "Colliders",
            "Loot",
            "Audio",
            "Build Sites",
            "Interactives"
        };
        public static void CheckForNewFolders()
        {
            BuildObjectsSearchData searchData = JsonConvert.DeserializeObject<BuildObjectsSearchData>(File.ReadAllText("searchData.txt"));

            using (StreamWriter writer = new StreamWriter("searchResult.txt"))
            {
                foreach (var cell in Resources.FindObjectsOfTypeAll<CellDirector>())
                {
                    string path = $"{cell.zoneDirector.name}/{cell.name}/Sector/";
                    var sector = GameObject.Find(path);
                    if(sector != null)
                    {
                        foreach(Transform child in sector.transform)
                        {
                            bool has = false;
                            foreach (var s in m_ExcludedCategories)
                            {
                                if (child.name.ToLower().Contains(s.ToLower()))
                                {
                                    has = true;
                                }
                            }
                            if (has) continue;

                            var childPath = path + child.name;
                            if(!searchData.IncludeFolder.Contains(childPath) && !searchData.ExcludePrefab.Contains(childPath) && !searchData.IncludePrefab.Contains(childPath) && !searchData.IgnoreList.Contains(childPath))
                            {
                                writer.WriteLine($"\"{childPath}\",");
                            }
                            foreach (Transform upgrade in child)
                            {
                                var upgradePath = childPath + "/" + upgrade.name;
                                if (upgrade.name.ToLower().Contains("_lv") && !searchData.IncludeFolder.Contains(upgradePath) && !searchData.ExcludePrefab.Contains(upgradePath) && !searchData.IncludePrefab.Contains(upgradePath) && !searchData.IgnoreList.Contains(upgradePath))
                                {
                                    writer.WriteLine($"\"{upgradePath}\",");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static List<BuildCategoryData> m_BuildCategories = new List<BuildCategoryData>();
        public static void GetAndSaveAllObjects()
        {
            BuildObjectsSearchData searchData = JsonConvert.DeserializeObject<BuildObjectsSearchData>(File.ReadAllText("searchData.txt"));

            List<int> knownObjects = new List<int>();
            uint buildObjectID = 1;
            foreach (var folder in searchData.IncludeFolder)
            {
                GameObject obj = GameObject.Find(folder);
                string[] pathData = folder.Split('/');
                string category = "Unknown";
                for (int i = 0; i < pathData.Length; i++)
                {
                    if (pathData[i] == "Sector" && pathData.Length > i + 1)
                    {
                        category = pathData[i + 1];
                        break;
                    }
                }

                BuildCategoryData categoryData = m_BuildCategories.SingleOrDefault(c => c.Category.Equals(category, StringComparison.CurrentCultureIgnoreCase));
                if (categoryData == null)
                {
                    categoryData = new BuildCategoryData()
                    {
                        Category = category,
                        Objects = new List<BuildObjectsData>()
                    };
                    m_BuildCategories.Add(categoryData);
                }

                if (obj == null)
                {
                    Debug.Log($"[BetterBuild] Can not find objects folder '{folder}'");
                }
                else
                {
                    foreach (Transform child in obj.transform)
                    {
                        if (searchData.ExcludePrefab.Contains(GetGameObjectPath(child))) continue;

                        int prefabID = -1;
                        Transform prefabTransform = child;
                        if (obj.transform.childCount > 0)
                        {
                            MeshFilter[] prefabMeshFilter = prefabTransform.GetComponentsInChildren<MeshFilter>();
                            Renderer[] prefabRenderer = prefabTransform.GetComponentsInChildren<Renderer>();

                            if (prefabMeshFilter != null && prefabRenderer != null && prefabMeshFilter.Length > 0 && prefabRenderer.Length > 0)
                            {
                                string nameStuff = "";
                                foreach (var mesh in prefabMeshFilter)
                                {
                                    if (mesh.sharedMesh == null) continue;
                                    nameStuff += mesh.sharedMesh.name;
                                }
                                foreach (var rend in prefabRenderer)
                                {
                                    if (rend.material == null) continue;
                                    nameStuff += rend.material.name;
                                }

                                if (nameStuff == "") continue;

                                prefabID = nameStuff.GetHashCode();

                                if (knownObjects.Contains(prefabID))
                                {
                                    continue;
                                }
                                knownObjects.Add(prefabID);
                            }
                        }
                        else
                        {
                            MeshFilter prefabMeshFilter = prefabTransform.GetComponent<MeshFilter>();
                            Renderer prefabRenderer = prefabTransform.GetComponent<Renderer>();
                            if (prefabRenderer != null && prefabMeshFilter != null)
                            {
                                prefabID = (prefabMeshFilter.sharedMesh.name + prefabRenderer.material.name).GetHashCode();

                                if (knownObjects.Contains(prefabID))
                                {
                                    continue;
                                }
                                knownObjects.Add(prefabID);
                            }
                        }

                        if (prefabID == -1) continue;

                        Region region = child.GetComponentInParent<Region>();

                        BuildObjectsData buildObject = new BuildObjectsData()
                        {
                            ID = buildObjectID++,
                            Name = child.name,
                            Path = GetGameObjectPath(child),
                            RemoveScripts = new List<string>(),
                            RenderID = prefabID
                        };
                        categoryData.Objects.Add(buildObject);
                    }
                }
            }
            foreach (var prefab in searchData.IncludePrefab)
            {
                GameObject obj = GameObject.Find(prefab);
                string[] pathData = prefab.Split('/');
                string category = "Unknown";
                for (int i = 0; i < pathData.Length; i++)
                {
                    if (pathData[i] == "Sector" && pathData.Length > i + 1)
                    {
                        category = pathData[i + 1];
                        break;
                    }
                }

                BuildCategoryData categoryData = m_BuildCategories.SingleOrDefault(c => c.Category.Equals(category, StringComparison.CurrentCultureIgnoreCase));
                if (categoryData == null)
                {
                    categoryData = new BuildCategoryData()
                    {
                        Category = category,
                        Objects = new List<BuildObjectsData>()
                    };
                    m_BuildCategories.Add(categoryData);
                }

                if (obj == null)
                {
                    Debug.Log($"[BetterBuild] Can not find objects prefab '{prefab}'");
                }
                else
                {
                    int prefabID = -1;
                    Transform prefabTransform = obj.transform;
                    if (obj.transform.childCount > 0)
                    {
                        MeshFilter[] prefabMeshFilter = prefabTransform.GetComponentsInChildren<MeshFilter>();
                        Renderer[] prefabRenderer = prefabTransform.GetComponentsInChildren<Renderer>();

                        if (prefabMeshFilter != null && prefabRenderer != null && prefabMeshFilter.Length > 0 && prefabRenderer.Length > 0)
                        {
                            string nameStuff = "";
                            foreach (var mesh in prefabMeshFilter)
                            {
                                if (mesh.sharedMesh == null) continue;
                                nameStuff += mesh.sharedMesh.name;
                            }
                            foreach (var rend in prefabRenderer)
                            {
                                if (rend.material == null) continue;
                                nameStuff += rend.material.name;
                            }

                            if (nameStuff == "") continue;

                            prefabID = nameStuff.GetHashCode();

                            if (knownObjects.Contains(prefabID))
                            {
                                continue;
                            }
                            knownObjects.Add(prefabID);
                        }
                    }
                    else
                    {
                        MeshFilter prefabMeshFilter = prefabTransform.GetComponent<MeshFilter>();
                        Renderer prefabRenderer = prefabTransform.GetComponent<Renderer>();
                        if (prefabRenderer != null && prefabMeshFilter != null)
                        {
                            prefabID = (prefabMeshFilter.sharedMesh.name + prefabRenderer.material.name).GetHashCode();

                            if (knownObjects.Contains(prefabID))
                            {
                                continue;
                            }
                            knownObjects.Add(prefabID);
                        }
                    }

                    if (prefabID == -1) continue;

                    Region region = obj.transform.GetComponentInParent<Region>();

                    BuildObjectsData buildObject = new BuildObjectsData()
                    {
                        ID = buildObjectID++,
                        Name = obj.name,
                        Path = GetGameObjectPath(obj.transform),
                        RemoveScripts = new List<string>(),
                        RenderID = prefabID
                    };
                    categoryData.Objects.Add(buildObject);
                }
            }

            string json = JsonConvert.SerializeObject(m_BuildCategories, Formatting.Indented);
            File.WriteAllText("buildObjects-new.txt", json);
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        // Token: 0x0200002B RID: 43
        public class ObjectData
        {
            // Token: 0x040000B9 RID: 185
            public Vector3 Position;

            // Token: 0x040000BA RID: 186
            public Vector3 Scale;

            // Token: 0x040000BB RID: 187
            public Vector3 Rotation;

            // Token: 0x040000BC RID: 188
            public uint ID;

            // Token: 0x040000BD RID: 189
            public int Identifier;
        }

        // Token: 0x0200002B RID: 43
        public class NewObjectData
        {
            // Token: 0x040000B9 RID: 185
            public Vector3 Position;

            // Token: 0x040000BA RID: 186
            public Vector3 Scale;

            // Token: 0x040000BB RID: 187
            public Vector3 Rotation;

            // Token: 0x040000BC RID: 188
            public uint ID;

            // Token: 0x040000BD RID: 189
            public uint Identifier;
        }

        public static GameObject FindGameobject(string name, int hash)
        {
            if (name == "CustomField")
            {

            }
            else if (name.StartsWith("gordo"))
            {
                foreach (object gobj in Gordos)
                    if (((OldBuildObject)gobj).ObjName == name)
                        return ((OldBuildObject)gobj).Object;
            }
            foreach (OldBuildObject obj in AllBuildObjects)
            {
                if (obj == null)
                    continue;

                if (obj.ObjName == name && obj.ObjHash == hash)
                    return obj.Object;
            }
            return null;
        }
    }

    public class OldBuildObject
    {
        public string CustomName;
        public string Name
        {
            get
            {
                return (CustomName.Length > 0 ? CustomName : ObjName);
            }
            set
            {
                CustomName = value;
            }
        }
        public string ObjName;
        public int ObjHash;
        public GameObject Object;

        public OldBuildObject(string customname, string objname, int objhash, GameObject obj)
        {
            this.CustomName = customname;
            this.ObjName = objname;
            this.ObjHash = objhash;
            this.Object = obj;
        }
    }
}

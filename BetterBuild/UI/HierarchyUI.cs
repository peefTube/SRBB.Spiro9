using BetterBuild.Persistance;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BetterBuild.UI
{
    public class HierarchyUI : SRSingleton<HierarchyUI>
    {
        private GameObject m_Gameobject;
        private Dictionary<uint, Texture2D> m_BuildObjectsPreview;
        private ScrollRect m_CategoryScroll;
        private ScrollRect m_ObjectsScroll;
        private InputField m_SearchInput;

        private void Start()
        {
            if (!Directory.Exists(Path.Combine(BetterBuildMod.ModDataPath, "Textures")))
                Directory.CreateDirectory(Path.Combine(BetterBuildMod.ModDataPath, "Textures"));

            m_Gameobject = transform.Find("Hierarchy").gameObject;
            m_CategoryScroll = transform.Find("Hierarchy/CategoryScroll").GetComponent<ScrollRect>();
            m_ObjectsScroll = transform.Find("Hierarchy/ObjectsScroll").GetComponent<ScrollRect>();
            m_SearchInput = transform.Find("Hierarchy/SearchInput").GetComponent<InputField>();

            m_BuildObjectsPreview = new Dictionary<uint, Texture2D>();
            for (int i = 0; i < m_CategoryScroll.content.childCount; i++)
            {
                Transform child = m_CategoryScroll.content.GetChild(i);
                Destroy(child.gameObject);
            }
            for (int i = 0; i < m_ObjectsScroll.content.childCount; i++)
            {
                Transform child = m_ObjectsScroll.content.GetChild(i);
                Destroy(child.gameObject);
            }

            m_SearchInput.onValueChanged.AddListener(Search);

            {
                GameObject categoryObj = Instantiate(Globals.CategoryButtonPrefab, m_CategoryScroll.content, false);

                categoryObj.GetComponentInChildren<Button>().onClick.AddListener(() => SelectCategory("Favorites"));
                categoryObj.GetComponentInChildren<Text>().text = "Favorites";
            }

            foreach (string categoryName in ObjectManager.BuildCategories.Keys)
            {
                GameObject categoryObj = Instantiate(Globals.CategoryButtonPrefab, m_CategoryScroll.content, false);

                categoryObj.GetComponentInChildren<Button>().onClick.AddListener(() => SelectCategory(categoryName));
                categoryObj.GetComponentInChildren<Text>().text = categoryName;
            }

            SelectCategory("Favorites");
        }

        public void SetActive(bool active)
        {
            m_Gameobject.SetActive(active);
        }

        private void Search(string term)
        {
            for (int i = 0; i < m_ObjectsScroll.content.childCount; i++)
            {
                Transform child = m_ObjectsScroll.content.GetChild(i);
                Destroy(child.gameObject);
            }

            if (term.Length < 2) return;

            foreach (var buildObject in ObjectManager.BuildObjectsData.Values.ToList())
            {
                if(buildObject.Name.ToLower().Contains(term.ToLower()))
                {
                    var objectID = buildObject.ID;
                    var objectName = buildObject.Name;

                    GameObject buildObj = Instantiate(Globals.ObjectButtonPrefab, m_ObjectsScroll.content, false);
                    buildObj.GetComponentInChildren<Button>().onClick.AddListener(() => SpawnObject(objectID));
                    buildObj.GetComponentInChildren<Text>().text = objectName;
                    var favorite = buildObj.transform.Find("Favorite");
                    favorite.GetComponent<Image>().color = ObjectManager.BuildCategories["Favorites"].Contains(objectID) ? Color.yellow : Color.gray;
                    favorite.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (!ObjectManager.BuildCategories["Favorites"].Contains(objectID))
                        {
                            ObjectManager.BuildCategories["Favorites"].Add(objectID);
                            favorite.GetComponent<Image>().color = Color.yellow;
                        }
                        else
                        {
                            ObjectManager.BuildCategories["Favorites"].Remove(objectID);
                            favorite.GetComponent<Image>().color = Color.gray;
                        }
                        File.WriteAllText(Path.Combine(BetterBuildMod.ModDataPath, "favorites.txt"), JsonConvert.SerializeObject(ObjectManager.BuildCategories["Favorites"]));
                    });

                    if (!m_BuildObjectsPreview.ContainsKey(objectID))
                    {
                        if (File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg")))
                        {
                            byte[] bytes = File.ReadAllBytes(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg"));

                            var result = new Texture2D(64, 64, TextureFormat.RGB24, false);
                            result.LoadImage(bytes);
                            result.Apply(false, false);

                            buildObj.GetComponentInChildren<RawImage>().texture = result;

                            m_BuildObjectsPreview.Add(objectID, result);
                        }
                        else
                        {
                            ObjectManager.RequestObject(objectID, (previewObj) =>
                            {
                                if (previewObj == null) return;
                                if (buildObj == null) return;

                                Texture2D texture = RuntimePreviewGenerator.GenerateModelPreview(previewObj.transform, 64, 64, true);

                                byte[] bytes = texture.EncodeToPNG();
                                File.WriteAllBytes(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg"), bytes);

                                buildObj.GetComponentInChildren<RawImage>().texture = texture;

                                m_BuildObjectsPreview.Add(objectID, texture);
                            });
                        }
                    }
                    else
                    {
                        buildObj.GetComponentInChildren<RawImage>().texture = m_BuildObjectsPreview[objectID];
                    }
                }
            }
        }

        private void SelectCategory(string categoryName)
        {
            if (ObjectManager.BuildCategories.TryGetValue(categoryName, out List<uint> categoryObjects))
            {
                for (int i = 0; i < m_ObjectsScroll.content.childCount; i++)
                {
                    Transform child = m_ObjectsScroll.content.GetChild(i);
                    Destroy(child.gameObject);
                }

                foreach (var obj in categoryObjects)
                {
                    if (!ObjectManager.BuildObjectsData.ContainsKey(obj)) continue;

                    var objectID = obj;
                    var objectName = ObjectManager.BuildObjectsData[objectID].Name;

                    GameObject buildObj = Instantiate(Globals.ObjectButtonPrefab, m_ObjectsScroll.content, false);
                    buildObj.GetComponentInChildren<Button>().onClick.AddListener(() => SpawnObject(objectID));
                    buildObj.GetComponentInChildren<Text>().text = objectName;
                    var favorite = buildObj.transform.Find("Favorite");
                    favorite.GetComponent<Image>().color = ObjectManager.BuildCategories["Favorites"].Contains(objectID) ? Color.yellow : Color.gray;
                    favorite.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if(!ObjectManager.BuildCategories["Favorites"].Contains(objectID))
                        {
                            ObjectManager.BuildCategories["Favorites"].Add(objectID);
                            favorite.GetComponent<Image>().color = Color.yellow;
                        }
                        else
                        {
                            ObjectManager.BuildCategories["Favorites"].Remove(objectID);
                            favorite.GetComponent<Image>().color = Color.gray;
                        }
                        File.WriteAllText(Path.Combine(BetterBuildMod.ModDataPath, "favorites.txt"), JsonConvert.SerializeObject(ObjectManager.BuildCategories["Favorites"]));
                        if (categoryName.Equals("Favorites"))
                        {
                            Destroy(buildObj);
                        }
                    });

                    if (!m_BuildObjectsPreview.ContainsKey(objectID))
                    {
                        if (File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg")))
                        {
                            byte[] bytes = File.ReadAllBytes(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg"));

                            var result = new Texture2D(64, 64, TextureFormat.RGB24, false);
                            result.LoadImage(bytes);
                            result.Apply(false, false);

                            buildObj.GetComponentInChildren<RawImage>().texture = result;

                            m_BuildObjectsPreview.Add(objectID, result);
                        }
                        else
                        {
                            ObjectManager.RequestObject(objectID, (previewObj) =>
                            {
                                if (previewObj == null) return;
                                if (buildObj == null) return;

                                Texture2D texture = RuntimePreviewGenerator.GenerateModelPreview(previewObj.transform, 64, 64, true);

                                byte[] bytes = texture.EncodeToPNG();
                                File.WriteAllBytes(Path.Combine(BetterBuildMod.ModDataPath, "Textures", objectID + ".jpg"), bytes);

                                buildObj.GetComponentInChildren<RawImage>().texture = texture;

                                m_BuildObjectsPreview.Add(objectID, texture);
                            });
                        }
                    }
                    else
                    {
                        buildObj.GetComponentInChildren<RawImage>().texture = m_BuildObjectsPreview[objectID];
                    }
                }
            }
        }

        private void SpawnObject(uint objectID)
        {
            ObjectManager.RequestObject(objectID, (buildObject) =>
            {
                if (buildObject == null) return;

                GameObject obj = Instantiate(buildObject, BetterBuildCamera.Instance.transform.position + (BetterBuildCamera.Instance.transform.forward * 10), Quaternion.identity) as GameObject;
                obj.GetComponent<BuildObject>().BuildID = World.LastBuildID++;
                obj.GetComponent<BuildObject>().Region = SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId();
                World.AddObject(objectID, obj);
                obj.SetActive(true);

                UndoManager.RegisterStates(new IUndo[] { new UndoSelection(), new UndoInstantiate(objectID, obj) }, "Create new Object");
                ObjectSelection.Instance.SetSelection(obj);
            });
        }
    }
}

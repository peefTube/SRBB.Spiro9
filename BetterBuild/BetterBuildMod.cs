using BetterBuild.Initializers;
using BetterBuild.Models;
using BetterBuild.Persistance;
using BetterBuild.UI;
using MonomiPark.SlimeRancher.Regions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BetterBuild
{
    public class BetterBuildMod : MonoBehaviour
    {
        public static BetterBuildMod Instance { get; private set; }
        public static string ModDataPath { get { return Path.Combine(Application.dataPath, "..", "BetterBuild"); } }
        
        private Version m_Version;

        public bool IsBuildMode;
        public string CurrentLevel;
        
        public void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            //if(args.Contains("-console"))
            //{
            //    gameObject.AddComponent<BBConsole>();
            //}
            m_Version = Assembly.GetExecutingAssembly().GetName().Version;
            
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            StartCoroutine(LoadAssetBundleData());
        }

        private void Update()
        {
            if(SceneManager.GetActiveScene().buildIndex == 2)
            {
                var watermark = GameObject.Find("Watermark");
                if(watermark != null)
                {
                    if (!watermark.GetComponent<TMPro.TextMeshProUGUI>().text.Contains("BetterBuild"))
                    {
                        watermark.GetComponent<TMPro.TextMeshProUGUI>().text += "\nBetterBuild v" + m_Version.Revision;
                    }
                }
            }
            ObjectManager.UpdateRequests();
        }

        public IEnumerator LoadAssetBundleData()
        {
            var myLoadedAssetBundle = AssetBundle.LoadFromMemory(Utils.ExtractResource("BetterBuild.betterbuild.dat"));
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                yield return null;
            }

            // Save asset bundle data
            Globals.ToolbarPrefab = myLoadedAssetBundle.LoadAsset<GameObject>("BetterBuildToolbar");
            Globals.HierarchyPrefab = myLoadedAssetBundle.LoadAsset<GameObject>("BetterBuildHierarchy");
            Globals.CategoryButtonPrefab = myLoadedAssetBundle.LoadAsset<GameObject>("CategoryButton");
            Globals.ObjectButtonPrefab = myLoadedAssetBundle.LoadAsset<GameObject>("ObjectButton");
            Globals.TooltipPrefab = myLoadedAssetBundle.LoadAsset<GameObject>("Tooltip");

            Globals.InspectorVector3 = myLoadedAssetBundle.LoadAsset<GameObject>("InspectorVector3");
            Globals.InspectorInput = myLoadedAssetBundle.LoadAsset<GameObject>("InspectorInput");

            Globals.ConeMesh = myLoadedAssetBundle.LoadAsset<Mesh>("ConeSoftEdges");
            Globals.CubeMesh = myLoadedAssetBundle.LoadAsset<Mesh>("Cube");

            Globals.HandleOpaqueMaterial = myLoadedAssetBundle.LoadAsset<Material>("HandleOpaqueMaterial");
            Globals.HandleRotateMaterial = myLoadedAssetBundle.LoadAsset<Material>("HandleRotateMaterial");
            Globals.HandleTransparentMaterial = myLoadedAssetBundle.LoadAsset<Material>("HandleTransparentMaterial");

            Globals.HighlightMaterial = myLoadedAssetBundle.LoadAsset<Material>("Highlight");
            Globals.UnlitVertexColorMaterial = myLoadedAssetBundle.LoadAsset<Material>("UnlitVertexColor");
            Globals.WireframeMaterial = myLoadedAssetBundle.LoadAsset<Material>("Wireframe");

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (Globals.ToolbarPrefab == null)
                Debug.Log("Could not load BetterBuildToolbar");
            if (Globals.ConeMesh == null)
                Debug.Log("Could not load ConeSoftEdges");
            if (Globals.CubeMesh == null)
                Debug.Log("Could not load Cube");
            if (Globals.HierarchyPrefab == null)
                Debug.Log("Could not load BetterBuildHierarchy");
            // Unload asset bundle
            myLoadedAssetBundle.Unload(false);
        }

        public void LoadLevel(string level, bool buildmode)
        {
            IsBuildMode = buildmode;
            CurrentLevel = level;

            if (buildmode)
            {
                SRSingleton<GameContext>.Instance.AutoSaveDirector.LoadNewGame(level, Identifiable.Id.BEACH_BALL_TOY, PlayerState.GameMode.CLASSIC, delegate
                {

                });
            }
            else
            {
                GameData.Summary saveToContinue = SRSingleton<GameContext>.Instance.AutoSaveDirector.GetSaveToContinue();
                SRSingleton<GameContext>.Instance.AutoSaveDirector.BeginLoad(saveToContinue.name, saveToContinue.saveName, delegate
                {
                        
                });
            }
        }

        private string levelname;
        private string error;
        private void OnGUI()
        {
            if(SceneManager.GetActiveScene().buildIndex == 2)
            {
                GUILayout.Label("BetterBuild v" + m_Version.Revision);
                foreach (var file in Directory.GetFiles(ModDataPath, "*.world"))
                {
                    var levelname = Path.GetFileNameWithoutExtension(file);
                    if (GUILayout.Button("Load " + levelname))
                    {
                        LoadLevel(levelname, true);
                    }
                    if (GUILayout.Button("Continue " + levelname))
                    {
                        LoadLevel(levelname, false);
                    }
                }
                GUILayout.Space(20);
                if (string.IsNullOrWhiteSpace(error))
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Levelname:");
                    levelname = GUILayout.TextField(levelname);
                    GUILayout.EndVertical();
                    if (GUILayout.Button("Create new Level"))
                    {
                        var fileName = levelname + ".world";
                        var isValid = !string.IsNullOrEmpty(fileName) &&
                                  fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                                  !File.Exists(Path.Combine(ModDataPath, fileName));
                        if (isValid)
                        {
                            LoadLevel(levelname, true);
                        }
                        else
                        {
                            error = "Invalid Levelname or already exists";
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Error creating level: " + error);
                    if(GUILayout.Button("Ok"))
                    {
                        error = null;
                    }
                }
            }
            else if(SceneManager.GetActiveScene().buildIndex == 3)
            {
               
            }
        }
        
        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ObjectManager.Clear();

            if(scene.buildIndex == 2)
            {
                IsBuildMode = false;
                CurrentLevel = null;

                //GameObject obj = FindObjectOfType<GameContext>().gameObject;
                //foreach(MonoBehaviour script in obj.GetComponentsInChildren<MonoBehaviour>())
                //{
                //    Debug.Log("Found: " + script.GetType());
                //}
            }
            else if(scene.buildIndex == 3)
            {
                Directory.CreateDirectory(Path.Combine(ModDataPath, "Export"));
                foreach(var c in ObjectManager.BuildObjectsData.Values)
                {
                    ObjExporter.DoExport(GameObject.Find(c.Path), true);
                }
                //MapConverter.CheckForNewFolders();
                //MapConverter.GetAndSaveAllObjects();
                //MapConverter.LoadWorld("Billy_finished_secret_mansion_converted");
            }
        }

        public void LoadWorld()
        {
            StartCoroutine(LoadWorldAfterFrame());
        }

        private IEnumerator LoadWorldAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            World.Load(false);
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}

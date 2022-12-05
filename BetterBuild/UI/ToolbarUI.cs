using BetterBuild.Gizmo;
using BetterBuild.Persistance;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BetterBuild.UI
{
    public class ToolbarUI : SRSingleton<ToolbarUI>
    {
        private List<Button> m_UtilityButtons = new List<Button>();
        private HierarchyUI m_HierarchyUI;

        private Button m_UndoButton;
        private Button m_RedoButton;
        private Text m_StatusText;

        private void Start()
        {
            m_HierarchyUI = FindObjectOfType<HierarchyUI>();

            Button saveButton = GetToolbarButton("Save");
            m_UndoButton = GetToolbarButton("Undo");
            m_RedoButton = GetToolbarButton("Redo");
            Button testButton = GetToolbarButton("Test");

            Button copyButton = GetToolbarButton("Copy");
            Button deleteButton = GetToolbarButton("Delete");
            Button moveButton = GetToolbarButton("Move");
            Button rotateButton = GetToolbarButton("Rotate");
            Button scaleButton = GetToolbarButton("Scale");
            m_UtilityButtons.AddRange(new Button[] { copyButton, deleteButton, moveButton, rotateButton, scaleButton });

            Button levelEditorButton = GetToolbarButton("LevelEditor");
            Button zoneEditorButton = GetToolbarButton("ZoneEditor");
            Button spawnerEditorButton = GetToolbarButton("SpawnerEditor");
            Button teleportButton = GetToolbarButton("Teleport");

            Button settingsButton = GetToolbarButton("Settings");
            Button infoButton = GetToolbarButton("Info");
            Button exitButton = GetToolbarButton("Exit");

            m_StatusText = GetToolbarText("Status");

            saveButton.onClick.AddListener(OnSave);
            m_UndoButton.onClick.AddListener(OnUndo);
            m_RedoButton.onClick.AddListener(OnRedo);
            testButton.onClick.AddListener(OnTest);
            
            m_UndoButton.interactable = false;
            m_RedoButton.interactable = false;

            copyButton.onClick.AddListener(OnCopy);
            deleteButton.onClick.AddListener(OnDelete);
            moveButton.onClick.AddListener(OnMove);
            rotateButton.onClick.AddListener(OnRotate);
            scaleButton.onClick.AddListener(OnScale);

            copyButton.interactable = false;

            levelEditorButton.onClick.AddListener(OnLevelEditor);
            zoneEditorButton.onClick.AddListener(OnZoneEditor);
            spawnerEditorButton.onClick.AddListener(OnSpawnerEditor);
            teleportButton.onClick.AddListener(OnTeleport);
            
            zoneEditorButton.interactable = false;
            spawnerEditorButton.interactable = false;

            zoneEditorButton.gameObject.SetActive(false);
            spawnerEditorButton.gameObject.SetActive(false);

            settingsButton.onClick.AddListener(OnSettings);
            infoButton.onClick.AddListener(OnInfo);
            exitButton.onClick.AddListener(OnExit);

            SetUtilityButtons(false);

            UndoManager.Instance.undoStackModified += OnUndoStackModified;
            UndoManager.Instance.redoStackModified += OnUndoStackModified;

            UpdateStatus();
        }

        private void OnUndoStackModified()
        {
            m_UndoButton.interactable = UndoManager.GetUndoCount() > 0;
            m_RedoButton.interactable = UndoManager.GetRedoCount() > 0;
        }

        public void UpdateStatus()
        {
            if (m_StatusText == null) return;

            m_StatusText.text = World.IsLoading ? $"World is loading...{World.LoadPercentage}%" : $"Loaded Objects: {World.TotalObjects}";
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject != null) return;

            if(Input.GetKey(KeyCode.LeftControl))
            {
                if(Input.GetKeyDown(KeyCode.S))
                {
                    World.Save();
                }
                if(Input.GetKeyDown(KeyCode.Z))
                {
                    OnUndo();
                }
                if(Input.GetKeyDown(KeyCode.Y))
                {
                    OnRedo();
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    ObjectEditor.Instance.DublicateSelectedObjects();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
                GizmoObject.Instance.SetTool(Tool.Position);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                GizmoObject.Instance.SetTool(Tool.Rotate);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                GizmoObject.Instance.SetTool(Tool.Scale);
            //if (Input.GetKeyDown(KeyCode.Q))
            //    ObjectEditor.Instance.SetCurrentTool(ObjectEditor.Tool.Wire);
            if (Input.GetKeyDown(KeyCode.Delete))
                ObjectEditor.Instance.DestroySelectedObjects();
            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    Debug.Log("Spawning");
            //    List<Region> regions = new List<Region>();
            //    SRSingleton<SceneContext>.Instance.RegionRegistry.GetContaining(ref regions, SRSingleton<SceneContext>.Instance.Player.transform.position);
            //    if (regions.Any())
            //    {
            //        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //        cube.transform.position = SRSingleton<SceneContext>.Instance.Player.transform.position;
            //        cube.transform.SetParent(regions[0].root.transform);

            //        GameObject spawnerObj = new GameObject("Spawner");
            //        // Rather use spawner.spawnLocs
            //        spawnerObj.transform.position = SRSingleton<SceneContext>.Instance.Player.transform.position;
            //        spawnerObj.transform.SetParent(regions[0].root.transform);

            //        var templateSpawner = FindObjectOfType<DirectedSlimeSpawner>();
            //        if (templateSpawner != null)
            //        {
            //            var spawner = spawnerObj.AddComponent<DirectedSlimeSpawner>();
            //            spawner.radius = 5f;
            //            spawner.spawnDelayFactor = 1f;
            //            spawner.constraints = new DirectedActorSpawner.SpawnConstraint[]
            //            {
            //        new DirectedActorSpawner.SpawnConstraint()
            //        {
            //            slimeset = new SlimeSet()
            //            {
            //                members = new SlimeSet.Member[]
            //                {
            //                    new SlimeSet.Member()
            //                    {
            //                        prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_BOOM_LARGO),
            //                        weight = 1
            //                    }
            //                },
            //            },
            //            window = new DirectedActorSpawner.TimeWindow()
            //            {
            //                startHour = 0,
            //                endHour = 0,
            //                timeMode = DirectedActorSpawner.TimeMode.ANY
            //            },
            //            feral = false,
            //            weight = 1f,
            //            maxAgitation = false
            //        }
            //            };
            //            spawner.spawnFX = templateSpawner.spawnFX;
            //            spawner.slimeSpawnFX = templateSpawner.slimeSpawnFX;
            //        }
            //        else
            //            Debug.Log("No template spawner");
            //    }
            //}
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(ObjectSelection.Instance.SelectedObjects.Count > 0)
                {
                    UndoManager.RegisterState(new UndoSelection(), "Selection changed");
                    ObjectSelection.Instance.Clear();
                }
            }

            if(Input.GetKeyDown(KeyCode.M))
            {
                if(ObjectEditor.Instance.CurrentTool == ObjectEditor.Tool.MissingObject)
                    ObjectEditor.Instance.SetCurrentTool(ObjectEditor.Tool.Level);
                else
                    ObjectEditor.Instance.SetCurrentTool(ObjectEditor.Tool.MissingObject);
            }
        }

        private void OnSave()
        {
            World.Save();
        }

        private void OnUndo()
        {
            if (UndoManager.GetUndoCount() > 0)
            {
                UndoManager.PerformUndo();
            }
        }

        private void OnRedo()
        {
            if (UndoManager.GetRedoCount() > 0)
            {
                UndoManager.PerformRedo();
            }
        }

        private void OnTest()
        {
            ObjectSelection.Instance.Clear();
            BetterBuildCamera.Instance.SetActive(false);
        }

        private void OnCopy()
        {
            ObjectEditor.Instance.DublicateSelectedObjects();
        }

        private void OnDelete()
        {
            ObjectEditor.Instance.DestroySelectedObjects();
        }

        private void OnMove()
        {
            GizmoObject.Instance.SetTool(Tool.Position);
        }

        private void OnRotate()
        {
            GizmoObject.Instance.SetTool(Tool.Rotate);
        }

        private void OnScale()
        {
            GizmoObject.Instance.SetTool(Tool.Scale);
        }

        private void OnLevelEditor()
        {
            ObjectEditor.Instance.SetCurrentTool(ObjectEditor.Tool.Level);
            m_HierarchyUI.gameObject.SetActive(true);

            m_RegionEditor = false;
        }

        private bool m_RegionEditor;
        private bool m_ShowRegions;
        private List<GameObject> m_RegionObjects = new List<GameObject>();
        private void OnZoneEditor()
        {
            ObjectEditor.Instance.SetCurrentTool(ObjectEditor.Tool.Zone);
            m_HierarchyUI.gameObject.SetActive(false);

            SetUtilityButtons(false);
            ObjectSelection.Instance.Clear();

            m_RegionEditor = true;

            /*
             * Idea for Zone Editing:
             * Create a object for zone selection/scaling
             * After pressing "Apply", add CellDirector and Region component (Maybe region initializer too)
             * For custom music, add RegionBackgroundMusic component
             * Sort childs like original SR and maybe add objects by closest (custom) region
             */
        }

        void OnGUI()
        {
            if (!m_RegionEditor) return;
            GUILayout.Space(30);
            if (GUILayout.Button("Show Regions"))
            {
                if (m_ShowRegions)
                {
                    foreach(var regionRender in m_RegionObjects)
                    {
                        Destroy(regionRender);
                    }
                    m_RegionObjects.Clear();
                }
                else
                {
                    foreach(var cell in SRSingleton<ZoneEditor>.Instance.CustomCells)
                    {
                        GameObject regionRender = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        regionRender.transform.position = cell.region.bounds.center;
                        regionRender.transform.localScale = cell.region.bounds.size;

                        m_RegionObjects.Add(regionRender);
                    }
                }
                m_ShowRegions = !m_ShowRegions;
            }
        }

        private void OnSpawnerEditor()
        {
            m_HierarchyUI.gameObject.SetActive(false);

            SetUtilityButtons(false);
            ObjectSelection.Instance.Clear();

            m_RegionEditor = false;
        }

        private void OnTeleport()
        {
            if(SRSingleton<TeleportUI>.Instance.IsOpen)
            {
                SRSingleton<TeleportUI>.Instance.Close();
            }
            else
            {
                SRSingleton<TeleportUI>.Instance.Open();
            }
        }

        private void OnSettings()
        {
            SRSingleton<SettingsUI>.Instance.Open();
        }

        private void OnInfo()
        {
            SRSingleton<InfoUI>.Instance.Open();
        }

        private void OnExit()
        {
            if (SRSingleton<GameContext>.Instance.AutoSaveDirector.SaveAllNow())
            {
                SRSingleton<SceneContext>.Instance.OnSessionEnded();
                SceneManager.LoadScene("MainMenu");
            }
        }

        private Button GetToolbarButton(string name)
        {
            Transform buttonTransform = transform.Find($"Toolbar/{name}Button");
            if (buttonTransform == null)
            {
                Debug.Log($"Could not find {name}Button");
            }
            return buttonTransform.GetComponent<Button>();
        }

        private Text GetToolbarText(string name)
        {
            Transform buttonTransform = transform.Find($"Toolbar/{name}Text");
            if (buttonTransform == null)
            {
                Debug.Log($"Could not find {name}Text");
            }
            return buttonTransform.GetComponent<Text>();
        }

        public void SetUtilityButtons(bool enabled)
        {
            foreach(var button in m_UtilityButtons)
            {
                button.interactable = enabled;
            }
        }
    }
}

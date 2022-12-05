using BetterBuild.Gizmo;
using BetterBuild.Persistance;
using BetterBuild.UI;
using MonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterBuild
{
    public class ObjectEditor : SRSingleton<ObjectEditor>
    {
        const float MaxObjectDistance = 400f;

        public enum Tool
        {
            Level,
            Wire,
            Zone,
            MissingObject
        }

        public Tool CurrentTool;

        /// Used to simplify handle transformations for multiple selection movements
        private Transform _mTransform;
        private bool _ignoremouse;

        /// Create or retreive the parent transform for handle movements.
        private Transform mTransform
        {
            get
            {
                if (_mTransform == null)
                {
                    _mTransform = new GameObject().transform;
                    _mTransform.name = "Handle Transform Parent";
                }
                return _mTransform;
            }
        }

        /// Where the handle was before the latest interaction began.  In OnHandleMove `transform - handleTransformOrigin` is the delta to move objects.
        GizmoTransform handleTransformOrigin;

        /// Cache of each selected object's transform at the beginning of handle interaction.
        GizmoTransform[] selectionTransformOrigin;
        Region[] allRegions;
        string[] allSectors = new string[]
        {
            "Main Nav",
            "Cliffs",
            "Mountains",
            "Solid Filler",
            "Solid FIller",
            "Rocks",
            "Flora",
            "Grass",
            "Deco",
            "Constructs",
            "Doors",
            "Trees",
            "Crystals",
            "Giant Tree",
            "Cave Roof",
            "Main Nav Internal",
            "Constructions",
            "Ranch Features",
            "Upgrades"
        };
        private bool m_PictureMode;

        private void Start()
        {
            GizmoObject.Instance.OnHandleBegin += OnHandleBegin;
            GizmoObject.Instance.OnHandleMove += OnHandleMove;
            ObjectSelection.Instance.OnSelectionChange += OnSelectionChange;

            dragRectStyle.normal.background = Texture2D.whiteTexture;

            allRegions = Resources.FindObjectsOfTypeAll<Region>();
        }

        private void Update()
        {
            //if(Input.GetKeyDown(KeyCode.L))
            //{
            //    SRSingleton<ZoneEditor>.Instance.CreateNewCell(SRSingleton<SceneContext>.Instance.PlayerZoneTracker.GetCurrentZone(), "testCell", new Bounds(SRSingleton<SceneContext>.Instance.Player.transform.position, Vector3.one * 50));
            //}
            if (SRInput.Instance.GetInputMode() != SRInput.InputMode.NONE) return;

            if (EventSystem.current.currentSelectedGameObject == null && Input.GetKeyDown(KeyCode.P))
            {
                m_PictureMode = !m_PictureMode;
                ObjectSelection.Instance.Clear();
                WireEditor.Instance.DisableWireObjects();
                ToolbarUI.Instance.gameObject.SetActive(m_PictureMode);
                HierarchyUI.Instance.gameObject.SetActive(m_PictureMode);
                SettingsUI.Instance.gameObject.SetActive(m_PictureMode);
                InfoUI.Instance.gameObject.SetActive(m_PictureMode);
                TeleportUI.Instance.gameObject.SetActive(m_PictureMode);
            }

            if (CurrentTool != Tool.Level) return;

            GizmoObject.Instance.UpdateDrag();

            if (Input.GetMouseButtonUp(0))
            {
                if (!_ignoremouse)
                    OnEditorMouseUp();

                _ignoremouse = false;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    _ignoremouse = true;
                }
                else
                {
                    _ignoremouse = false;
                    OnEditorMouseDown();
                }
            }
            else if (Input.GetMouseButton(0))
            {
                OnEditorMouseMove();
            }
        }

        public void SetCurrentTool(Tool tool)
        {
            CurrentTool = tool;
            if(tool == Tool.Level)
            {
                HierarchyUI.Instance.gameObject.SetActive(true);
                WireEditor.Instance.DisableWireObjects();
            }
            else if(tool == Tool.Wire)
            {
                HierarchyUI.Instance.gameObject.SetActive(false);
                WireEditor.Instance.EnableWireObjects();
            }
            else if(tool == Tool.Zone)
            {
                HierarchyUI.Instance.gameObject.SetActive(true);
                WireEditor.Instance.DisableWireObjects();
            }
            else if(tool == Tool.MissingObject)
            {
                HierarchyUI.Instance.gameObject.SetActive(false);
                WireEditor.Instance.DisableWireObjects();
            }
            ObjectSelection.Instance.Clear();
        }

        public void DestroySelectedObjects()
        {
            if (CurrentTool != Tool.Level) return;

            UndoManager.RegisterState(new UndoDestroy(ObjectSelection.Instance.SelectedObjects), "Objects Destroyed");
            foreach (var o in ObjectSelection.Instance.SelectedObjects)
            {
                World.RemoveObject(o);
                Destroy(o);
            }
            ObjectSelection.Instance.Clear();
        }

        public void DublicateSelectedObjects()
        {
            if (CurrentTool != Tool.Level) return;
            if (ObjectSelection.Instance.SelectedObjects.Count == 0) return;

            List<GameObject> newObjs = new List<GameObject>();
            foreach (var o in ObjectSelection.Instance.SelectedObjects)
            {
                var obj = o.GetComponent<BuildObject>();
                if (obj != null)
                {
                    var newObj = Instantiate(o, o.transform.position, o.transform.rotation, o.transform.parent);
                    foreach (MeshFilter meshFilter in newObj.GetComponentsInChildren<MeshFilter>())
                    {
                        if (meshFilter.GetComponent<ObjectHighlight>())
                        {
                            Object.Destroy(meshFilter.GetComponent<ObjectHighlight>());
                        }
                    }
                    newObj.GetComponent<BuildObject>().BuildID = World.LastBuildID++;
                    newObjs.Add(newObj);
                    World.AddObject(obj.ID, newObj);
                }
            }
            UndoManager.RegisterState(new UndoDublicate(newObjs), "Objects Dublicated");
            ObjectSelection.Instance.SetSelection(newObjs);
        }

        #region Mouse Events
        private Vector2 mMouseOrigin = Vector2.zero;
        private bool mMouseDragging = false,
                        mMouseIsDown = false,
                        mDragCanceled = false;
        const float MOUSE_DRAG_DELTA = .2f;
        private Rect dragRect = new Rect(0, 0, 0, 0);
        private GUIStyle dragRectStyle = new GUIStyle();
        public Color dragRectColor = new Color(0f, .75f, 1f, .6f);

        public void OnGUI()
        {
            /// draw a selection rect
            if (mMouseDragging)
            {
                GUI.color = dragRectColor;
                GUI.Box(dragRect, "", dragRectStyle);
                GUI.color = Color.white;
            }
        }

        public void OnEditorMouseMove()
        {
            if (GizmoObject.Instance.IsInUse)
                return;

            if (mMouseDragging)
            {
                //if (pb_InputManager.IsKeyInUse() || pb_InputManager.IsMouseInUse())
                //{
                //    mDragCanceled = true;
                //    mMouseDragging = false;
                //    return;
                //}

                dragRect.x = Mathf.Min(mMouseOrigin.x, Input.mousePosition.x);
                dragRect.y = Screen.height - Mathf.Max(mMouseOrigin.y, Input.mousePosition.y);
                dragRect.width = Mathf.Abs(mMouseOrigin.x - Input.mousePosition.x);
                dragRect.height = Mathf.Abs(mMouseOrigin.y - Input.mousePosition.y);

                UpdateDragPicker();
            }
            else
            if (mMouseIsDown && !mDragCanceled && Vector2.Distance(mMouseOrigin, Input.mousePosition) > MOUSE_DRAG_DELTA)
            {
                UndoManager.RegisterState(new UndoSelection(), "Selection changed");
                mMouseDragging = true;

                dragRect.x = mMouseOrigin.x;
                dragRect.y = Screen.height - mMouseOrigin.y;
                dragRect.width = 0f;
                dragRect.height = 0f;
            }
        }

        public void OnEditorMouseDown()
        {
            if (!GizmoObject.Instance.IsInUse)
            {
                mMouseOrigin = Input.mousePosition;
                mDragCanceled = false;
                mMouseIsDown = true;
            }
        }

        public void OnEditorMouseUp()
        {
            mMouseIsDown = false;

            if (!GizmoObject.Instance.IsInUse)
            {
                if (mMouseDragging || mDragCanceled)
                {
                    mDragCanceled = false;
                    mMouseDragging = false;
                }
                else
                {
                    Ray ray = BetterBuildCamera.Instance.EditorCamera.ScreenPointToRay(Input.mousePosition);

                    GameObject hit = GizmoUtility.ObjectRaycast(ray, FindCloseObjects());

                    if (hit != null)
                    {
                        UndoManager.RegisterState(new UndoSelection(), "Selection changed");

                        if (!Input.GetKey(KeyCode.LeftControl))
                            ObjectSelection.Instance.SetSelection(hit);
                        else
                        {
                            if (ObjectSelection.Instance.SelectedObjects.Contains(hit))
                            {
                                ObjectSelection.Instance.RemoveFromSelection(hit);
                            }
                            else
                            {
                                ObjectSelection.Instance.AddToSelection(hit);
                            }
                        }
                    }
                    else
                    {
                        if (ObjectSelection.Instance.SelectedObjects.Count > 0)
                        {
                            UndoManager.RegisterState(new UndoSelection(), "Selection changed");
                        }
                        ObjectSelection.Instance.Clear();

                        if (ObjectSelection.Instance.SelectedObjects.Count < 1)
                            GizmoObject.Instance.SetIsHidden(true);
                    }
                }
            }
        }

        void UpdateDragPicker()
        {
            Rect screenRect = new Rect(dragRect.x, Screen.height - (dragRect.y + dragRect.height), dragRect.width, dragRect.height);
            if(Input.GetKey(KeyCode.LeftControl))
            {
                var sel = new List<GameObject>(ObjectSelection.Instance.SelectedObjects);
                sel.AddRange(FindCloseObjects(screenRect));
                ObjectSelection.Instance.UpdateSelection(sel);
            }
            else
                ObjectSelection.Instance.SetSelection(FindCloseObjects(screenRect));
        }

        public List<GameObject> FindCloseObjects(Rect? screenRect = null)
        {
            List<GameObject> objs = new List<GameObject>();

            foreach (var objList in World.BuildObjects.Values.ToList())
            {
                foreach (var obj in objList)
                {
                    if (!obj.activeInHierarchy) continue;

                    if (screenRect != null)
                    {
                        Vector2 pos = BetterBuildCamera.Instance.EditorCamera.WorldToScreenPoint(obj.transform.position);
                        if (!((Rect)screenRect).Contains(pos))
                            continue;
                    }
                    if (Vector3.Distance(obj.transform.position, BetterBuildCamera.Instance.EditorCamera.transform.position) < MaxObjectDistance)
                    {
                        objs.Add(obj);
                    }
                }
            }
            //var activeRegions = allRegions.Where(r => !r.Hibernated);
            //foreach (var region in activeRegions)
            //{
            //    foreach (var sector in allSectors)
            //    {
            //        Transform parent = region.root.transform.Find(sector);
            //        if (parent != null)
            //        {
            //            AddChildren(parent, objs, screenRect);
            //        }
            //    }
            //}

            return objs;
        }

        public void AddChildren(Transform parent, List<GameObject> objs, Rect? screenRect = null)
        {
            foreach (Transform child in parent)
            {
                if (!child.gameObject.activeInHierarchy) continue;

                if (child.name.Contains("_lv"))
                {
                    AddChildren(child, objs);
                }
                else
                {
                    if (screenRect != null)
                    {
                        Vector2 pos = BetterBuildCamera.Instance.EditorCamera.WorldToScreenPoint(child.position);
                        if (!((Rect)screenRect).Contains(pos))
                            continue;
                    }
                    if (Vector3.Distance(child.position, BetterBuildCamera.Instance.EditorCamera.transform.position) < MaxObjectDistance)
                    {
                        objs.Add(child.gameObject);
                    }
                }
            }
        }
        #endregion

        public void OnSelectionChange()
        {
            if (CurrentTool != Tool.Level) return;
            if (ObjectSelection.Instance.SelectedObjects.Count > 0)
            {
                if (ObjectSelection.Instance.SelectedObjects.Count == 1)
                {
                    GizmoObject.Instance.SetTRS(ObjectSelection.Instance.GetCenter(), ObjectSelection.Instance.SelectedObjects[0].transform.rotation, Vector3.one);
                    InspectorUI.Instance?.SetActive(true);
                }
                else
                {
                    GizmoObject.Instance.SetTRS(ObjectSelection.Instance.GetCenter(), Quaternion.identity, Vector3.one);
                    InspectorUI.Instance?.SetActive(false);
                }
                GizmoObject.Instance.SetIsHidden(false);
                ToolbarUI.Instance?.SetUtilityButtons(true);
                HierarchyUI.Instance?.SetActive(false);
            }
            else
            {
                ToolbarUI.Instance?.SetUtilityButtons(false);
                GizmoObject.Instance.SetIsHidden(true);
                HierarchyUI.Instance?.SetActive(true);
                InspectorUI.Instance?.SetActive(false);
            }

        }

        public void OnHandleBegin(GizmoTransform transform)
        {
            handleTransformOrigin = transform;
            CacheSelectionTransforms();
        }

        public void OnHandleMove(GizmoTransform transform)
        {
            GizmoTransform delta = (transform - handleTransformOrigin);

            mTransform.SetTRS(handleTransformOrigin);

            Transform[] parents = new Transform[ObjectSelection.Instance.SelectedObjects.Count];

            for (int i = 0; i < ObjectSelection.Instance.SelectedObjects.Count; i++)
            {
                GameObject go = ObjectSelection.Instance.SelectedObjects[i];
                go.transform.SetTRS(selectionTransformOrigin[i]);
                parents[i] = go.transform.parent;
                go.transform.parent = mTransform;
            }

            mTransform.SetTRS(transform);

            for (int i = 0; i < ObjectSelection.Instance.SelectedObjects.Count; i++)
            {
                ObjectSelection.Instance.SelectedObjects[i].transform.parent = parents[i];
            }
        }

        /**
         * Store the transform of each selected gameObject in selectionTransformOrigin array.
         */
        void CacheSelectionTransforms()
        {
            int count = ObjectSelection.Instance.SelectedObjects.Count;
            selectionTransformOrigin = new GizmoTransform[count];
            for (int i = 0; i < count; i++)
            {
                selectionTransformOrigin[i] = new GizmoTransform(ObjectSelection.Instance.SelectedObjects[i].transform);
            }
        }
    }
}

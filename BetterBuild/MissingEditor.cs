using BetterBuild.Gizmo;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class MissingEditor : MonoBehaviour
    {
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
            "Upgrades",
            "Resources",
            "Water",
            "Interactives",
            "Build Sites",
            "Loot",
            "FX"
        };

        private GameObject m_Selected;

        private void Start()
        {
            allRegions = Resources.FindObjectsOfTypeAll<Region>();
        }

        private void Update()
        {
            if (ObjectEditor.Instance.CurrentTool != ObjectEditor.Tool.MissingObject) return;

            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = BetterBuildCamera.Instance.EditorCamera.ScreenPointToRay(Input.mousePosition);
                GameObject hit = GizmoUtility.ObjectRaycast(ray, FindCloseObjects());

                if (m_Selected != null)
                {
                    foreach (MeshFilter meshFilter in m_Selected.GetComponentsInChildren<MeshFilter>())
                    {
                        if (meshFilter.GetComponent<ObjectHighlight>())
                        {
                            Destroy(meshFilter.GetComponent<ObjectHighlight>());
                        }
                    }
                }
                if (hit != null)
                {
                    m_Selected = hit;
                    foreach (MeshFilter meshFilter in m_Selected.GetComponentsInChildren<MeshFilter>())
                    {
                        meshFilter.gameObject.AddComponent<ObjectHighlight>();
                    }
                }
                else
                {
                    m_Selected = null;
                }
            }
        }

        private void OnGUI()
        {
            if (ObjectEditor.Instance.CurrentTool != ObjectEditor.Tool.MissingObject) return;
            GUILayout.Space(40);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Missing object selector");
            if(m_Selected == null)
            {
                GUILayout.Label("Click on the world to select a object");
            }
            else
            {
                GUILayout.Label("Name: " + m_Selected.name);
                GUILayout.Label("Path: " + GetGameObjectPath(m_Selected.transform));
                GUILayout.Label("Position: " + m_Selected.transform.position);
            }
            GUILayout.EndVertical();
        }

        private string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        public List<GameObject> FindCloseObjects(Rect? screenRect = null)
        {
            List<GameObject> objs = new List<GameObject>();
            var activeRegions = allRegions.Where(r => !r.Hibernated);
            foreach (var region in activeRegions)
            {
                foreach (var sector in allSectors)
                {
                    Transform parent = region.root.transform.Find(sector);
                    if (parent != null)
                    {
                        AddChildren(parent, objs, screenRect);
                    }
                }
            }

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
                    if (Vector3.Distance(child.position, BetterBuildCamera.Instance.EditorCamera.transform.position) < Globals.Settings.RenderDistance)
                    {
                        objs.Add(child.gameObject);
                    }
                }
            }
        }

        public static void SetClipboard(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Attempt to set clipboard with null");

            Process clipboardExecutable = new Process();
            clipboardExecutable.StartInfo = new ProcessStartInfo // Creates the process
            {
                RedirectStandardInput = true,
                FileName = @"clip",
            };
            clipboardExecutable.Start();

            clipboardExecutable.StandardInput.Write(value); // CLIP uses STDIN as input.
            // When we are done writing all the string, close it so clip doesn't wait and get stuck
            clipboardExecutable.StandardInput.Close();

            return;
        }
    }
}

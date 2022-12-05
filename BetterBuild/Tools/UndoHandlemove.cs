using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterBuild.Gizmo;
using BetterBuild.Persistance;
using UnityEngine;

namespace BetterBuild
{
    public class UndoHandlemove : IUndo
    {
        private Tool tool;

        public UndoHandlemove(Tool tool)
        {
            this.tool = tool;
        }

        public void ApplyState(Hashtable values)
        {
            foreach (DictionaryEntry item in values)
            {
                var buildObject = BuildObject.GetBuildObject((uint)item.Key);
                if(buildObject != null)
                {
                    if (tool == Tool.Position)
                        buildObject.transform.position = (Vector3)item.Value;
                    else if (tool == Tool.Scale)
                    {
                        object[] data = (object[])item.Value;
                        buildObject.transform.position = (Vector3)data[0];
                        buildObject.transform.localScale = (Vector3)data[1];
                    }
                    else if (tool == Tool.Rotate)
                    {
                        object[] data = (object[])item.Value;
                        buildObject.transform.position = (Vector3)data[0];
                        buildObject.transform.eulerAngles = (Vector3)data[1];
                    }
                }
            }
            GizmoObject.Instance.SetTRS(ObjectSelection.Instance.GetCenter(), Quaternion.identity, Vector3.one);
        }

        public void OnExitScope()
        {
            
        }

        public Hashtable RecordState()
        {
            Hashtable hash = new Hashtable();

            foreach (GameObject go in ObjectSelection.Instance.SelectedObjects)
            {
                var buildObject = go.GetComponent<BuildObject>();
                if (buildObject != null)
                {
                    if (tool == Tool.Position)
                        hash.Add(buildObject.BuildID, go.transform.position);
                    else if (tool == Tool.Scale)
                        hash.Add(buildObject.BuildID, new object[] { go.transform.position, go.transform.localScale });
                    else if (tool == Tool.Rotate)
                        hash.Add(buildObject.BuildID, new object[] { go.transform.position, go.transform.eulerAngles });
                }
            }

            return hash;
        }
    }
}

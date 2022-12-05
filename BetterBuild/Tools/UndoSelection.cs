using BetterBuild.Persistance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class UndoSelection : IUndo
    {
        public UndoSelection()
        {
            
        }

        public void ApplyState(Hashtable values)
        {
            List<GameObject> objs = new List<GameObject>();
            foreach(var id in values.Values.Cast<uint>())
            {
                var buildObject = BuildObject.GetBuildObject(id);
                if(buildObject != null)
                {
                    objs.Add(buildObject.gameObject);
                }
            }
            ObjectSelection.Instance.SetSelection(objs);
        }

        public void OnExitScope()
        {
            
        }

        public Hashtable RecordState()
        {
            Hashtable hash = new Hashtable();
            int n = 0;

            foreach (GameObject go in ObjectSelection.Instance.SelectedObjects)
            {
                var buildObject = go.GetComponent<BuildObject>();
                if (buildObject != null)
                {
                    hash.Add(n++, buildObject.BuildID);
                }
            }

            return hash;
        }
    }
}

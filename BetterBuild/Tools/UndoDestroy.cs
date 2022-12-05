using BetterBuild.Persistance;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterBuild
{
    public class UndoDestroy : IUndo
    {
        struct DestroyObject
        {
            public uint ID;
            public uint BuildID;
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Scale;
        }
        private DestroyObject[] m_Objects;
        private bool shouldDestroy = false;

        public UndoDestroy(List<GameObject> objects)
        {
            m_Objects = new DestroyObject[objects.Count];
            for (int i = 0; i < m_Objects.Length; i++)
            {
                var go = objects[i];
                var obj = go.GetComponent<BuildObject>();
                if (obj != null)
                {
                    m_Objects[i] = new DestroyObject()
                    {
                        ID = obj.ID,
                        BuildID = obj.BuildID,
                        Pos = go.transform.position,
                        Rot = go.transform.eulerAngles,
                        Scale = go.transform.localScale
                    };
                }
            }
        }

        public void ApplyState(Hashtable values)
        {
            if (shouldDestroy)
            {
                List<GameObject> objs = new List<GameObject>();
                foreach (var obj in m_Objects)
                {
                    var buildObject = BuildObject.GetBuildObject(obj.BuildID);
                    if (buildObject != null)
                    {
                        objs.Add(buildObject.gameObject);
                    }
                }
                foreach (var o in objs)
                {
                    World.RemoveObject(o);
                    GameObject.Destroy(o);
                }
                ObjectSelection.Instance.Clear();
            }
            else
            {
                var newObjs = new List<GameObject>();
                foreach (var o in m_Objects)
                {
                    var newObj = GameObject.Instantiate(ObjectManager.GetObject(o.ID), o.Pos, Quaternion.Euler(o.Rot));
                    newObj.transform.localScale = o.Scale;
                    newObj.GetComponent<BuildObject>().BuildID = o.BuildID;
                    newObjs.Add(newObj);
                    World.AddObject(o.ID, newObj);
                    newObj.SetActive(true);
                }
                ObjectSelection.Instance.SetSelection(newObjs);
            }
            shouldDestroy = !shouldDestroy;
        }

        public void OnExitScope()
        {

        }

        public Hashtable RecordState()
        {
            Hashtable hash = new Hashtable();
            return hash;
        }
    }
}

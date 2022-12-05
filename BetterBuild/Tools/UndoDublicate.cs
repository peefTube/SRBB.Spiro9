using BetterBuild.Persistance;
using MonomiPark.SlimeRancher.Regions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterBuild
{
    public class UndoDublicate : IUndo
    {
        struct DublicateObject
        {
            public uint ID;
            public uint BuildID;
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Scale;
            public RegionRegistry.RegionSetId Region;
        }
        private DublicateObject[] m_Objects;
        bool shouldDestroy = true;

        public UndoDublicate(List<GameObject> objects)
        {
            m_Objects = new DublicateObject[objects.Count];
            for (int i = 0; i < m_Objects.Length; i++)
            {
                var go = objects[i];
                var obj = go.GetComponent<BuildObject>();
                if (obj != null)
                {
                    m_Objects[i] = new DublicateObject()
                    {
                        ID = obj.ID,
                        BuildID = obj.BuildID,
                        Region = obj.Region,
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
                ObjectSelection.Instance.SetSelection(values.Values.Cast<GameObject>().ToList());
            }
            else
            {
                var newObjs = new List<GameObject>();
                foreach (var o in m_Objects)
                {
                    var newObj = GameObject.Instantiate(ObjectManager.GetObject(o.ID), o.Pos, Quaternion.Euler(o.Rot));
                    newObj.GetComponent<BuildObject>().BuildID = o.BuildID;
                    newObj.GetComponent<BuildObject>().Region = o.Region;
                    newObj.transform.localScale = o.Scale;
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
            int n = 0;

            foreach (GameObject go in ObjectSelection.Instance.SelectedObjects)
                hash.Add(n++, go);

            return hash;
        }
    }
}

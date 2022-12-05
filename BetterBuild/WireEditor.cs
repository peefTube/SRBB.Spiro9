using BetterBuild.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class WireEditor : SRSingleton<WireEditor>
    {
        private GameObject m_WireHolder;
        private List<GameObject> m_WireObjects = new List<GameObject>();

        private void Start()
        {
            m_WireHolder = new GameObject("WireHolder");
        }

        private void Update()
        {
            if (ObjectEditor.Instance.CurrentTool != ObjectEditor.Tool.Wire) return;
        }

        public void EnableWireObjects()
        {
            DisableWireObjects();
            foreach(var id in ObjectManager.WireObjects)
            {
                if(World.BuildObjects.TryGetValue(id, out List<GameObject> objs))
                {
                    foreach(var obj in objs)
                    {
                        GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        wire.transform.SetParent(m_WireHolder.transform);
                        wire.transform.position = obj.transform.position;

                        m_WireObjects.Add(wire);
                    }
                }
            }
        }

        public void DisableWireObjects()
        {
            foreach(var obj in m_WireObjects)
            {
                Destroy(obj);
            }
            m_WireObjects.Clear();
        }
    }
}

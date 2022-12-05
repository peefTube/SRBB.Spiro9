using BetterBuild.Persistance;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class UndoInstantiate : IUndo
    {
        public uint ID;
        public Vector3 Pos;
        public uint BuildID;
        public RegionRegistry.RegionSetId Region;

        public UndoInstantiate(uint id, GameObject obj)
        {
            ID = id;
            Pos = obj.transform.position;
            BuildID = obj.GetComponent<BuildObject>().BuildID;
            Region = obj.GetComponent<BuildObject>().Region;
        }

        public void ApplyState(Hashtable values)
        {
            var Obj = BuildObject.GetBuildObject(BuildID);
            if (Obj == null)
            {
                ObjectManager.RequestObject(ID, (buildObject) =>
                {
                    GameObject obj = GameObject.Instantiate(buildObject, Pos, Quaternion.identity) as GameObject;
                    obj.GetComponent<BuildObject>().BuildID = BuildID;
                    obj.GetComponent<BuildObject>().Region = Region;
                    World.AddObject(ID, obj);
                    obj.SetActive(true);

                    ObjectSelection.Instance.SetSelection(obj);
                });
            }
            else
            {
                World.RemoveObject(ID, Obj.gameObject);
                GameObject.Destroy(Obj.gameObject);
            }
        }

        public void OnExitScope()
        {
            
        }

        public Hashtable RecordState()
        {
            return new Hashtable();
        }
    }
}

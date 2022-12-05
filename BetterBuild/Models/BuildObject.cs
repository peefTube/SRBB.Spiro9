using BetterBuild.Persistance;
using MonomiPark.SlimeRancher.DataModel;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class BuildObject : MonoBehaviour
    {
        public static Dictionary<uint, BuildObject> AllObjects = new Dictionary<uint, BuildObject>();

        private float m_OcclusionTime;
        private Renderer[] m_Renderers;
        private bool m_DoesRender = true;

        public uint ID;
        private uint m_BuildID;
        public uint BuildID
        {
            get
            {
                return m_BuildID;
            }
            set
            {
                AllObjects[value] = this;
                m_BuildID = value;
            }
        }
        public uint HandlerID;

        public RegionRegistry.RegionSetId Region;

        private void Start()
        {
            m_Renderers = GetComponentsInChildren<Renderer>();

            if (gameObject.GetComponentInChildren<TeleportDestination>() != null)
            {
                gameObject.GetComponentInChildren<TeleportDestination>().regionSetId = Region;
            }
        }

        private void Update()
        {
            m_OcclusionTime -= Time.deltaTime;
            if (m_OcclusionTime <= 0)
            {
                m_OcclusionTime = 1;
                if (m_DoesRender && Vector3.Distance(SRSingleton<SceneContext>.Instance.Player.transform.position, transform.position) > Globals.Settings.RenderDistance)
                {
                    m_DoesRender = !m_DoesRender;
                    foreach (var rend in m_Renderers)
                    {
                        rend.enabled = m_DoesRender;
                    }
                }
                else if (!m_DoesRender && Vector3.Distance(SRSingleton<SceneContext>.Instance.Player.transform.position, transform.position) < Globals.Settings.RenderDistance)
                {
                    m_DoesRender = !m_DoesRender;
                    foreach (var rend in m_Renderers)
                    {
                        rend.enabled = m_DoesRender;
                    }
                }
            }
        }

        public static BuildObject GetBuildObject(uint id)
        {
            if (AllObjects.TryGetValue(id, out BuildObject obj))
                return obj;
            return null;
        }

        internal Dictionary<string, StringV01> GetData()
        {
            Dictionary<string, StringV01> data = new Dictionary<string, StringV01>();

            if (gameObject.GetComponentInChildren<TeleportDestination>() != null)
            {
                data["tpdestination"] = new StringV01() { value = gameObject.GetComponentInChildren<TeleportDestination>().teleportDestinationName };
            }
            if (gameObject.GetComponentInChildren<TeleportSource>() != null)
            {
                data["tpsource"] = new StringV01() { value = gameObject.GetComponentInChildren<TeleportSource>().destinationSetName };
            }
            if (gameObject.GetComponentInChildren<JournalEntry>() != null)
            {
                data["journaltext"] = new StringV01() { value = gameObject.GetComponentInChildren<JournalEntry>().entryKey };
            }

            return data;
        }

        internal void SetData(Dictionary<string, StringV01> data)
        {
            if (gameObject.GetComponentInChildren<TeleportDestination>() != null && data.ContainsKey("tpdestination"))
            {
                gameObject.GetComponentInChildren<TeleportDestination>().teleportDestinationName = data["tpdestination"].value;
            }
            if (gameObject.GetComponentInChildren<TeleportSource>() != null && data.ContainsKey("tpsource"))
            {
                gameObject.GetComponentInChildren<TeleportSource>().destinationSetName = data["tpsource"].value;
            }
            if (gameObject.GetComponentInChildren<JournalEntry>() != null && data.ContainsKey("journaltext"))
            {
                gameObject.GetComponentInChildren<JournalEntry>().entryKey = data["journaltext"].value;
            }
        }
    }
}

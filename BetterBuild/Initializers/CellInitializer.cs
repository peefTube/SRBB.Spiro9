using MonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using UnityEngine;

namespace BetterBuild.Initializers
{
    public class CellInitializer : MonoBehaviour
    {
        private CellDirector m_CellDirector;
        private Region m_Region;

        private GameObject m_Sector;
        private Bounds m_Bounds;

        private void Start()
        {
            m_CellDirector = gameObject.AddComponent<CellDirector>();
            m_Region = gameObject.AddComponent<Region>();

            m_CellDirector.region = m_Region;

            m_Region.setId = SRSingleton<SceneContext>.Instance.PlayerState.model.currRegionSetId;
            m_Region.bounds = m_Bounds;
            m_Region.root = m_Sector;
            // TODO maybe add region.proxyMesh? Combine a simple mesh of the base materials?

            m_Region.regionReg.DeregisterRegion(m_Region, m_Region.setId);
            m_Region.regionReg.RegisterRegion(m_Region, m_Region.setId, m_Region.bounds);
            m_Region.CheckReferences();

            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.position = m_Sector.transform.position;
            //cube.transform.SetParent(m_Sector.transform);

            //GameObject spawnerObj = new GameObject("Spawner");
            //// Rather use spawner.spawnLocs
            //spawnerObj.transform.position = m_Sector.transform.position;
            //spawnerObj.transform.SetParent(m_Sector.transform);

            //var templateSpawner = FindObjectOfType<DirectedSlimeSpawner>();
            //if (templateSpawner != null)
            //{
            //    var spawner = spawnerObj.AddComponent<DirectedSlimeSpawner>();
            //    spawner.radius = 5f;
            //    spawner.spawnDelayFactor = 1f;
            //    spawner.constraints = new DirectedActorSpawner.SpawnConstraint[]
            //    {
            //    new DirectedActorSpawner.SpawnConstraint()
            //    {
            //        slimeset = new SlimeSet()
            //        {
            //            members = new SlimeSet.Member[]
            //            {
            //                new SlimeSet.Member()
            //                {
            //                    prefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PINK_BOOM_LARGO),
            //                    weight = 1
            //                }
            //            },
            //        },
            //        window = new DirectedActorSpawner.TimeWindow()
            //        {
            //            startHour = 0,
            //            endHour = 0,
            //            timeMode = DirectedActorSpawner.TimeMode.ANY
            //        },
            //        feral = false,
            //        weight = 1f,
            //        maxAgitation = false
            //    }
            //    };
            //    spawner.spawnFX = templateSpawner.spawnFX;
            //    spawner.slimeSpawnFX = templateSpawner.slimeSpawnFX;
            //}
            //else
            //    Debug.Log("No template spawner");

            SRSingleton<ZoneEditor>.Instance.AddCell(m_CellDirector);
        }

        public void Initialize(GameObject sector, Bounds bounds)
        {
            m_Sector = sector;
            m_Bounds = bounds;
        }
    }
}

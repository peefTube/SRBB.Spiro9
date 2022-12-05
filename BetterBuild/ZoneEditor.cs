using BetterBuild.Initializers;
using MonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public class ZoneEditor : SRSingleton<ZoneEditor>
    {
        private List<ZoneDirector> m_Zones = new List<ZoneDirector>();
        private List<Region> m_Regions = new List<Region>();
        private List<CellDirector> m_CustomCells = new List<CellDirector>();

        public List<CellDirector> CustomCells { get { return m_CustomCells; } }

        private void Start()
        {
            m_Zones = Resources.FindObjectsOfTypeAll<ZoneDirector>().ToList();
            m_Regions = Resources.FindObjectsOfTypeAll<Region>().ToList();
        }

        public GameObject CreateNewCell(ZoneDirector.Zone zoneid, string cellname, Bounds bounds)
        {
            var zone = m_Zones.FirstOrDefault((z) => z.zone == zoneid);
            if(zone != null)
            {
                GameObject cellObj = new GameObject(cellname);
                GameObject cellRoot = new GameObject("Sector");

                cellRoot.transform.SetParent(cellObj.transform);
                cellObj.transform.SetParent(zone.transform);
                cellObj.transform.position = bounds.center;

                var initializer = cellObj.AddComponent<CellInitializer>();
                initializer.Initialize(cellRoot, bounds);

                cellRoot.SetActive(false);

                return cellRoot;
            }
            return null;
        }

        public void AddCell(CellDirector cell)
        {
            m_CustomCells.Add(cell);
        }
    }
}

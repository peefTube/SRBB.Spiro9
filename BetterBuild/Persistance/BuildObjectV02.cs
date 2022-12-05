using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace BetterBuild.Persistance
{
    public class BuildObjectV02 : VersionedPersistedDataSet<BuildObjectV01>
    {
        public Vector3V02 pos;
        public Vector3V02 euler;
        public Vector3V02 scale;
        public RegionRegistry.RegionSetId region;

        public override string Identifier
        {
            get
            {
                return "BBBO";
            }
        }

        public override uint Version
        {
            get
            {
                return 2;
            }
        }

        public BuildObjectV02() { }

        public override void LoadData(BinaryReader reader)
        {
            pos = LoadPersistable<Vector3V02>(reader);
            euler = LoadPersistable<Vector3V02>(reader);
            scale = LoadPersistable<Vector3V02>(reader);
            region = (RegionRegistry.RegionSetId)reader.ReadByte();
        }

        public override void UpgradeFrom(BuildObjectV01 legacyData)
        {
            pos = legacyData.pos;
            euler = legacyData.euler;
            scale = legacyData.scale;
            region = RegionRegistry.RegionSetId.HOME;
        }

        public override void WriteData(BinaryWriter writer)
        {
            WritePersistable(writer, pos);
            WritePersistable(writer, euler);
            WritePersistable(writer, scale);
            writer.Write((byte)region);
        }
    }
}

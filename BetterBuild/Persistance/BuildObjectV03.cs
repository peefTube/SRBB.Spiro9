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
    public class BuildObjectV03 : VersionedPersistedDataSet<BuildObjectV02>
    {
        public Vector3V02 pos;
        public Vector3V02 euler;
        public Vector3V02 scale;
        public RegionRegistry.RegionSetId region;
        public uint BuildID;
        public uint HandlerID;

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
                return 3;
            }
        }

        public BuildObjectV03() { }

        public override void LoadData(BinaryReader reader)
        {
            pos = LoadPersistable<Vector3V02>(reader);
            euler = LoadPersistable<Vector3V02>(reader);
            scale = LoadPersistable<Vector3V02>(reader);
            region = (RegionRegistry.RegionSetId)reader.ReadByte();
            BuildID = reader.ReadUInt32();
            HandlerID = reader.ReadUInt32();

            if (BuildID > World.LastBuildID)
            {
                World.LastBuildID = BuildID;
            }
            if (HandlerID > Globals.LastHandlerID)
            {
                Globals.LastHandlerID = HandlerID;
            }
        }

        public override void UpgradeFrom(BuildObjectV02 legacyData)
        {
            pos = legacyData.pos;
            euler = legacyData.euler;
            scale = legacyData.scale;
            region = legacyData.region;
            BuildID = World.LastBuildID++;
            HandlerID = Globals.LastHandlerID++;
        }

        public override void WriteData(BinaryWriter writer)
        {
            WritePersistable(writer, pos);
            WritePersistable(writer, euler);
            WritePersistable(writer, scale);
            writer.Write((byte)region);
            writer.Write(BuildID);
            writer.Write(HandlerID);
        }
    }
}

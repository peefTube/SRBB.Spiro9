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
    public class BuildObjectV04 : VersionedPersistedDataSet<BuildObjectV03>
    {
        public Vector3V02 pos;
        public Vector3V02 euler;
        public Vector3V02 scale;
        public RegionRegistry.RegionSetId region;
        public uint BuildID;
        public uint HandlerID;

        public Dictionary<string, StringV01> Data;

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
                return 4;
            }
        }

        public BuildObjectV04() { }

        public override void LoadData(BinaryReader reader)
        {
            pos = LoadPersistable<Vector3V02>(reader);
            euler = LoadPersistable<Vector3V02>(reader);
            scale = LoadPersistable<Vector3V02>(reader);
            region = (RegionRegistry.RegionSetId)reader.ReadByte();
            BuildID = reader.ReadUInt32();
            HandlerID = reader.ReadUInt32();
            Data = LoadDictionary(reader, (r) => r.ReadString(), (r) => LoadPersistable<StringV01>(reader));

            if (BuildID > World.LastBuildID)
            {
                World.LastBuildID = BuildID;
            }
            if (HandlerID > Globals.LastHandlerID)
            {
                Globals.LastHandlerID = HandlerID;
            }
        }

        public override void UpgradeFrom(BuildObjectV03 legacyData)
        {
            pos = legacyData.pos;
            euler = legacyData.euler;
            scale = legacyData.scale;
            region = legacyData.region;
            BuildID = legacyData.BuildID;
            HandlerID = legacyData.HandlerID;
            Data = new Dictionary<string, StringV01>();
        }

        public override void WriteData(BinaryWriter writer)
        {
            WritePersistable(writer, pos);
            WritePersistable(writer, euler);
            WritePersistable(writer, scale);
            writer.Write((byte)region);
            writer.Write(BuildID);
            writer.Write(HandlerID);
            WriteDictionary(writer, Data, (w, v) => w.Write(v), (w, v) => WritePersistable(w, v));
        }
    }
}

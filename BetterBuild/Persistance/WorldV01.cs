using MonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterBuild.Persistance
{
    public class WorldV01 : PersistedDataSet
    {
        public string name;
        public Dictionary<uint, List<BuildObjectV04>> buildObjects;

        public override string Identifier
        {
            get
            {
                return "BBW";
            }
        }

        public override uint Version
        {
            get
            {
                return 1;
            }
        }

        public WorldV01() { }

        public override void LoadData(BinaryReader reader)
        {
            name = reader.ReadString();
            buildObjects = LoadDictionary(reader, (r) => r.ReadUInt32(), (r) => LoadList<BuildObjectV04>(r));
        }

        public override void WriteData(BinaryWriter writer)
        {
            writer.Write(name);
            WriteDictionary(writer, buildObjects, (w, k) => w.Write(k), (w, v) => WriteList(w, v));
        }
    }
}

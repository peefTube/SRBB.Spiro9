using System.IO;
using MonomiPark.SlimeRancher.Persist;
using MonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace BetterBuild.Persistance
{
    public class BuildObjectV01 : PersistedDataSet
    {
        public Vector3V02 pos;
        public Vector3V02 euler;
        public Vector3V02 scale;

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
                return 1;
            }
        }

        public BuildObjectV01() { }

        public override void LoadData(BinaryReader reader)
        {
            pos = LoadPersistable<Vector3V02>(reader);
            euler = LoadPersistable<Vector3V02>(reader);
            scale = LoadPersistable<Vector3V02>(reader);
        }

        public override void WriteData(BinaryWriter writer)
        {
            WritePersistable(writer, pos);
            WritePersistable(writer, euler);
            WritePersistable(writer, scale);
        }
    }
}
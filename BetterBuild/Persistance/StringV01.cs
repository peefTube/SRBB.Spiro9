using MonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterBuild.Persistance
{
    public class StringV01 : PersistedDataSet
    {
        public string value;

        public override string Identifier
        {
            get
            {
                return "BBS1";
            }
        }
        
        public override uint Version
        {
            get
            {
                return 1;
            }
        }

        public override void LoadData(BinaryReader reader)
        {
            value = reader.ReadString();
        }

        public override void WriteData(BinaryWriter writer)
        {
            writer.Write(value);
        }
    }
}

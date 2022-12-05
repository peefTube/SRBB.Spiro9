using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterBuild
{
    public class BuildZoneData
    {
        public int ZoneID;
        public List<BuildCategoryData> Categories;
    }

    public class BuildCategoryData
    {
        public string Category;
        public List<BuildObjectsData> Objects;
    }

    public class BuildObjectsData
    {
        public uint ID;
        public int RenderID;
        public string Name;
        public string Path;
        public List<string> RemoveScripts;
    }

    public class BuildObjectsSearchData
    {
        public List<string> IncludeFolder;
        public List<string> IncludePrefab;
        public List<string> ExcludePrefab;
        public List<string> IgnoreList;
    }
}

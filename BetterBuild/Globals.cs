using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterBuild
{
    public static class Globals
    {
        public static GameObject ToolbarPrefab;
        public static GameObject HierarchyPrefab;
        public static GameObject TooltipPrefab;
        public static GameObject CategoryButtonPrefab;
        public static GameObject ObjectButtonPrefab;

        public static GameObject InspectorVector3;
        public static GameObject InspectorInput;

        public static Material HandleOpaqueMaterial;
        public static Material HandleTransparentMaterial;
        public static Material HandleRotateMaterial;

        public static Material HighlightMaterial;
        public static Material UnlitVertexColorMaterial;
        public static Material WireframeMaterial;

        public static Mesh ConeMesh;   // Used as translation handle cone caps.
        public static Mesh CubeMesh;    // Used for scale handle

        public static uint LastHandlerID;

        public static SettingsUI.Settings Settings = new SettingsUI.Settings();
    }
}

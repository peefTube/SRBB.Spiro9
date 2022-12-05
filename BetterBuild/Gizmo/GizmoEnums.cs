using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterBuild.Gizmo
{
    public enum HandleMovement
    {
        Up,
        Right,
        Forward,
        Plane
    }

    [System.Flags]
    public enum Axis
    {
        None = 0x0,
        X = 0x1,
        Y = 0x2,
        Z = 0x4
    }

    /**
     * Defines options for gizmo types and scene interaction.
     */
    public enum Tool
    {
        None,
        Position,
        Rotate,
        Scale,
        View
    }

    /**
    * Describes different culling options.
*/
    public enum Culling
    {
        Back = 0x1,
        Front = 0x2,
        FrontBack = 0x4
    }
}

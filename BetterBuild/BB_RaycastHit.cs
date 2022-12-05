using UnityEngine;

namespace BetterBuild
{
    /**
    * Describes the intersection point of a ray and mesh.
    */
    public class BB_RaycastHit
    {
        public Vector3 point;
        public float distance;
        public Vector3 normal;
        public int[] triangle;
    }
}

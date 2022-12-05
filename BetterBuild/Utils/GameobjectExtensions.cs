using BetterBuild.Gizmo;
using UnityEngine;

namespace BetterBuild
{
    /**
* Extension classes that make working with serialized types interchangeably with unity types easier
*/
    public static class GameobjectExtensions
    {

        /**
         * Shortcut for if(!GetComponent<T>) AddComponent<T>.
         */
        public static T DemandComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            T component = go.GetComponent<T>();

            if (component == null)
                component = go.AddComponent<T>();

            return component;
        }

        /**
             * Add an empty gameObject as a child to `go`.
             */
        public static GameObject AddChild(this GameObject go)
        {
            GameObject child = new GameObject();
            child.transform.SetParent(go.transform, false);
            return child;
        }

        /**
         * Add an empty gameObject as a child to `trs`.
         */
        public static Transform AddChild(this Transform trs)
        {
            Transform go = new GameObject().GetComponent<Transform>();
            go.SetParent(trs);
            return go;
        }

        /**
             * Set a UnityEngine.Transform with a Runtime.pb_Transform.
             */
        public static void SetTRS(this Transform transform, GizmoTransform pbTransform)
        {
            transform.position = pbTransform.position;
            transform.localRotation = pbTransform.rotation;
            transform.localScale = pbTransform.scale;
        }

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
    }
}

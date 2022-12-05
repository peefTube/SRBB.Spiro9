using System.Collections.Generic;
using UnityEngine;

namespace BetterBuild
{
    public class ObjectSelection : SRSingleton<ObjectSelection>
    {
        public delegate void OnSelectionChangeHandler();
        public event OnSelectionChangeHandler OnSelectionChange;

        public List<GameObject> SelectedObjects { get; } = new List<GameObject>();

        public Vector3 GetCenter()
        {
            if (SelectedObjects.Count == 0)
                return Vector3.zero;
            if (SelectedObjects.Count == 1)
                return SelectedObjects[0].transform.position;

            Bounds bounds = new Bounds(SelectedObjects[0].transform.position, Vector3.zero);
            for (int i = 1; i < SelectedObjects.Count; i++)
                bounds.Encapsulate(SelectedObjects[i].transform.position);
            return bounds.center;
        }

        public int GetCount()
        {
            return SelectedObjects.Count;
        }

        /**
         * Clear all objects in the current selection.
         */
        public void Clear()
        {
            int cleared = _Clear();

            if (cleared > 0)
            {
                OnSelectionChange?.Invoke();
            }
        }

        /**
        * Clear the selection lists and set them to `selection`.
        */
        public void SetSelection(List<GameObject> selection)
        {
            _Clear();

            foreach (GameObject go in selection)
                _AddToSelection(go);

            OnSelectionChange?.Invoke();
        }

        /**
        * Clear the selection lists and set them to `selection`.
        */
        public void UpdateSelection(List<GameObject> selection)
        {
            foreach (GameObject go in selection)
                _AddToSelection(go);

            OnSelectionChange?.Invoke();
        }

        /**
        * Clear the selection lists and set them to `selection`.
        */
        public void SetSelection(GameObject selection)
        {
            SetSelection(new List<GameObject>() { selection });
        }

        /**
         * Append `go` to the current selection (doesn't add in the event that it is already in the selection list).
         */
        public void AddToSelection(GameObject go)
        {
            _AddToSelection(go);

            OnSelectionChange?.Invoke();
        }

        /**
         * Remove an object from the current selection.
         */
        public void RemoveFromSelection(GameObject go)
        {
            if (_RemoveFromSelection(go))
            {
                OnSelectionChange?.Invoke();
            }
        }

        private void InitializeSelected(GameObject go)
        {
            foreach (MeshFilter meshFilter in go.GetComponentsInChildren<MeshFilter>())
            {
                meshFilter.gameObject.AddComponent<ObjectHighlight>();
            }
        }

        private void DeinitializeSelected(GameObject go)
        {
            foreach (MeshFilter meshFilter in go.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.GetComponent<ObjectHighlight>())
                {
                    Object.Destroy(meshFilter.GetComponent<ObjectHighlight>());
                }
            }
        }

        private bool _AddToSelection(GameObject go)
        {
            if (go != null && !SelectedObjects.Contains(go))
            {
                InitializeSelected(go);
                SelectedObjects.Add(go);

                return true;
            }
            return false;
        }

        private bool _RemoveFromSelection(GameObject go)
        {
            if (go != null && SelectedObjects.Contains(go))
            {
                DeinitializeSelected(go);
                SelectedObjects.Remove(go);

                return true;
            }

            return false;
        }

        private int _Clear()
        {
            int count = SelectedObjects.Count;

            for (int i = 0; i < count; i++)
                DeinitializeSelected(SelectedObjects[i]);

            SelectedObjects.Clear();

            return count;
        }
    }
}

using BetterBuild.Gizmo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BetterBuild.UI
{
    public class InspectorVector3UI : InspectorBase
    {
        private Text Label;
        private InputField XInput;
        private InputField YInput;
        private InputField ZInput;

        private Vector3 m_LastVector;

        public Action OnBeforeChange;
        public string Name { set { Label.text = value; } }

        private void Awake()
        {
            Label = transform.Find("Label").GetComponent<Text>();
            XInput = transform.Find("XInput").GetComponent<InputField>();
            YInput = transform.Find("YInput").GetComponent<InputField>();
            ZInput = transform.Find("ZInput").GetComponent<InputField>();
            
            XInput.onEndEdit.AddListener(OnValueChanged);
            YInput.onEndEdit.AddListener(OnValueChanged);
            ZInput.onEndEdit.AddListener(OnValueChanged);
        }

        private void Update()
        {
            Vector3 vector = (Vector3)getter();
            if (Vector3.Distance(m_LastVector, vector) > 0.01f)
            {
                m_LastVector = vector;
                XInput.text = vector.x.ToString();
                YInput.text = vector.y.ToString();
                ZInput.text = vector.z.ToString();
            }
        }

        private void OnValueChanged(string arg)
        {
            if (float.TryParse(XInput.text, out float x) && float.TryParse(YInput.text, out float y) && float.TryParse(ZInput.text, out float z))
            {
                OnBeforeChange?.Invoke();
                setter(new Vector3(x, y, z));
            }
            GizmoObject.Instance.SetTRS(ObjectSelection.Instance.GetCenter(), ObjectSelection.Instance.SelectedObjects[0].transform.rotation, Vector3.one);
        }
    }
}

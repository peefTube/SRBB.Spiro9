using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace BetterBuild.UI
{
    public class InspectorInput : InspectorBase
    {
        private Text m_Label;
        private InputField m_Input;

        public Action OnChange;

        public string Name { set { m_Label.text = value; } }
        public string Placeholder { set { m_Input.placeholder.GetComponent<Text>().text = value; } }

        private void Awake()
        {
            m_Label = transform.Find("Label").GetComponent<Text>();
            m_Input = transform.Find("Input").GetComponent<InputField>();

            m_Input.onEndEdit.AddListener(OnEndEdit);
        }

        private void Start()
        {
            m_Input.text = (string)getter();
        }

        private void OnEndEdit(string arg0)
        {
            setter(arg0);
            OnChange?.Invoke();
        }
    }
}

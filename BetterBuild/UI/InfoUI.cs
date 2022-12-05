using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InfoUI : SRSingleton<InfoUI>
{
    private GameObject m_GameObject;
    
    private void Start()
    {
        m_GameObject = transform.Find("Info").gameObject;

        Button closeButton = transform.Find($"Info/Panel/CloseButton")?.GetComponent<Button>();
        closeButton.onClick.AddListener(OnClose);

        OnClose();
    }

    private void OnClose()
    {
        m_GameObject.SetActive(false);
    }

    public void Open()
    {
        m_GameObject.SetActive(true);
    }
}

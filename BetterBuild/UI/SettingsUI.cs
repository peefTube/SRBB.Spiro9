﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using BetterBuild;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using BetterBuild.Initializers;

public class SettingsUI : SRSingleton<SettingsUI>
{
    private GameObject m_GameObject;
    private Text m_RenderText;
    private Text m_HighlightText;

    [Serializable]
    public class Settings
    {
        public bool EnableFog = false;
        public int RenderDistance = 1000;
        public ObjectHighlight.HighlightType HighlightMethod = ObjectHighlight.HighlightType.Wireframe;
        public byte HighlightStrength = 50;
    }

    private void Start()
    {
        m_GameObject = transform.Find("Settings").gameObject;

        Button closeButton = GetSettingsButton("Close");
        Toggle fogToggle = transform.Find("Settings/Panel/FogToggle").GetComponent<Toggle>();
        m_RenderText = transform.Find("Settings/Panel/RenderText").GetComponent<Text>();
        Slider renderSlider = transform.Find("Settings/Panel/RenderSlider").GetComponent<Slider>();
        Dropdown hightlightDropdown = transform.Find("Settings/Panel/HighlightDropdown").GetComponent<Dropdown>();
        m_HighlightText = transform.Find("Settings/Panel/HighlightText").GetComponent<Text>();
        Slider hightlightSlider = transform.Find("Settings/Panel/HighlightSlider").GetComponent<Slider>();

        closeButton.onClick.AddListener(OnClose);
        fogToggle.onValueChanged.AddListener(OnFogChanged);
        renderSlider.onValueChanged.AddListener(OnRenderDistanceChanged);
        hightlightDropdown.onValueChanged.AddListener(OnHighlightMethodChanged);
        hightlightSlider.onValueChanged.AddListener(OnHightlightStrengthChanged);

        if(File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "settings.txt")))
        {
            Globals.Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(BetterBuildMod.ModDataPath, "settings.txt")));
        }

        renderSlider.value = Globals.Settings.RenderDistance;
        OnRenderDistanceChanged(Globals.Settings.RenderDistance);
        fogToggle.isOn = Globals.Settings.EnableFog;
        OnFogChanged(Globals.Settings.EnableFog);
        hightlightDropdown.value = (int)Globals.Settings.HighlightMethod;
        OnHighlightMethodChanged((int)Globals.Settings.HighlightMethod);
        hightlightSlider.value = Globals.Settings.HighlightStrength;
        OnHightlightStrengthChanged(Globals.Settings.HighlightStrength);

        OnClose();
    }

    private void OnHightlightStrengthChanged(float arg0)
    {
        m_HighlightText.text = $"Highlight Strength: {arg0}";
        Globals.Settings.HighlightStrength = (byte)arg0;

        Globals.HighlightMaterial.color = new Color(Globals.HighlightMaterial.color.r, Globals.HighlightMaterial.color.g, Globals.HighlightMaterial.color.b, arg0 / 255f);
        Globals.WireframeMaterial.color = new Color(Globals.WireframeMaterial.color.r, Globals.WireframeMaterial.color.g, Globals.WireframeMaterial.color.b, arg0 / 255f);
        SaveSettings();
    }

    private void OnHighlightMethodChanged(int arg0)
    {
        Globals.Settings.HighlightMethod = (ObjectHighlight.HighlightType)arg0;
        SaveSettings();

        foreach (var highlight in FindObjectsOfType<ObjectHighlight>())
        {
            highlight.Awake();
        }
    }

    private void OnRenderDistanceChanged(float arg0)
    {
        m_RenderText.text = $"Render Distance: {arg0} Meters";
        Globals.Settings.RenderDistance = (int)arg0;

        SaveSettings();
    }

    private void OnFogChanged(bool arg0)
    {
        RenderSettings.fog = arg0;
        Globals.Settings.EnableFog = arg0;
        SaveSettings();
    }

    private void OnClose()
    {
        m_GameObject.SetActive(false);
    }

    public void Open()
    {
        m_GameObject.SetActive(true);
    }

    private Button GetSettingsButton(string name)
    {
        return transform.Find($"Settings/Panel/{name}Button")?.GetComponent<Button>();
    }

    private void SaveSettings()
    {
        File.WriteAllText(Path.Combine(BetterBuildMod.ModDataPath, "settings.txt"), JsonConvert.SerializeObject(Globals.Settings));
    }
}

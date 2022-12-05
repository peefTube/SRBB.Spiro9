using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using BetterBuild.Models;
using System.IO;
using Newtonsoft.Json;
using BetterBuild;
using System;
using MonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using System.Linq;

public class TeleportUI : SRSingleton<TeleportUI>
{
    private GameObject m_GameObject;
    private List<TeleportModel> m_TeleportModels;

    private ScrollRect m_CategoryScroll;
    private InputField m_NameInput;

    public bool IsOpen { get { return m_GameObject.activeSelf; } }

    private void Start()
    {
        m_GameObject = transform.Find("Teleports").gameObject;

        m_CategoryScroll = transform.Find("Teleports/CategoryScroll").GetComponent<ScrollRect>();
        m_NameInput = transform.Find("Teleports/NameInput").GetComponent<InputField>();
        var addButton = transform.Find("Teleports/AddButton").GetComponent<Button>();

        if (File.Exists(Path.Combine(BetterBuildMod.ModDataPath, "tp.txt")))
        {
            m_TeleportModels = JsonConvert.DeserializeObject<TeleportModel[]>(File.ReadAllText(Path.Combine(BetterBuildMod.ModDataPath, "tp.txt"))).ToList();
        }
        else
        {
            string data = "[{\"Name\":\"Ranch Home\",\"PositionX\":52.8,\"PositionY\":16.3,\"PositionZ\":-132.7,\"RotationX\":0.0,\"RotationY\":102.6,\"RotationZ\":0.0,\"Region\":0},{\"Name\":\"Desert Temple\",\"PositionX\":119.9,\"PositionY\":1077.4,\"PositionZ\":917.5,\"RotationX\":0.0,\"RotationY\":0.0,\"RotationZ\":0.0,\"Region\":1},{\"Name\":\"Slimulations\",\"PositionX\":1052.1,\"PositionY\":14.4,\"PositionZ\":824.0,\"RotationX\":0.0,\"RotationY\":315.0,\"RotationZ\":0.0,\"Region\":4},{\"Name\":\"Nimble 1\",\"PositionX\":-768.5,\"PositionY\":7.5,\"PositionZ\":-841.3,\"RotationX\":0.0,\"RotationY\":44.9,\"RotationZ\":0.0,\"Region\":2},{\"Name\":\"Nimble 2\",\"PositionX\":-189.8,\"PositionY\":11.1,\"PositionZ\":-1008.4,\"RotationX\":0.0,\"RotationY\":273.6,\"RotationZ\":0.0,\"Region\":2}]";
            m_TeleportModels = JsonConvert.DeserializeObject<TeleportModel[]>(data).ToList();

            File.WriteAllText(Path.Combine(BetterBuildMod.ModDataPath, "tp.txt"), JsonConvert.SerializeObject(m_TeleportModels.ToArray(), Formatting.Indented));
        }

        foreach (var model in m_TeleportModels)
        {
            GameObject categoryObj = Instantiate(Globals.CategoryButtonPrefab, m_CategoryScroll.content, false);

            string categoryName = model.Name;
            categoryObj.GetComponentInChildren<Button>().onClick.AddListener(() => TeleportTo(model.Region, new Vector3(model.PositionX, model.PositionY, model.PositionZ), new Vector3(model.RotationX, model.RotationY, model.RotationZ)));
            categoryObj.GetComponentInChildren<Text>().text = categoryName;
        }

        addButton.onClick.AddListener(OnAddTeleport);

        Close();
    }

    private void OnAddTeleport()
    {
        var name = m_NameInput.text;
        if(string.IsNullOrEmpty(name))
        {
            name = SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId() + " Teleport";
        }

        var model = new TeleportModel()
        {
            Name = name,
            PositionX = BetterBuildCamera.Instance.transform.position.x,
            PositionY = BetterBuildCamera.Instance.transform.position.y,
            PositionZ = BetterBuildCamera.Instance.transform.position.z,
            RotationX = BetterBuildCamera.Instance.transform.eulerAngles.x,
            RotationY = BetterBuildCamera.Instance.transform.eulerAngles.y,
            RotationZ = BetterBuildCamera.Instance.transform.eulerAngles.z,
            Region = SRSingleton<SceneContext>.Instance.RegionRegistry.GetCurrentRegionSetId()
        };
        GameObject categoryObj = Instantiate(Globals.CategoryButtonPrefab, m_CategoryScroll.content, false);

        string categoryName = model.Name;
        categoryObj.GetComponentInChildren<Button>().onClick.AddListener(() => TeleportTo(model.Region, new Vector3(model.PositionX, model.PositionY, model.PositionZ), new Vector3(model.RotationX, model.RotationY, model.RotationZ)));
        categoryObj.GetComponentInChildren<Text>().text = categoryName;

        m_NameInput.text = "";

        m_TeleportModels.Add(model);
        File.WriteAllText(Path.Combine(BetterBuildMod.ModDataPath, "tp.txt"), JsonConvert.SerializeObject(m_TeleportModels.ToArray(), Formatting.Indented));
    }

    private void TeleportTo(RegionRegistry.RegionSetId region, Vector3 position, Vector3 rotation)
    {
        BetterBuildCamera.Instance.transform.position = position;
        BetterBuildCamera.Instance.transform.eulerAngles = new Vector3(BetterBuildCamera.Instance.transform.eulerAngles.x, rotation.y, BetterBuildCamera.Instance.transform.eulerAngles.z);
        SRSingleton<SceneContext>.Instance.PlayerState.model.SetCurrRegionSet(region);
        ObjectSelection.Instance.Clear();
    }

    public void Open()
    {
        m_GameObject.SetActive(true);
    }

    public void Close()
    {
        m_GameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown mapFilesDropdown;

    public void CheckForMapFiles()
    {
        List<string> mapFiles = new List<string>();
        string[] filesInDirectory = Directory.GetFiles(Application.persistentDataPath);
        for (int i = 0; i < filesInDirectory.Length; i++)
        {
            filesInDirectory[i] = filesInDirectory[i].Remove(0, Application.persistentDataPath.Length + 1);
            if (filesInDirectory[i].StartsWith("map_") && filesInDirectory[i].EndsWith(".json"))
            {
                filesInDirectory[i] = filesInDirectory[i].Remove(0, "map_".Length);
                filesInDirectory[i] = filesInDirectory[i].Remove(filesInDirectory[i].Length - ".json".Length, ".json".Length);
                mapFiles.Add(filesInDirectory[i]);
            }
        }
        mapFilesDropdown.ClearOptions();
        mapFilesDropdown.AddOptions(mapFiles);
    }

    private void Start()
    {
        CheckForMapFiles();
    }

    public void PlayGame()
    {
        StartData.GameSettingsData settings = StartData.Instance.getData();
        settings.mapFilePath = Application.persistentDataPath + "/map_" + mapFilesDropdown.captionText.text + ".json";
        StartData.Instance.UpdateData(settings);
        SceneManager.LoadScene(1);
    }
}

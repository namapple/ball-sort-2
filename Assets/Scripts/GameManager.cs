using System.IO;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Core.PathCore;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Path = System.IO.Path;

public class GameManager : MonoBehaviour
{
    public GameObject root;
    public GameObject itemsRoot;
    public Button btnStart;
    public LevelData levelData;

    public int currentLevel = 1;

    private void Start()
    {
        btnStart.onClick.AddListener(OnClickButtonStart);
        currentLevel = 1;
    }

    private void OnClickButtonStart()
    {
        Init();
    }

    private void Init()
    {
        LoadLevelData();
    }

    private void LoadLevelData()
    {
        string levelDataPath =
            Path.Combine("LevelData", "beginner", "level_" + currentLevel + ".bytes");
        TextAsset levelDataFile = Resources.Load<TextAsset>(levelDataPath);
        
        Debug.Log(levelDataPath);
        var json = JsonConvert.DeserializeObject<LevelData>(levelDataFile.ToString());
        Debug.Log(json.bubbleTypes);
    }
    
}

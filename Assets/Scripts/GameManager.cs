using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    PLAYING,
    MOVING,
    WIN,
    LOSE
}

public class GameManager : MonoBehaviour
{
    public Button btnStart;
    public LevelData levelData;
    public GameObject tubeRoot;
    public GameObject itemBalls;
    private TubeLayout tubeLayoutComponent;

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
        SetUpTubeLayout();
    }

    private LevelData LoadLevelData()
    {
        // đường dẫn của file leveldata, lưu ý: k cần đuôi .bytes
        string levelDataPath =
            Path.Combine("LevelData", "beginner", "level_" + currentLevel);
        // load file text leveldata
        TextAsset levelDataFile = Resources.Load<TextAsset>(levelDataPath);
        // deserialize file text vừa load
        LevelData result = JsonConvert.DeserializeObject<LevelData>(levelDataFile.ToString());
        return result;
    }

    private void SetUpTubeLayout()
    {
        if (tubeLayoutComponent != null) Destroy(tubeLayoutComponent.gameObject);
        // Đường dẫn của prefab
        LevelData data = LoadLevelData();
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + data.numStack);
        Debug.Log(tubeLayoutPath);
        // Load prefab theo đường dẫn
        GameObject tubeLayoutPrefab = Resources.Load<GameObject>(tubeLayoutPath);

        // Khởi tạo prefab lên scene, nhơ gán vào 1 biến GameObject
        GameObject tubeLayoutObj = Instantiate(tubeLayoutPrefab, tubeRoot.transform, false);
        // tubeLayoutObj.transform.SetParent(tubeRoot.transform,false);
        tubeLayoutComponent = tubeLayoutObj.GetComponent<TubeLayout>();

        for (int i = 0; i < data.bubbleTypes.Count; i++)
        {
            int tubeIndex = i / 4;
            int ballIndex = i % 4;
            
            string ballPath = Path.Combine("Prefabs", "Balls", "Ball" + data.bubbleTypes[i]);
            GameObject ballPrefab = Resources.Load<GameObject>(ballPath);

            GameObject ballObject = Instantiate(ballPrefab,
                itemBalls.transform, false);
            ballObject.transform.position =
                tubeLayoutComponent.tubeLayout[tubeIndex].posList[ballIndex].transform.position;
            ballObject.transform.localScale =
                tubeLayoutComponent.tubeLayout[tubeIndex].posList[ballIndex].transform.localScale;
            ballObject.transform.rotation =
                tubeLayoutComponent.tubeLayout[tubeIndex].posList[ballIndex].transform.rotation;
        }
    }
}

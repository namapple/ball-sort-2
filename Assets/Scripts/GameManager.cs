using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum GameState
{
    PLAYING,
    MOVING,
    WIN,
    LOSE
}

public class GameManager : MonoBehaviour
{
    public Button btnStart = null;

    // public LevelData levelData = null;
    public GameObject tubeRoot = null;
    public GameObject itemBalls = null;
    public GameState state = GameState.PLAYING;

    public LevelDifficulty levelDifficulty = LevelDifficulty.ADVANCED;

    public List<BallObject> ballList = new List<BallObject>();
    private TubeLayout tubeLayoutComponent = null;

    public int currentLevel = 50;

    public TubeObject selectedTube = null;

    private void Start()
    {
        btnStart.onClick.AddListener(OnClickButtonStart);
        // currentLevel = 61;
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
            Path.Combine("LevelData", levelDifficulty.ToString().ToLower(),
                "level_" + currentLevel);
        // load file text leveldata
        TextAsset levelDataFile = Resources.Load<TextAsset>(levelDataPath);
        // deserialize file text vừa load
        LevelData result = JsonConvert.DeserializeObject<LevelData>(levelDataFile.ToString());
        return result;
    }

    private void SetUpTubeLayout()
    {
        // Xóa tubeLayout cũ khi reset level
        if (tubeLayoutComponent != null)
        {
            Destroy(tubeLayoutComponent.gameObject);
        }

        // XÓa hết ball cũ khi reset level
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i].gameObject);
        }

        ballList.Clear();
        // Đường dẫn của prefab
        LevelData levelData = LoadLevelData();
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + levelData.numStack);
        // Load prefab theo đường dẫn
        GameObject tubeLayoutPrefab = Resources.Load<GameObject>(tubeLayoutPath);

        // Khởi tạo prefab lên scene, nhớ gán vào 1 biến GameObject
        GameObject tubeLayoutObj = Instantiate(tubeLayoutPrefab, tubeRoot.transform, false);
        // Lấy component để truy cập tubeList
        tubeLayoutComponent = tubeLayoutObj.GetComponent<TubeLayout>();

        //chạy loop qua tubeList để subscribe Action OnClickTube
        for (int i = 0; i < tubeLayoutComponent.tubeList.Count; i++)
        {
            tubeLayoutComponent.tubeList[i].onClickTube += OnClickTubeObject;
        }

        // chạy loop đổ bóng vào các tube theo tubeIndex và ballIndex
        for (int i = 0; i < levelData.bubbleTypes.Count; i++)
        {
            int tubeIndex = i / 4;
            int ballIndex = i % 4;

            string ballPath = Path.Combine("Prefabs", "Balls", "Ball" + levelData.bubbleTypes[i]);
            GameObject ballPrefab = Resources.Load<GameObject>(ballPath);


            GameObject ballObject = Instantiate(ballPrefab,
                itemBalls.transform, false);

            // Lấy component BallOject để add vào 1 list dùng để quản lý
            BallObject ballObjectComponent = ballObject.GetComponent<BallObject>();
            ballObjectComponent.type = levelData.bubbleTypes[i];
            // Add hết các ball đã tạo ra vào 1 list
            ballList.Add(ballObjectComponent);

            //Add ball vào từng tube theo tubeIndex
            tubeLayoutComponent.tubeList[tubeIndex].ballObjects.Add(ballObjectComponent);

            //Đặt ballobject vào vị trí các pos trong postList
            ballObject.transform.position =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.position;
            if (levelData.numStack > 8)
            {
                ballObject.transform.localScale = new Vector3(0.7f, 0.7f,
                    tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.localScale.z);
            }
            else
            {
                ballObject.transform.localScale = tubeLayoutComponent.tubeList[tubeIndex]
                    .posList[ballIndex].transform.localScale;
            }

            ballObject.transform.rotation =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
        }
    }

    public void OnClickTubeObject(TubeObject tube)
    {
        if (selectedTube == null)
        {
            if (tube.IsTubeEmpty())
            {
                Debug.Log("Click lần đầu vào TUBE rỗng");
                return;
            }

            selectedTube = tube;
            Debug.Log("Click vào TUBE đủ điều kiện move BALL");
            // Thực hiện move ball trên cùng của Tube đang chọn lên vị trí posTop của tube đang chọn
            tube.ballObjects[tube.ballObjects.Count - 1].transform
                .DOMove(tube.posTop.transform.position, 0.15f);
        }
        else
        {
            if (selectedTube == tube || tube.IsTubeFull() || tube.IsTubeDone() ||
                (!tube.IsTubeEmpty() && tube.GetTopBallType() != selectedTube.GetTopBallType()))
            {
                selectedTube.ballObjects[selectedTube.ballObjects.Count - 1].transform
                    .DOMove(
                        selectedTube.posList[selectedTube.ballObjects.Count - 1].transform
                            .position, 0.15f);
                selectedTube = null;
                Debug.Log("Move Ball về vị trí cũ");
            }
            else
            {
                Debug.Log("Đủ đkiện nhận ball");
                tube.ballObjects.Add(
                    selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                selectedTube.ballObjects[selectedTube.ballObjects.Count - 1].transform
                    .DOMove(tube.posTop.transform.position,
                        0.15f).OnComplete(() =>
                    {
                        tube.ballObjects[tube.ballObjects.Count - 1].transform.DOMove(
                            tube.posList[tube.ballObjects.Count - 1].transform.position,
                            0.15f);
                    });
                selectedTube.ballObjects.Remove(
                    selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                // tube.ballObjects.Add(selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                selectedTube = null;
            }
        }
    }
}
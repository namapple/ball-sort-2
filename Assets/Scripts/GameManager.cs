using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public Button btnStart = null, btnUndo = null, btnAddTube = null, btnRandomLevel = null, btnReset = null;
    public GameObject tubeRoot = null;
    public GameObject itemBalls = null;
    public GameState gameState = GameState.PLAYING;
    public LevelDifficulty levelDifficulty = LevelDifficulty.ADVANCED;
    private TubeLayout tubeLayoutComponent = null;
    public LevelData levelData = new LevelData();
    public int currentLevel = 50;
    public int undoLimit = 5;
    public TubeObject selectedTube = null;
    public List<BallObject> ballList = new List<BallObject>();
    public List<StepMove> listStepMoved = new List<StepMove>();

    private void Start()
    {
        btnReset.onClick.AddListener(OnClickBtnReset);
        // btnStart.onClick.AddListener(OnClickButtonStart);
        btnUndo.onClick.AddListener(OnClickBtnUndo);
        btnAddTube.onClick.AddListener(OnClickBtnAddTube);
        btnRandomLevel.onClick.AddListener(OnClickBtnRandom);
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
        levelData = JsonConvert.DeserializeObject<LevelData>(levelDataFile.ToString());
        return levelData;
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
        // LevelData levelData = LoadLevelData();
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
            tubeLayoutComponent.tubeList[i].id = i;
        }
        
        // chạy loop đổ bóng vào các tube theo tubeIndex và ballIndex
        FillBall();
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
            if (selectedTube == tube || tube.IsTubeFull() || tube.IsTubeResolved() ||
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
                    .DOMove(tube.posTop.transform.position, 0.15f).OnComplete(() =>
                    {
                        tube.ballObjects[tube.ballObjects.Count - 1].transform
                            .DOMove(tube.posList[tube.ballObjects.Count - 1].transform.position, 0.15f).OnComplete(
                                () =>
                                {
                                    if (tube.IsTubeResolved())
                                    {
                                        Debug.Log("TUBE is resolved");
                                    }

                                    if (CheckGameWin())
                                    {
                                        Debug.Log("WIN!!!");
                                    }
                                });
                    });
                selectedTube.ballObjects.Remove(
                    selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                // Gán id của 2 tube vào StepMove
                listStepMoved.Add(new StepMove(selectedTube.id, tube.id));
                selectedTube = null;
            }
        }
    }

    public void DoUndoMove()
    {
        StepMove step = listStepMoved[listStepMoved.Count - 1];
        TubeObject tubeA = tubeLayoutComponent.tubeList[step.tubeA];
        TubeObject tubeB = tubeLayoutComponent.tubeList[step.tubeB];
        
        gameState = GameState.MOVING;

        Debug.Log("Moving DoMOVE 0");
        tubeB.ballObjects[tubeB.ballObjects.Count - 1].transform
            .DOMove(tubeB.posTop.transform.position, 0.15f).OnComplete(() =>
            {
                Debug.Log("Moving DoMOVE 1");
                tubeB.ballObjects[tubeB.ballObjects.Count - 1].transform
                    .DOMove(tubeA.posTop.transform.position, 0.15f).OnComplete(() =>
                    {
                        Debug.Log("OnComplete -- DoMOVE 1");
                        tubeA.ballObjects.Add(
                            tubeB.ballObjects[tubeB.ballObjects.Count - 1]);
                        
                        tubeB.ballObjects.Remove(
                            tubeB.ballObjects[tubeB.ballObjects.Count - 1]);
                        
                        Debug.Log("Moving DoMOVE 2");
                        tubeA.ballObjects[tubeA.ballObjects.Count - 1].transform
                            .DOMove(tubeA.posList[tubeA.ballObjects.Count - 1].transform.position, 0.15f).OnComplete(
                                () =>
                                {
                                    Debug.Log("OnComplete -- DoMOVE 2");
                                    gameState = GameState.PLAYING;
                                    listStepMoved.Remove(step);
                                });
                    });
            });

        Debug.Log("Undo Done!!");
    }

    public void OnClickBtnUndo()
    {
        Debug.Log("Click btn UNDO");

        if (gameState != GameState.PLAYING)
        {
            return;
        }

        if (listStepMoved.Count == 0)
        {
            return;
        }
        if (selectedTube != null)
        {
            gameState = GameState.MOVING;
            Debug.Log("DOMove step 0");
            selectedTube.ballObjects[selectedTube.ballObjects.Count - 1].transform
                .DOMove(
                    selectedTube.posList[selectedTube.ballObjects.Count - 1].transform
                        .position, 0.15f).OnComplete(() =>
                {
                    Debug.Log("DOMove step 1");
                    // gameState = GameState.PLAYING;
                    selectedTube = null;

                    DoUndoMove();
                });
            Debug.Log("DOMove step 2");

        }
        else
        {
            DoUndoMove();
        }
    }

    public void OnClickBtnAddTube()
    {
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + (levelData.numStack + 1));
        GameObject tubeLayoutPrefab = Resources.Load<GameObject>(tubeLayoutPath);

        GameObject tubeLayoutObj = Instantiate(tubeLayoutPrefab, tubeRoot.transform, false);
        TubeLayout newTubeLayout = tubeLayoutObj.GetComponent<TubeLayout>();

        for (int i = 0; i < newTubeLayout.tubeList.Count; i++)
        {
            newTubeLayout.tubeList[i].onClickTube += OnClickTubeObject;
        }

        for (int tubeIndex = 0; tubeIndex < tubeLayoutComponent.tubeList.Count; tubeIndex++)
        {
            // Gán list ball cũ cho list ball mới
            newTubeLayout.tubeList[tubeIndex].ballObjects = tubeLayoutComponent.tubeList[tubeIndex].ballObjects;
            for (int ballIndex= 0; ballIndex < newTubeLayout.tubeList[tubeIndex].ballObjects.Count; ballIndex++)
            {
                BallObject ballObject = newTubeLayout.tubeList[tubeIndex].ballObjects[ballIndex];
                //Đặt ballobject vào vị trí các pos trong postList
                ballObject.transform.position =
                    newTubeLayout.tubeList[tubeIndex].posList[ballIndex].transform.position;
                ballObject.transform.localScale = newTubeLayout.GetTubeScale();
                ballObject.transform.rotation =
                    newTubeLayout.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
            }
        }
       // Xóa tubeLayout cũ đi vì đã thêm 1 tube mới
        Destroy(tubeLayoutComponent.gameObject);
        // Gán lại tubeLayout mới
        tubeLayoutComponent = newTubeLayout;
    }

    public bool CheckGameWin()
    {
        foreach (TubeObject tube in tubeLayoutComponent.tubeList)
        {
            if (!tube.IsTubeResolved() && !tube.IsTubeEmpty())
            {
                return false;
            }
        }

        return true;
    }

    public void OnClickBtnRandom()
    {
        levelDifficulty = (LevelDifficulty) Random.Range(0, 3); // K bao gồm expert và genius
        currentLevel = Random.Range(1, 121);
        Init();
    }


    public void OnClickBtnReset()
    {
        // Xóa hết balls có trong ballList để thêm ball mới
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i].gameObject);
        }
        ballList.Clear();

        // Xóa hết ball object đã thêm vào từng tube trong tubeList để thêm mới
        for (int i = 0; i < tubeLayoutComponent.tubeList.Count; i++)
        {
            tubeLayoutComponent.tubeList[i].ballObjects.Clear();
        }
        FillBall();
    }

    public void FillBall()
    {
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
            ballObject.transform.localScale = tubeLayoutComponent.GetTubeScale();

            ballObject.transform.rotation =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
        }
    }
}

public enum GameState
{
    PLAYING,
    MOVING,
    WIN,
}

public class StepMove
{
    public int tubeA = -1;
    public int tubeB = -1;

    public StepMove(int tubeA, int tubeB)
    {
        this.tubeA = tubeA;
        this.tubeB = tubeB;
    }
}
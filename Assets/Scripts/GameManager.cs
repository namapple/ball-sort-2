using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using DG.Tweening;
using GameTown.MiniGame.BallSort;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public Button btnUndo = null, btnAddTube = null, btnReset = null, btnNextLevel = null, btnRestart = null, btnClose = null, btnPause = null, btnResume = null;
    public Button btnWin = null, btnLose = null;
    public GameObject tubeRoot = null;
    public GameObject itemBalls = null;
    public GameState gameState = GameState.PLAYING;
    // public LevelDifficulty levelDifficulty = LevelDifficulty.ADVANCED;
    
    private TubeLayout tubeLayoutComponent = null;
    public LevelData levelData = new LevelData();

    public LevelConfig levelConfig = new LevelConfig();

    public Text txtCurrentLevel = null;
    public Text txtDiffucultyMode = null;
    public Text txtRandomStage = null;
    public Image progressFill = null;

    private float maxDuration;
    private float remainningTime;
    private Coroutine countdownTimer;

    public int userCurrentLevel = 1;
    public string difficulty = null;
    public LevelDifficulty difficultyConfig;
    public int randomStagePerDifficulty;
    public GameObject winPanel = null;
    public GameObject losePanel = null;
    public GameObject pausePanel = null;
    public Text txtScore = null;
    public Text txtEndScore = null;
    private int currentScore;
    public TubeObject selectedTube = null;
    
    public List<BallObject> ballList = new List<BallObject>();
    public List<StepMove> listStepMoved = new List<StepMove>();

    private void Start()
    {
        levelConfig = LoadConfig();
        Init();
        txtScore.text = "Score: " + currentScore;
        // winPanel.SetActive(false);
        losePanel.SetActive(false);
        pausePanel.SetActive(false);
        txtCurrentLevel.text = "Level: " + userCurrentLevel;
        txtDiffucultyMode.text = "Mode: " + difficulty;
        txtRandomStage.text = "Stage: " + randomStagePerDifficulty;
        btnReset.onClick.AddListener(OnClickBtnReset);
        btnUndo.onClick.AddListener(OnClickBtnUndo);
        btnAddTube.onClick.AddListener(OnClickBtnAddTube);
        btnNextLevel.onClick.AddListener(OnClickBtnNext);
        btnRestart.onClick.AddListener(OnClickBtnRestart);
        btnClose.onClick.AddListener(OnClickBtnClose);
        btnPause.onClick.AddListener(OnClickBtnPause);
        btnResume.onClick.AddListener(OnClickBtnResume);
        btnWin.onClick.AddListener(OnClickBtnWin);
        btnLose.onClick.AddListener(OnClickBtnLose);
    }
    
    private void Init()
    {
        LoadLevelData();
        SetUpTubeLayout();
        GetTimeLimitAtStart();
    }

    public LevelConfig LoadConfig()
    {
        LevelConfig config = new LevelConfig();

        TextAsset jsonFile = Resources.Load<TextAsset>("level_config");

        config = JsonConvert.DeserializeObject<LevelConfig>(jsonFile.text);

        return config;
    }
    
    private LevelData LoadLevelData()
    {
        // random ????? kh?? theo level
        GetLevelDifficulty();
        // random stage 1 -> 120
        GetRandomStage();
        // ???????ng d???n c???a file leveldata, l??u ??: k c???n ??u??i .bytes
        string levelDataPath =
            Path.Combine("LevelData", difficulty.ToLower(), "level_" + randomStagePerDifficulty);
        // load file text leveldata t??? th?? m???c Resources (th?? m???c ?????c bi???t c???a Unity)
        TextAsset levelDataFile = Resources.Load<TextAsset>(levelDataPath);
        // deserialize file text v???a load
        levelData = JsonConvert.DeserializeObject<LevelData>(levelDataFile.text);
        return levelData;
    }

    private void SetUpTubeLayout()
    {
        // X??a tubeLayout c?? khi reset level
        if (tubeLayoutComponent != null)
        {
            Destroy(tubeLayoutComponent.gameObject);
        }
      
        // X??a h???t ball c?? khi reset level
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i].gameObject);
        }

        ballList.Clear();
        // ???????ng d???n c???a prefab
        // LevelData levelData = LoadLevelData();
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + levelData.numStack);
        // Load prefab theo ???????ng d???n
        GameObject tubeLayoutPrefab = Resources.Load<GameObject>(tubeLayoutPath);

        // Kh???i t???o prefab l??n scene, nh??? g??n v??o 1 bi???n GameObject
        GameObject tubeLayoutObj = Instantiate(tubeLayoutPrefab, tubeRoot.transform, false);
        // L???y component ????? truy c???p tubeList
        tubeLayoutComponent = tubeLayoutObj.GetComponent<TubeLayout>();

        // Ch???y loop qua tubeList ????? subscribe Action OnClickTube
        for (int i = 0; i < tubeLayoutComponent.tubeList.Count; i++)
        {
            tubeLayoutComponent.tubeList[i].onClickTube += OnClickTubeObject;
            tubeLayoutComponent.tubeList[i].id = i;
        }
        
        // Ch???y loop ????? b??ng v??o c??c tube theo tubeIndex v?? ballIndex
        FillBall();
    }

    public void OnClickTubeObject(TubeObject tube)
    {
        if (selectedTube == null)
        {
            if (tube.IsTubeEmpty() || tube.IsTubeResolved())
            {
                Debug.Log("Click l???n ?????u v??o TUBE r???ng/???? xong");
                return;
            }

            selectedTube = tube;
            Debug.Log("Click v??o TUBE ????? ??i???u ki???n move BALL");
            // Th???c hi???n move ball tr??n c??ng c???a Tube ??ang ch???n l??n v??? tr?? posTop c???a tube ??ang ch???n
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
                Debug.Log("Move Ball v??? v??? tr?? c??");
            }
            else
            {
                Debug.Log("????? ??ki???n nh???n ball");
                int selectedTubeId = selectedTube.id;
                tube.ballObjects.Add(
                    selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                selectedTube.ballObjects[selectedTube.ballObjects.Count - 1].transform
                    .DOMove(tube.posTop.transform.position, 0.15f).OnComplete(() =>
                    {
                        tube.ballObjects[tube.ballObjects.Count - 1].transform
                            .DOMove(tube.posList[tube.ballObjects.Count - 1].transform.position, 0.15f).OnComplete(
                                () =>
                                {
                                    int pointAddedStep = 0;
                                    if (tube.IsTubeResolved())
                                    {
                                        Debug.Log("TUBE is resolved");
                                        pointAddedStep = AddScore();
                                    }
                                    
                                    // G??n id c???a 2 tube v??o StepMove
                                    listStepMoved.Add(new StepMove(selectedTubeId, tube.id, pointAddedStep));

                                    if (CheckGameWin())
                                    {
                                        gameState = GameState.WIN;
                                        DoWin();
                                    }
                                });
                    });
                selectedTube.ballObjects.Remove(
                    selectedTube.ballObjects[selectedTube.ballObjects.Count - 1]);
                selectedTube = null;
            }
        }
    }

    public void DoUndoMove()
    {
        StepMove step = listStepMoved[listStepMoved.Count - 1];
        TubeObject tubeA = tubeLayoutComponent.tubeList[step.tubeA];
        TubeObject tubeB = tubeLayoutComponent.tubeList[step.tubeB];

        // Tr??? ??i???m khi undo tube ???? resolve
        currentScore -= step.point;
        txtScore.text = "Score: " + currentScore;
        
        gameState = GameState.MOVING;

        tubeB.ballObjects[tubeB.ballObjects.Count - 1].transform
            .DOMove(tubeB.posTop.transform.position, 0.15f).OnComplete(() =>
            {
                tubeB.ballObjects[tubeB.ballObjects.Count - 1].transform
                    .DOMove(tubeA.posTop.transform.position, 0.15f).OnComplete(() =>
                    {
                        tubeA.ballObjects.Add(
                            tubeB.ballObjects[tubeB.ballObjects.Count - 1]);
                        
                        tubeB.ballObjects.Remove(
                            tubeB.ballObjects[tubeB.ballObjects.Count - 1]);
                        
                        tubeA.ballObjects[tubeA.ballObjects.Count - 1].transform
                            .DOMove(tubeA.posList[tubeA.ballObjects.Count - 1].transform.position, 0.15f).OnComplete(
                                () =>
                                {
                                    gameState = GameState.PLAYING;
                                    listStepMoved.Remove(step);
                                });
                    });
            });

        Debug.Log("Undo Completed!!");
    }

    public void OnClickBtnUndo()
    {
        if (gameState != GameState.PLAYING)
        {
            Debug.Log("Return do gamestate != PLAYING");
            return;
        }

        if (listStepMoved.Count == 0)
        {
            Debug.Log("Return do kh??ng c?? step move n??o");
            return;
        }
        
        if (selectedTube != null)
        {
            gameState = GameState.MOVING;
            selectedTube.ballObjects[selectedTube.ballObjects.Count - 1].transform
                .DOMove(
                    selectedTube.posList[selectedTube.ballObjects.Count - 1].transform
                        .position, 0.15f).OnComplete(() =>
                {
                    // gameState = GameState.PLAYING;
                    selectedTube = null;
                    DoUndoMove();
                });
        }
        else
        {
            DoUndoMove();
        }
    }

    public void OnClickBtnAddTube()
    {
        if (gameState != GameState.PLAYING)
        {
            Debug.Log("Return do gameState != PLAYING");
            return;
        }

        // Check xem c?? tubeLayout ch??a th?? m???i th??m
        if (tubeLayoutComponent == null)
        {
            Debug.Log("Ch??a c?? tubeLayout n??o");
            return;
        }
        
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + (levelData.numStack + 1));
        GameObject tubeLayoutPrefab = Resources.Load<GameObject>(tubeLayoutPath);

        GameObject tubeLayoutObj = Instantiate(tubeLayoutPrefab, tubeRoot.transform, false);
        TubeLayout newTubeLayout = tubeLayoutObj.GetComponent<TubeLayout>();

        for (int i = 0; i < newTubeLayout.tubeList.Count; i++)
        {
            newTubeLayout.tubeList[i].onClickTube += OnClickTubeObject;
            newTubeLayout.tubeList[i].id = i;
        }

        for (int tubeIndex = 0; tubeIndex < tubeLayoutComponent.tubeList.Count; tubeIndex++)
        {
            // G??n list ball c?? cho list ball m???i
            newTubeLayout.tubeList[tubeIndex].ballObjects = tubeLayoutComponent.tubeList[tubeIndex].ballObjects;
            for (int ballIndex= 0; ballIndex < newTubeLayout.tubeList[tubeIndex].ballObjects.Count; ballIndex++)
            {
                BallObject ballObject = newTubeLayout.tubeList[tubeIndex].ballObjects[ballIndex];
                //?????t ballobject v??o v??? tr?? c??c pos trong postList
                ballObject.transform.position =
                    newTubeLayout.tubeList[tubeIndex].posList[ballIndex].transform.position;
                ballObject.transform.localScale = newTubeLayout.GetTubeScale();
                ballObject.transform.rotation =
                    newTubeLayout.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
            }
        }
       // X??a tubeLayout c?? ??i v?? ???? th??m 1 tube m???i
        Destroy(tubeLayoutComponent.gameObject);
        // G??n l???i tubeLayout m???i
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

    // public void OnClickBtnRandom()
    // {
    //     listStepMoved.Clear();
    //     if (gameState != GameState.PLAYING)
    //     {
    //         return;
    //     }
    //     
    //     levelDifficulty = (LevelDifficulty) Random.Range(0, 5); // K bao g???m expert v?? genius
    //     randomStagePerDifficulty = Random.Range(1, 121);
    //     Init();
    //     txtCurrentLevel.text = "Level: " + userCurrentLevel;
    //     txtDiffucultyMode.text = "Mode: " + difficulty;
    //     txtRandomStage.text = "Stage: " + randomStagePerDifficulty;
    // }

    public void OnClickBtnReset()
    {
        if (gameState != GameState.PLAYING)
        {
            Debug.Log("Return do gameState != PLAYING");
            return;
        }

        // Ch??a c?? tubeLayout th?? kh??ng reset ???????c
        if (tubeLayoutComponent == null)
        {
            Debug.Log("Ch??a c?? tubeLayout");
            return;
        }
        txtCurrentLevel.text = "Level: " + userCurrentLevel;
        txtDiffucultyMode.text = "Mode: " + difficulty;
        txtRandomStage.text = "Stage: " + randomStagePerDifficulty;
        
        // Tr??? h???t ??i???m c???a level hi???n t???i khi reset
        int totalScore = 0;
        for (int i = 0; i < listStepMoved.Count; i++)
        {
            totalScore += listStepMoved[i].point;
        }

        currentScore -= totalScore;
        txtScore.text = "Score: " + currentScore;
        listStepMoved.Clear();
        
        // X??a h???t balls c?? trong ballList ????? th??m ball m???i
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i].gameObject);
        }
        ballList.Clear();
        
        // X??a h???t ball object ???? th??m v??o t???ng tube trong tubeList ????? th??m m???i
        for (int i = 0; i < tubeLayoutComponent.tubeList.Count; i++)
        {
            tubeLayoutComponent.tubeList[i].ballObjects.Clear();
        }
        FillBall();

        // Reset l???i timer
        remainningTime = maxDuration;
        UpdateTimer();
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

            // L???y component BallObject ????? add v??o 1 list d??ng ????? qu???n l??
            BallObject ballObjectComponent = ballObject.GetComponent<BallObject>();
            ballObjectComponent.type = levelData.bubbleTypes[i];
            // Add h???t c??c ball ???? t???o ra v??o 1 list
            ballList.Add(ballObjectComponent);

            // Add ball v??o t???ng tube theo tubeIndex
            tubeLayoutComponent.tubeList[tubeIndex].ballObjects.Add(ballObjectComponent);

            // ?????t ballObject v??o v??? tr?? c??c pos trong postList thu???c m???i tube
            ballObject.transform.position =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.position;
            ballObject.transform.localScale = tubeLayoutComponent.GetTubeScale();

            ballObject.transform.rotation =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
        }
    }
    
    public void DoWin()
    {
        // winPanel.SetActive(true);
        Debug.Log("WIN!!!!!");
        StopCoroutine(countdownTimer);
        userCurrentLevel++;
        gameState = GameState.PLAYING;
        remainningTime = 0f;
        listStepMoved.Clear();
        Init();
        txtCurrentLevel.text = "Level: " + userCurrentLevel;
        txtDiffucultyMode.text = "Mode: " + difficulty;
    }
    
    public void OnClickBtnNext()
    {
        gameState = GameState.PLAYING;
        remainningTime = 0f;
        listStepMoved.Clear();
        Init();
        txtCurrentLevel.text = "Level: " + userCurrentLevel;
        txtDiffucultyMode.text = "Mode: " + difficulty;
        
        // winPanel.SetActive(false);
    }
    public void GetLevelDifficulty()
    {
        for (int i = 0; i <= levelConfig.user_level.Count; i++)
        {
            if (userCurrentLevel >= levelConfig.user_level[i].level_from && userCurrentLevel <= levelConfig.user_level[i].level_to)
            {
                int index = Random.Range(0, levelConfig.user_level[i].level_difficulty.Count);

                difficulty = levelConfig.user_level[i].level_difficulty[index];
                GetLevelDifficultyConfig();
                
                txtCurrentLevel.text = "Level: " + userCurrentLevel;
                txtDiffucultyMode.text = "Mode: " + difficulty;
                
                
                Debug.Log(difficulty);
                break;
            }
        }
    }

    public void GetLevelDifficultyConfig()
    {
        for (int i = 0; i <= levelConfig.level_dificulty.Count; i++)
        {
            if (difficulty.Equals(levelConfig.level_dificulty[i].level_difficulty))
            {
                difficultyConfig = levelConfig.level_dificulty[i];
                break;
            }
        }
    }

    public void GetRandomStage()
    {
        randomStagePerDifficulty = Random.Range(1, difficultyConfig.max_level + 1);
        txtRandomStage.text = "Stage: " + randomStagePerDifficulty;
    }
    
    public void GetTimeLimitAtStart()
    {
        maxDuration = difficultyConfig.time;
        remainningTime = maxDuration;
       
        UpdateTimer();
    }

    public void UpdateTimer()
    {
        if (countdownTimer != null)
        {
            StopCoroutine(countdownTimer);
        }
        // float fillAmount = (remainningTime-0.1f) / maxDuration;
        // progressFill.DOFillAmount(fillAmount,0.1f);

        countdownTimer = MonoExtensions.InvokeRepeatingSafe(this, () =>
        {
            remainningTime -= 0.1f;
            float fillAmount = remainningTime / maxDuration;

            // progressFill.fillAmount = remainningTime / maxDuration;
            // float fillAmount = (remainningTime-0.1f) / maxDuration;
            // if (fillAmount < 0) fillAmount = 0;
            
            progressFill.DOFillAmount(fillAmount,0.1f);
            if (remainningTime <= 0)
            {
                gameState = GameState.LOSE;
                DOLose();
                StopCoroutine(countdownTimer);
            }
        }, new WaitForSecondsRealtime(0f), new WaitForSecondsRealtime(0.1f));
    }

    public void DOLose()
    {
        // Show score panel
        // 2 btn: Return Home, Restart (continue playing at Level 1)
        Debug.Log("H???T GI???!");
        losePanel.SetActive(true);
        txtEndScore.text = "End Score: " + currentScore;
    }

    public int AddScore()
    {
        currentScore += difficultyConfig.point_each_tube;
        txtScore.text = "Score: " + currentScore;
        return difficultyConfig.point_each_tube;
    }

    public void OnClickBtnRestart()
    {
        Debug.Log("Restart Game -- Set UserCurrentLevel = 1");
        gameState = GameState.PLAYING;
        losePanel.SetActive(false);
        userCurrentLevel = 1;
        txtCurrentLevel.text = "Level: " + userCurrentLevel;
        currentScore = 0;
        txtScore.text = "Score: " + currentScore;
        Init();
    }

    public void OnClickBtnPause()
    {
        if (gameState != GameState.PLAYING)
        {
            return;
        }
        pausePanel.SetActive(true);
        if (countdownTimer != null)
        {
            StopCoroutine(countdownTimer);
        }
        Time.timeScale = 0;
    }

    public void OnClickBtnResume()
    {
        Time.timeScale = 1;
        UpdateTimer();
        pausePanel.SetActive(false);
    }

    public void OnClickBtnWin()
    {
        DoWin();
    }
    
    public void OnClickBtnLose()
    {
        remainningTime = 0;
        UpdateTimer();
    }
    
    public void OnClickBtnClose()
    {
        Debug.Log("Return to Main Menu");
        
    }
    
    
    
    
    // END CLASS GameManager.cs
}

public enum GameState
{
    PLAYING,
    MOVING,
    WIN,
    LOSE,
}

public class StepMove
{
    public int tubeA = -1;
    public int tubeB = -1;
    public int point = 0;
    public StepMove(int tubeA, int tubeB,int point)
    {
        this.tubeA = tubeA;
        this.tubeB = tubeB;
        this.point = point;
    }
}
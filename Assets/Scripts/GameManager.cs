using System.IO;
using System.Collections;
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
    public Button btnStart;
    public LevelData levelData;
    public GameObject tubeRoot;
    public GameObject itemBalls;
    public GameState state = GameState.PLAYING;

    public List<BallObject> ballList = new List<BallObject>();
    private TubeLayout tubeLayoutComponent;

    public int currentLevel;

    public TubeObject selectedTube = null;

    private void Start()
    {
        btnStart.onClick.AddListener(OnClickButtonStart);
        currentLevel = 13;
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
            Path.Combine("LevelData", LevelDifficulty.BEGINNER.ToString().ToLower(),
                "level_" + currentLevel);
        // load file text leveldata
        TextAsset levelDataFile = Resources.Load<TextAsset>(levelDataPath);
        // deserialize file text vừa load
        LevelData result = JsonConvert.DeserializeObject<LevelData>(levelDataFile.ToString());
        return result;
    }

    private void SetUpTubeLayout()
    {
        if (tubeLayoutComponent != null) Destroy(tubeLayoutComponent.gameObject);
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i].gameObject);
        }

        ballList.Clear();
        // Đường dẫn của prefab
        LevelData data = LoadLevelData();
        string tubeLayoutPath = Path.Combine("Prefabs", "Tubes", "Tube_" + data.numStack);
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
        for (int i = 0; i < data.bubbleTypes.Count; i++)
        {
            int tubeIndex = i / 4;
            int ballIndex = i % 4;

            string ballPath = Path.Combine("Prefabs", "Balls", "Ball" + data.bubbleTypes[i]);
            GameObject ballPrefab = Resources.Load<GameObject>(ballPath);


            GameObject ballObject = Instantiate(ballPrefab,
                itemBalls.transform, false);

            // Lấy component BallOject để add vào 1 list dùng để quản lý
            BallObject ballObjectComponent = ballObject.GetComponent<BallObject>();
            ballObjectComponent.type = data.bubbleTypes[i];
            // Add hết các ball đã tạo ra vào 1 list
            ballList.Add(ballObjectComponent);

            //Add ball vào từng tube theo tubeIndex
            tubeLayoutComponent.tubeList[tubeIndex].ballObjects.Add(ballObjectComponent);

            //Đặt ballobject vào vị trí các pos trong postList
            ballObject.transform.position =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.position;
            ballObject.transform.localScale =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.localScale;
            ballObject.transform.rotation =
                tubeLayoutComponent.tubeList[tubeIndex].posList[ballIndex].transform.rotation;
        }
    }

    public void OnClickTubeObject(TubeObject tubeObject)
    {
        // Case: click vào tube rỗng/tube full ball cùng màu -> k làm gì
        if (tubeObject.IsTubeEmpty() || tubeObject.IsTubeDone())
        {
            Debug.Log("EMPTY----DONE");
            return;
        }

        // Case: click vào tube hợp lệ: gán tube đang click cho SelectedTube,
        if (selectedTube == null)
        {
            Debug.Log("selectedTube = null");
            selectedTube = tubeObject;
            // Thực hiện move ball trên cùng của Tube đang chọn lên vị trí posTop của tube đang chọn
            tubeObject.ballObjects[tubeObject.ballObjects.Count - 1].transform
                .DOMove(tubeObject.posTop.transform.position, 0.5f);
        }
        else
        {
            // Case: Click vào chính ball đang chọn -> move ball xuống
            if (selectedTube == tubeObject)
            {
                tubeObject.posTop.transform
                    .DOMove(tubeObject.ballObjects[tubeObject.ballObjects.Count - 1].transform.position,
                        0.5f);
            }
            else
            {
                Debug.Log("Clicked tube 2");
            }
        }

        


        // Click lại vào tube đang chọn (tube 1): ball đi xuống

        // Click vào tube 2:
        // Case 1: tube 2 rỗng -> move ball từ tube 1 sang tube 2
        // Case 2: tube 2 có ball -> Check xem ball trên cùng của tube 2 có cùng màu với ball được chọn k
        // Nếu cùng màu: check xem tube 2 full chưa, chưa full thì move, full rồi thì k làm gì
    }
}
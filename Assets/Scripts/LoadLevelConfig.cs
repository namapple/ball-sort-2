using GameTown.MiniGame.BallSort;
using Newtonsoft.Json;
using UnityEngine;

public class LoadLevelConfig : MonoBehaviour
{
    public LevelConfig levelConfig = new LevelConfig();

    public LevelConfig LoadConfig()
    {
        TextAsset levelConfigFile = Resources.Load<TextAsset>("levelConfig");

        levelConfig = JsonConvert.DeserializeObject<LevelConfig>(levelConfigFile.text);
        return levelConfig;
    }

    private void Start()
    {
        levelConfig = LoadConfig();
        Debug.Log(levelConfig.user_level[0].level_difficulty[0]);
    }
}
